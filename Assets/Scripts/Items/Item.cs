using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] public Items items;

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
            switch (items.type)
            {
                case Items.itemType.FOOD:
                    playerInventory.AddFood(items.amount);
                    break;
                default:
                    playerInventory.AddScrap(items.amount);
                    break;
            }

            Destroy(gameObject);
        }
    }
}
