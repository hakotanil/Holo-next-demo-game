using UnityEngine;

public partial class StateManager : ExpandedStats
{
    [NaughtyAttributes.ReadOnly] public float currentSpeed;
    [HideInInspector] public InputHandler inputs;
    [HideInInspector] public Animator anim;

    [HideInInspector]public Vector3 aimposition;

    void Awake()
    {
        inputs = GetComponentInParent<InputHandler>();
        anim = GetComponent<Animator>();
    }
    void FixedUpdate()
    {
        Movement();
    }

    public override WoundInfo TakeDamage(DamageInfo info)
    {
        WoundInfo baseResult = base.TakeDamage(info);
        if (death)
            return baseResult;

        StartCoroutine(InputHandler.instance.cam.BackRecoil(0.25f, -0.011f));
        return new WoundInfo(info.damageToBody, death);


    }

    #region Anim Methods

    void ANIM_Throw()
    {
        Transform throwable = ResourceManager.GetThrowable("Rock").transform;
        throwable.position = rockThrowPoint.position;
        throwable.GetComponent<HitProjectile>().Fire(this, inputs.cam.mainCamera.forward);
    }

    #endregion



    // looking move position and setting animations
    void Movement()
    {
        Vector3 targetdir = inputs.movedir;
        targetdir.y = 0;
        anim.SetFloat("Horizontal", inputs.horizontal);
        anim.SetFloat("Vertical", inputs.vertical);
        currentSpeed = speed;

        if (inputs.cam.aim)
            currentSpeed = speed - 0.5f ;
        else
            anim.SetFloat("Vertical" , Mathf.Lerp(anim.GetFloat("Vertical"), inputs.moveamount / 2, Time.deltaTime * 4));

        if (InputHandler.instance.moveamount > 0 && !inputs.movingToWall)
            anim.SetBool("Move", true);
        else
            anim.SetBool("Move", false);


        FreeMove();

        void FreeMove()
        {
            Vector3 v = inputs.cam.pivot.transform.forward;
            v.y = 0;
            inputs.movedir = (v).normalized;

            targetdir = inputs.movedir;
            if (targetdir == Vector3.zero)
                targetdir = transform.forward;
            Quaternion tr = Quaternion.LookRotation(targetdir);
            Quaternion targetrotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * 8 / Time.timeScale);
            transform.rotation = targetrotation;
        }
    }

}