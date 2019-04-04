using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] public ItemsData itemsData;

    private PlayerInventory playerInventory;

    // Start is called before the first frame update
    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            switch (itemsData.type)
            {
                case ItemsData.itemType.FOOD:
                    playerInventory.AddFood(itemsData.amount);
                    break;
                default:
                    playerInventory.AddScrap(itemsData.amount);
                    break;
            }

            Destroy(gameObject);
        }
    }
}
