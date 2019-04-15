using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterLevelMenu : MonoBehaviour
{
    [Header("Player inventory components")] 
    [SerializeField] private TextMeshProUGUI playerScrapAmount;
    [SerializeField] private TextMeshProUGUI playerFoodAmount;
    
    [Header("Player stats components")]
    [SerializeField] private TextMeshProUGUI playerArmorAmount;
    [SerializeField] private TextMeshProUGUI playerLifeAmount;

    [Header("Inter-Level components")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Text textLife;
    [SerializeField] private TextMeshProUGUI textLifeCost;
    [SerializeField] private Text textArmor;
    [SerializeField] private TextMeshProUGUI textArmorCost;
    [SerializeField] private Text textNextLevel;

    [Header("End level constraints")] 
    [SerializeField] public int foodRequirement;
    
    [Header("Cost values")]
    [SerializeField] int addArmorCost;
    [SerializeField] int addLifeCost;

    [Header("Increasing stats by")]
    [SerializeField] private int addArmorAmount;
    [SerializeField] private int addLifeAmount;

    PlayerLife playerLife;
    PlayerInventory playerInventory;
    private GameManager gameManager;

    private bool hasAppliedRequirement = false;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        playerLife = FindObjectOfType<PlayerLife>();
        playerInventory = FindObjectOfType<PlayerInventory>();
    }

    void Update()
    {
        if (gameManager.gameState == GameManager.GameState.INTERLEVEL)
        {
            if(playerInventory == null)
                playerInventory = FindObjectOfType<PlayerInventory>();

            if (playerLife == null)
                playerLife = FindObjectOfType<PlayerLife>();
            
            if (gameManager.generationInProgress)
            {
                textNextLevel.text = "Loading..";
                nextLevelButton.interactable = false;
            }
            else
            {
                textNextLevel.text = "Next level..";
                nextLevelButton.interactable = true;
            }

            if (playerLife.activeArmor >= playerLife.maxArmor)
                textArmor.text = "Increase armor";
            else
                textArmor.text = "Repair armor";

            if (playerLife.activeLife >= playerLife.maxLife)
                textLife.text = "Increase life";
            else
                textLife.text = "Heal";

            textArmorCost.text = "Scrap cost : " + addArmorCost;
            textLifeCost.text = "Food cost : " + addLifeCost;
            
            playerArmorAmount.text = playerLife.activeArmor + "/" + playerLife.maxArmor;
            playerLifeAmount.text = playerLife.activeLife + "/" + playerLife.maxLife;

            playerScrapAmount.text = playerInventory.scrap.ToString();
            playerFoodAmount.text = playerInventory.food.ToString();
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

    public void ApplyEndLevelRequirement()
    {
        // Check if the requirement has aleady been applied
        if (hasAppliedRequirement)
            return;

        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
        
        // Check if the player has not enough food
        if (playerInventory.food < foodRequirement)
        {
            playerLife.activeLife -= (foodRequirement - playerInventory.food);
            playerInventory.food = 0;
            
            // Check if the player is still alive
            if (playerLife.activeLife <= 0)
            {
                gameManager.gameState = GameManager.GameState.DEATH;
                return;
            }
        }
        else
        {
            playerInventory.food -= foodRequirement;
        }

        hasAppliedRequirement = true;
        
        gameManager.gameState = GameManager.GameState.INTERLEVEL;
    }
    
    public void NextLevel()
    {
        hasAppliedRequirement = false;
        StartCoroutine(gameManager.GenerateMap());
    }
}
