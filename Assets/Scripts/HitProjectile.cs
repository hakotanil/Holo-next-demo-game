using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitProjectile : MonoBehaviour
{
    SimpleStats thrower;
    [Range(0,50)] public float speed;
    [Range(0,100)]public float Damage;

    [Range(0, 100)] public float maxArmorPen;
    [Range(0, 100)] public float minArmorPen;

    public void Fire(SimpleStats thrower, Vector3 _dir)
    {
        this.thrower = thrower;
        _dir.Normalize();
        Vector3 deviotedDir = new Vector3(NewRatio(_dir.x,20), NewRatio(_dir.y, 20), NewRatio(_dir.z, 20));
        GetComponentInChildren<Rigidbody>().AddForce(deviotedDir * speed, ForceMode.Impulse);
    }
    public void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.contacts[0].otherCollider;
        if (!other.TryGetComponent(out ExpandedStats stat) || stat.teamNumber == thrower.teamNumber)
            return;

        float armorPenRate = Random.Range(minArmorPen, maxArmorPen);
        DamageInfo.TryDealDamage(thrower, other.gameObject.transform, transform.position, Damage, armorPenRate, out _);
        Destroy(gameObject);
    }

    static float NewRatio(float val, float ratio)
    {
        float random = Random.Range(-ratio, ratio);
        val += val * random / 100;
        return val;
    }

    private void OnValidate()
    {
        if (minArmorPen > maxArmorPen)
            minArmorPen = maxArmorPen;
    }
}
