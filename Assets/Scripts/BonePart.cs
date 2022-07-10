using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonePart : MonoBehaviour
{
    [NaughtyAttributes.ReadOnly]public Vector3 originalPoint;
    [NaughtyAttributes.ReadOnly]public Quaternion originalRot;
    [Range(0, 10)] public float damageMultiplier = 1;
    public bonePart part;
}
public enum bonePart
{
    head,
    body,
    arm,
    leg,
    extra,
}
