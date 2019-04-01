using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterLevelMenu : MonoBehaviour
{
    [Header("Inter-Level components")]
    [SerializeField] Image increaseLife;
    [SerializeField] Image increaseArmor;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Text textLife;
    [SerializeField] private Text textArmor;
    [SerializeField] private Text textNextLevel;

    [Header("Cost values")]
    [SerializeField] int addArmorCost;
    [SerializeField] int addLifeCost;

    [Header("Increasing stats by")]
    [SerializeField] private int addArmorAmount;
    [SerializeField] private int addLifeAmount;

    PlayerLife playerLife;
    PlayerInventory playerInventory;
    private GameManager gameManager;
    private MapDrawer mapDrawer;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        playerLife = FindObjectOfType<PlayerLife>();
        playerInventory = FindObjectOfType<PlayerInventory>();
        mapDrawer = FindObjectOfType<MapDrawer>();
    }

    void Update()
    {
        if (gameManager.gameState == GameManager.GameState.INTERLEVEL)
        {
            if(playerInventory == null)
                playerInventory = FindObjectOfType<PlayerInventory>();

            if (playerLife == null)
                playerLife = FindObjectOfType<PlayerLife>();
            
            /*if (mapGenerator.isRunning)
            {
                textNextLevel.text = "Loading..";
                nextLevelButton.interactable = false;
            }
            else
            {*/
                textNextLevel.text = "Next level..";
                nextLevelButton.interactable = true;
            //}

            if (playerLife.activeArmor >= playerLife.maxArmor)
                textArmor.text = "Increase armor";
            else
                textArmor.text = "Repair armor";

            if (playerLife.activeLife >= playerLife.maxLife)
                textLife.text = "Increase life";
            else
                textLife.text = "Heal";
        }
    }

    public void AddArmor()
    {
        if(playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();

        if (playerLife == null)
            playerLife = FindObjectOfType<PlayerLife>();
        
        if (playerInventory.scrap >= addArmorCost)
        {
            playerLife.IncreaseArmor(addArmorAmount);
            playerInventory.AddScrap(-addArmorCost);
        }
    }

    public void AddLife()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();

        if (playerLife == null)
            playerLife = FindObjectOfType<PlayerLife>();
        
        if (playerInventory.food >= addLifeCost)
        {
            playerLife.ChangeLife(addLifeAmount);
            playerInventory.AddFood(-addLifeCost);
        }
    }

    public void NextLevel()
    {
        StartCoroutine(gameManager.GenerateMap());
    }
}
