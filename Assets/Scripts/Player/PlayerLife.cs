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

    [NonSerialized] public int maxLife;
    [NonSerialized] public int activeLife;

    [NonSerialized] public int maxArmor;
    [NonSerialized] public int activeArmor;
    
    // Start is called before the first frame update
    void Start()
    {
        ResetLife();
    }

    public void ResetLife()
    {
        maxLife = baseLife;
        activeLife = maxLife;

        maxArmor = baseArmor;
        activeArmor = maxArmor;
    }
    
    public void ChangeLife(int lifeAdded)
    {
        // Increase health
        if (activeLife == maxLife)
        {
            // Increase max life
            maxLife = activeLife + lifeAdded;
            activeLife = maxLife;
            return;
        }

        if (activeLife + maxLife > maxLife)
            activeLife = maxLife;
        else
            activeLife += lifeAdded;
    }

    public void IncreaseArmor(int armorAdded)
    {
        if(activeArmor == maxArmor)
        {
            maxArmor += armorAdded;
            activeArmor = maxArmor;
            return;
        }

        if (activeArmor + armorAdded > maxArmor)
            activeArmor = maxArmor;
        else
            activeArmor += armorAdded;
    }

    public bool ApplyDamage(int amount)
    {
        // Apply damage to the armor
        if (activeArmor > 0)
        {
            activeArmor -= amount;

            if (activeArmor < 0)
                activeArmor = 0;

            return false;
        }

        activeLife -= amount;

        if (activeLife <= 0)
            return true;
        
        return false;
    }
}
