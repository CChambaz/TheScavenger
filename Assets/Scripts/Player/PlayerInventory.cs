using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [NonSerialized] public int scrap = 0;
    [NonSerialized] public int food = 0;

    public void AddScrap(int amount)
    {
        scrap += amount;
    }

    public void AddFood(int amount)
    {
        food += amount;
    }
}
