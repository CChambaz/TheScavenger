using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("HUD Components")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] Image imgHealth;
    [SerializeField] Image imgShield;
    [SerializeField] private TextMeshProUGUI scrapText;
    [SerializeField] private TextMeshProUGUI foodText;

    [Header("Compass components")] 
    [SerializeField] private Transform compassPivotPoint;
    [SerializeField] private float compassOffset;

    [Header("Time left component")] 
    [SerializeField] private TextMeshProUGUI timeLeftText;
    [SerializeField] private GameObject timeLeftContainer;
    
    [Header("Fade attributs")]
    [SerializeField] float fadeDuration;
    [SerializeField] float fillSpeed;

    Transform playerTransform;
    private Transform manholeTransform;
    PlayerLife playerStats;
    private PlayerInventory playerInventory;
    private GameManager gameManager;
    private InterLevelMenu interLevelMenu;

    int previousLife = -1;
    private int previousScrap = -1;
    private int previousFood = -1;
    int previousShield = -1;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GetComponent<PlayerLife>();
        playerInventory = GetComponent<PlayerInventory>();
        playerTransform = GetComponent<Transform>();
        manholeTransform = FindObjectOfType<PlayerSpawn>().GetComponent<Transform>();
        gameManager = FindObjectOfType<GameManager>();
        interLevelMenu = FindObjectOfType<InterLevelMenu>();
        uiCanvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (previousLife != playerStats.activeLife)
            UpdateLife();

        if (previousShield != playerStats.activeArmor)
            UpdateShield();

        if(previousScrap != playerInventory.scrap)
            UpdateScrap();

        if (previousFood != playerInventory.food)
            UpdateFood();
        
        if(gameManager.gameState == GameManager.GameState.INGAMEDAY || timeLeftContainer.activeSelf)
            UpdateTimeLeft();
        
        UpdateCompass();
    }

    void UpdateLife()
    {
        float actualLife = (float)playerStats.activeLife;
        float totalLife= (float)playerStats.maxLife;

        StartCoroutine(Fill(imgHealth, actualLife / totalLife));

        previousLife = (int)actualLife;
    }

    void UpdateShield()
    {
        float actualShield = (float)playerStats.activeArmor;
        float totalShield = (float)playerStats.maxArmor;

        StartCoroutine(Fill(imgShield, actualShield / totalShield));

        previousShield = (int)actualShield;
    }

    void UpdateScrap()
    {
        scrapText.text = playerInventory.scrap.ToString();

        previousScrap = playerInventory.scrap;
    }

    void UpdateFood()
    {
        foodText.text = playerInventory.food.ToString() + "/" + interLevelMenu.foodRequirement;

        previousFood = playerInventory.food;
    }

    void UpdateCompass()
    {
        Vector2 playerToManhole = (manholeTransform.position - playerTransform.position);

        compassPivotPoint.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(playerToManhole.y, playerToManhole.x) * Mathf.Rad2Deg) + compassOffset);
    }

    void UpdateTimeLeft()
    {
        timeLeftText.text = ((int) (gameManager.dayDuration - gameManager.actualDayDuration)).ToString();
        
        if(gameManager.gameState == GameManager.GameState.INGAMENIGHT)
            timeLeftContainer.SetActive(false);
        else
            timeLeftContainer.SetActive(true);
    }
    
    IEnumerator Fill(Image imageToFill, float fillGoal)
    {
        while (imageToFill.fillAmount != fillGoal)
        {
            imageToFill.fillAmount = Mathf.Lerp(imageToFill.fillAmount, fillGoal, fillSpeed);

            if(imageToFill.fillAmount < 0)
                imageToFill.fillAmount = 0;
            else if(imageToFill.fillAmount > 1)
                imageToFill.fillAmount = 1;

            yield return new WaitForEndOfFrame();
        }
    }
}
