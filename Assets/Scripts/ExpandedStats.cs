using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Base class of every damageable object use for breakable objects, players ....
/// </summary>
public abstract class SimpleStats : MonoBehaviour
{
    public int maxhealth;
    public int maxArmor;
    [NaughtyAttributes.ReadOnly] public float health;   
    [NaughtyAttributes.ReadOnly] public float armor;
    public int teamNumber;

    [HideInInspector] public bool death;
    public abstract WoundInfo TakeDamage(DamageInfo info);
}

public abstract class ExpandedStats : SimpleStats
{
    [NaughtyAttributes.HorizontalLine(5)]
    public Transform rockThrowPoint;
    public float speed;

    [HideInInspector] public LayerMask ignorelayers;

    public virtual void Start()
    {
        ignorelayers =  ~(LayerMask.GetMask("Player") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("MainCapsule"));
        health = maxhealth;
        armor = maxArmor;
    }


    public override WoundInfo TakeDamage(DamageInfo info)
    {
        if (info.damageToBody == 0 || death)
            return new WoundInfo();
        info.HitPoint.y = transform.position.y;

        Vector3 distance = info.HitPoint - transform.position;
        float angle = Vector3.Angle(transform.forward, distance);

        armor -= info.damageToArmor;
        health -= info.damageToBody;
        if (armor < 0)
        {
            health += armor;
            armor = 0;
        }

        // If death 
        if (OnLethalDamage(info))
            return new WoundInfo(info.damageToBody, true);
        else
        {
            Animator anim = GetComponent<Animator>();
            if (angle < 90)
                anim.CrossFade("Hitted_Front", 0.1f, -1, 0);
            else
                anim.CrossFade("Hitted_Back", 0.1f, -1, 0);
        }

        Debug.Log("zırha hasar = " + info.damageToArmor + " bodye hasar = " + info.damageToBody);

        return new WoundInfo(info.damageToBody, false);
    }

    protected virtual bool OnLethalDamage(DamageInfo info)
    {
        if (health <= 0)
        {
            health = 0;
            death = true;

            SetActiveMode(false);
            GameManager.GainScore(info.damageFrom.teamNumber);
            StartCoroutine(Spawn(3));
            return true;
        }
        return false;
    }

    IEnumerator Spawn(int coolDown)
    {
        transform.position = new Vector3(0, -5, 0);
        yield return new WaitForSeconds(coolDown);

        
        NavMesh.SamplePosition(GameManager.teams[teamNumber].teamBase.spawnPoint.transform.position, out NavMeshHit myNavHit, 100, NavMesh.AllAreas);
        transform.position = myNavHit.position;
        death = false;
        health = maxhealth;
        armor = maxArmor;
        SetActiveMode(true);
    }


    public void SetActiveMode(bool open)
    {
        if (GetComponent<Animator>().isActiveAndEnabled == open)
            return;

        Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
        Collider[] cols = GetComponentsInChildren<Collider>();
        for (int i = 1; i < cols.Length; i++)
            cols[i].isTrigger = open;
        
        for (int i = 1; i < rigs.Length; i++)
            rigs[i].isKinematic = open;

        GetComponent<Animator>().enabled = open;
        if(TryGetComponent(out NavMeshAgent agent))
            agent.enabled = open;
        GetComponent<Collider>().enabled = open;
    }
}

/// <summary>
/// carries damage information of the hitted enemy
/// </summary>
public class WoundInfo
{
    public float takenDamage = 0;
    public float curHealth = 0;
    public bool finalBlow = false;
    
    public WoundInfo() {  }
    public WoundInfo(float _takenDamage, bool _finalBlow)
    {
        takenDamage = _takenDamage;
        finalBlow = _finalBlow;
    }
}
public class DamageInfo
{
    public GameObject hittedObj;
    public float damageToBody;
    public float damageToArmor;
    public bonePart hitPart;
    public SimpleStats damageFrom;
    public SimpleStats damageTo;
    public Vector3 HitPoint;

    DamageInfo(SimpleStats from, Transform _to , Vector3 _point, float _damage, float _armorPenRate)
    {
        damageFrom = from;
        damageTo = _to.GetComponent<SimpleStats>();
        HitPoint = _point;
        hittedObj = _to.gameObject;
        damageToBody = _armorPenRate * _damage / 100;
        damageToArmor = _damage - damageToBody;
    }

    /// <summary>
    /// If object damageable return true else false
    /// </summary>
    public static bool TryDealDamage(SimpleStats _from, Transform _to, Vector3 _hitPoint, float _damage, float _armorPenRate ,  out WoundInfo wInfo)
    {
        wInfo = new WoundInfo();
        if (!_to.TryGetComponent(out SimpleStats o))
            return false;

        wInfo = o.TakeDamage(new DamageInfo(_from, _to , _hitPoint, _damage, _armorPenRate));
        return true;
    }



    // Can Used Later for area damage

    /*public static List<WoundInfo> DealDamageToArea(Vector3 _originPoint, float _range, float _damage, bool _push)
    {
        List<WoundInfo> wInfos = new List<WoundInfo>();
        Enemy[] allEnemies = Enemy.LivingEnemies.ToArray();

        foreach (var tarEnemy in allEnemies)
        {
            Vector3 EnemyPos = tarEnemy.transform.position;
            EnemyPos.y = _originPoint.y;

            float distance = Vector3.Distance(_originPoint, EnemyPos);
            Vector3 dir = EnemyPos - _originPoint;

            if (distance > _range)
                continue;
            foreach (var item in tarEnemy.bones)
            {
                float distanceToBone = Vector3.Distance(_originPoint, item.transform.position);
                Vector3 dirToBone = (item.transform.position - _originPoint).normalized;
                Debug.DrawRay(_originPoint, dirToBone * distanceToBone, Color.red, 100);
                if (!Physics.Raycast(_originPoint, dirToBone, distanceToBone + 0.2f, ~LayerMask.GetMask("MainCapsule", "Enemy", "Player")))
                {
                    Debug.Log("bomb hitted something");
                    float ratio = Mathf.Clamp01(1 - distance / _range);
                    if (DamageInfo.TryDealDamage(item.transform, item.transform.position, _damage * ratio, out WoundInfo wInfo))
                    {
                        wInfos.Add(wInfo);
                        if (_push)
                            DamageInfo.TryPush(item.gameObject, (_originPoint - tarEnemy.anim.bodyPosition).normalized, ratio * _damage * 100);
                    }
                    break;
                }
            }
        }
        return wInfos;
    }*/

}

