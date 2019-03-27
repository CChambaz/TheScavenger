using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRage : MonoBehaviour
{
    [Header("Rage attribut")]
    [SerializeField] float energyUseRate;
    [SerializeField] int rageMultiplier;
    [SerializeField] public float maxEnergy;

    public int activeRageMultiplier = 1;

    public float actualEnergy;

    // Start is called before the first frame update
    void Start()
    {
        actualEnergy = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.E))
            EnableRage();

        if (activeRageMultiplier > 1)
            CheckRageStatus();
    }

    public void AddEnergy(float energyAdded)
    {
        actualEnergy += energyAdded;
    }

    void EnableRage()
    {
        if (actualEnergy > 0)
        {
            activeRageMultiplier = rageMultiplier;
        }
    }

    void CheckRageStatus()
    {
        if (actualEnergy <= 0)
        {
            activeRageMultiplier = 1;
            actualEnergy = 0;
            return;
        }

        if (actualEnergy > 0)
            actualEnergy -= energyUseRate;        
    }
}
