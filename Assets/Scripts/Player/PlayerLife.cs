using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    [Header("Attribut")]
    [SerializeField] int baseLife;
    [SerializeField] int baseArmor;
    [SerializeField] public int maxTotalArmor;

    [NonSerialized] public int maxLife;
    [NonSerialized] public int activeLife;

    [NonSerialized] public int maxArmor;
    [NonSerialized] public int activeArmor;

    // Start is called before the first frame update
    void Start()
    {
        maxLife = baseLife;
        activeLife = maxLife;

        maxArmor = baseArmor;
        activeArmor = maxArmor;
    }

    public void ChangeLife(int lifeAdded)
    {
        // Take damage
        if (lifeAdded < 0)
        {
            // Damage on the armor
            if(activeArmor > 0)
            {
                activeArmor--;
                return;
            }

            // Damage on life
            activeLife += lifeAdded;
            return;
        }

        // Increase health
        if (activeLife == maxLife)
        {
            // Increase max life
            maxLife = activeLife + lifeAdded;
            activeLife = maxLife;
        }
        else
            activeLife += lifeAdded;

        if (activeLife > maxLife)
            activeLife = maxLife;
    }

    public void IncreaseArmor(int armorAdded, bool canIncreaseMaxArmor)
    {
        if(activeArmor >= maxArmor && maxArmor < maxTotalArmor && canIncreaseMaxArmor)
        {
            maxArmor += armorAdded;
            activeArmor = maxArmor;
            return;
        }

        if (maxArmor == maxTotalArmor && canIncreaseMaxArmor)
            return;

        if (activeArmor == maxArmor)
            return;

        if (activeArmor + armorAdded > maxArmor)
            activeArmor = maxArmor;
        else
            activeArmor += armorAdded;
    }
}
