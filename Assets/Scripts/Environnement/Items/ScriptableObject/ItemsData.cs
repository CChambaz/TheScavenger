using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items")]
public class ItemsData : ScriptableObject
{
    public enum itemType
    {
        FOOD,
        SCRAP
    }

    [SerializeField] public itemType type;
    [SerializeField] public int amount;
}
