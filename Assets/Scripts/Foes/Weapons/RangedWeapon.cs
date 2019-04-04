using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Foe/Ranged weapon")]
public class RangedWeapon : ScriptableObject
{
    [SerializeField] public GameObject projectile;
    [SerializeField] public float projectileSpeed;
    [SerializeField] public float[] shootAngles;
}
