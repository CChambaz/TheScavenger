using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private RangedWeapon rangedWeapon;
    
    public void Shoot(Vector3 target)
    {
        Vector2 vecToTarget = target - transform.position;
        
        // Check if weapon shot only one projectile
        if (rangedWeapon.shootAngles.Length == 1)
        {
            GameObject projectile = Instantiate(rangedWeapon.projectile, transform.position, LookAt2D(vecToTarget, 0f));

            projectile.GetComponent<Projectile>().velocity = rangedWeapon.projectileSpeed;
        }
        else
        {
            for (int i = 0; i < rangedWeapon.shootAngles.Length; i++)
            {
                GameObject projectile = Instantiate(rangedWeapon.projectile, transform.position, LookAt2D(vecToTarget, rangedWeapon.shootAngles[i]));

                projectile.GetComponent<Projectile>().velocity = rangedWeapon.projectileSpeed;
            }
        }
    }
    
    Quaternion LookAt2D(Vector2 forward, float offset)
    {
        return Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg) + offset);
    }
}
