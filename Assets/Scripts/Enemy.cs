using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody), typeof(Animator))]
public class Enemy : ExpandedStats
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public NavMeshAgent nav;
    


    [SerializeReference]
    public List<Attack> attacks = new List<Attack>();
    Attack curattack;
    [Header("Enemy")]
    public Transform HeadRayPoint;
    public States state;

    [Range(0, 180)] public int viewAngle = 30;
    [Range(0, 100)] public int viewDistance = 25;


    UpdateVariables updateVariables;
    AnimatorParameters animatorParameters;

    [NaughtyAttributes.ReadOnly] public List<BonePart> bones;

    public bool CanMove => !(death
        || state == States.Ragdoll);

    public bool OnChasing => state == States.Chasing
        || state == States.LockedChase
        || state == States.LosingChase;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();

        updateVariables = new UpdateVariables(transform , nav);
        animatorParameters = new AnimatorParameters(anim);
        
    }
    public override void Start()
    {
        base.Start();
    }
    protected virtual void Update()
    {
        if (!CanMove)
            return;

        updateVariables.Update();

        SearchPlayer();
        RandomPatrol();
        CalculateAutoRotate();
        CalculateMovementAnimation();
        Attack();

        void Attack()
        {
            if (state != States.Chasing || (state == States.Chasing && animatorParameters.GetBool("Hitting")))
                return;
            curattack = null;
            SetNewAttack();
            if(curattack == null)
                return;

            animatorParameters.SetBool("Hitting", true);
            anim.CrossFade(curattack.attackName, 0.2f);

        }
        void SearchPlayer()
        {
            if (updateVariables.canSeeTimer < 0.4f)
                return;
            updateVariables.canSeeTimer = 0;

            Vector3 targetDir = (InputHandler.instance.state.transform.position + Vector3.up) - HeadRayPoint.position;
            float angle = Vector3.Angle(targetDir, HeadRayPoint.forward);

            if (angle < viewAngle)
            {
                Debug.DrawRay(HeadRayPoint.position, targetDir * viewDistance, Color.red, .01f, false);
                if (Physics.Raycast(HeadRayPoint.position, targetDir, out RaycastHit hit, viewDistance , ~LayerMask.GetMask("Enemy" , "MaınCapsule")) && hit.transform.gameObject.CompareTag("Player"))
                {
                    updateVariables.targetStats = InputHandler.instance.stat;
                    state = States.Chasing;
                }
                else
                    LoseFocus();
            }
            else
                LoseFocus();

            if (state == States.Chasing && updateVariables.navMeshTimer > 1)
            {
                nav.SetDestination(InputHandler.instance.state.transform.position);
                updateVariables.navMeshTimer = 0;
            }

            void LoseFocus()
            {
                if (state == States.Chasing)
                {
                    state = States.LosingChase;
                    StartCoroutine(SearchPlayer());

                    IEnumerator SearchPlayer()
                    {
                        float passedTime = 0;
                        yield return new WaitWhile(() => { passedTime += Time.deltaTime; return passedTime < 2 && state != States.Chasing; });
                        if (state == States.LosingChase)
                        {
                            nav.SetDestination(InputHandler.instance.state.transform.position);
                            state = States.Patrolling;
                        }
                    }
                }
            }

        }
        void CalculateMovementAnimation()
        {

            bool OutStopDistance = updateVariables.distanceToTarget > (state != States.Patrolling ? 1.2f : 0);
            bool canMoveInAttack = !animatorParameters.GetBool("Hitting") || curattack.states.HasFlag(global::Attack.AttackStates.canMove);

            float tempVertical = (state == States.Patrolling || OnChasing)
                && canMoveInAttack
                && OutStopDistance ? 1 : 0;

            nav.isStopped = tempVertical == 0;

            animatorParameters.SetFloat("Vertical", Mathf.MoveTowards(animatorParameters.GetFloat("Vertical"), tempVertical, Time.deltaTime));
        }
        void CalculateAutoRotate()
        {
            if (updateVariables.angleToTarget > 5)
            {
                Quaternion tarRot = Quaternion.LookRotation(updateVariables.dirToTarget, Vector3.up);
                tarRot.x = transform.rotation.x;
                tarRot.z = transform.rotation.z;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, tarRot, Time.deltaTime * 90);
            }
        }
        void RandomPatrol()
        {

            if (state == States.Patrolling && nav.remainingDistance < 0.5f && !nav.pathPending)
                state = States.Patroller;

            if (state == States.Patroller)
            {
                Vector3 point = transform.position + Random.insideUnitSphere * 7;
                Debug.DrawRay(point, Vector3.down * 2, Color.red, 1);
                if (state == States.Patroller && NavMesh.SamplePosition(point, out NavMeshHit myNavHit, 100, NavMesh.AllAreas) && myNavHit.distance > 0.8f)
                {
                    
                    nav.SetDestination(myNavHit.position);
                    if(nav.pathStatus == NavMeshPathStatus.PathPartial)
                        Debug.DrawRay(myNavHit.position, Vector3.down * 2, Color.red, 10);
                    else
                        Debug.DrawRay(myNavHit.position, Vector3.down * 2, Color.green, 100);
                    state = States.Patrolling;
                }
            }

        }
    }

    public override WoundInfo TakeDamage(DamageInfo info)
    {
        WoundInfo baseResult = base.TakeDamage(info);
        if (death)
            return baseResult;

        Alert(InputHandler.instance.transform.position);
        state = States.Chasing;
        updateVariables.targetStats = info.damageFrom as ExpandedStats;

        return new WoundInfo(info.damageToBody, false);
    }

    public void Alert(Vector3 Position)
    {
        if (state == States.Ragdoll || OnChasing)
            return;


        state = States.Patrolling;
        nav.SetDestination(Position);
    }


    void SetNewAttack()
    {
        List<Attack> possibleAttacks = new List<Attack>();
        foreach (var attack in attacks)
        {
            if (updateVariables.distanceToPlayer < attack.maxDistance && updateVariables.distanceToPlayer > attack.minDistance && updateVariables.angleToPlayer < 70)
                possibleAttacks.Add(attack);
        }
        if (possibleAttacks.Count > 1)
        {
            int rand = UnityEngine.Random.Range(0, possibleAttacks.Count);
            curattack = attacks[rand];
        }
        else if (possibleAttacks.Count == 1)
        {
            curattack = possibleAttacks[0];
        }
    }

    #region IK
    float look_weight;
    readonly float body_weight = 0.1f;
    void OnAnimatorIK()
    {
        if (!OnChasing)
        {
            look_weight = Mathf.MoveTowards(look_weight, 0, Time.deltaTime * 3);
            anim.SetLookAtWeight(look_weight, body_weight, 0.2f, 0, 1);
            return;
        }

        look_weight = Mathf.MoveTowards(look_weight, 1, Time.deltaTime * 3);
        anim.SetLookAtPosition(InputHandler.instance.state.transform.position + Vector3.up * 1.6f);
        anim.SetLookAtWeight(look_weight, body_weight, 1, 1, 0.6f);

    }
    void UpdateIK(AvatarIKGoal goal, Vector3 pos, float w)
    {
        anim.SetIKPositionWeight(goal, w);
        anim.SetIKPosition(goal, pos);
    }
    #endregion

    #region Animation, Physics
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Alert(InputHandler.instance.transform.position);
    }
    public void ANIM_Throw()
    {
        Transform throwable = ResourceManager.GetThrowable("Rock").transform;
        throwable.position = rockThrowPoint.position;
        throwable.GetComponent<HitProjectile>().Fire(this, updateVariables.targetStats.transform.position - rockThrowPoint.position);
    }

    #endregion
    public enum States : byte
    {
        Idle,
        Patroller,
        Patrolling,
        Chasing,
        LockedChase,
        LosingChase,
        Ragdoll,
    }


    // New Technique for Performance ( NOT TESTED YET )
    internal class UpdateVariables
    {
        readonly Transform transform;
        readonly NavMeshAgent nav;

        // TO PLAYER
        internal Vector3 PlayerPos => InputHandler.instance.state.transform.position;
        internal Vector3 PlayerOrigin => InputHandler.instance.state.transform.position + Vector3.up * 1.5f;

        internal float distanceToPlayer;
        internal float angleToPlayer;

        // TO TARGET
        public ExpandedStats targetStats;
        internal float distanceToTarget;
        internal Vector3 dirToTarget;
        internal float angleToTarget;
        internal float signedAngleToTarget;

        // TIMERS
        internal float canSeeTimer;
        internal float navMeshTimer;

        internal UpdateVariables(Transform t, NavMeshAgent n){transform = t; nav = n; }

        internal void Update()
        {
            angleToPlayer = Vector3.Angle((PlayerPos - transform.position).normalized, transform.forward);
            distanceToPlayer = Vector3.Distance(transform.position, PlayerPos);

            dirToTarget =  (nav.steeringTarget - transform.position).normalized;
            distanceToTarget = nav.remainingDistance;
            angleToTarget = Vector3.Angle(dirToTarget, transform.forward);
            signedAngleToTarget = -Vector3.SignedAngle(dirToTarget, transform.forward, Vector3.up);

            canSeeTimer += Time.deltaTime;
            navMeshTimer += Time.deltaTime;
        }
    }
    [System.Serializable]

    // New Technique for Performance ( NOT TESTED YET )
    public class AnimatorParameters
    {
        readonly Animator anim;
        public Dictionary<string, object> values = new Dictionary<string, object>();
        public AnimatorParameters(Animator t)
        {
            anim = t;
            foreach (var item in anim.GetBehaviours<OnMotion>())
                item.SetParameters(this);
            foreach (var item in anim.parameters)
                if(item.type == AnimatorControllerParameterType.Float)
                    values.Add(item.name, item.defaultFloat);
                else
                    values.Add(item.name, item.defaultBool);
        }

        internal void SetFloat(string name , float val)
        {
            anim.SetFloat(name, val);
            values[name] = val; 
        }
        internal void SetBool(string name, bool val)
        {
            anim.SetBool(name, val);
            values[name] = val;
        }
        internal float GetFloat(string name) => (float)values[name];
        internal bool GetBool(string name) => (bool)values[name];
    }
    [ExecuteInEditMode]
    private void OnValidate()
    {
        UpdateAttackTypes();


        void UpdateAttackTypes()
        {
            if (attacks.Count == 0)
                attacks.Add(new Attack());
            for (int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i] == null)
                {
                    attacks[i] = new Attack();
                    return;
                }
                switch (attacks[i].attackType)
                {

                    case Attack.AttackType.Ranged:
                        if (attacks[i].GetType() != typeof(RangedAttack))
                            attacks[i] = new RangedAttack();
                        Debug.Log("ranged atack ayarlandı");
                        break;
                    case Attack.AttackType.Melee:
                        if (attacks[i].GetType() != typeof(Attack))
                            attacks[i] = new Attack();
                        break;
                }
            }
        }
    }

}


// Changable attack types based on selected type
[System.Serializable]
public class Attack
{
    public AttackType attackType;
    public AttackStates states;
    public byte damage;
    public string attackName;
    public float maxDistance;
    public float minDistance;
    
    public enum AttackType
    {
        Melee,
        Ranged,
        Other,
    }

    public enum AttackStates
    {
        none = 0,
        canSprint = 1,
        canMove = 2,
        canRotate = 4,
    }
}
public class RangedAttack : Attack
{
    public GameObject projectile;
    public Transform firePoint;
    public RangedAttack()
    {
        attackType = AttackType.Ranged;
    }
}


