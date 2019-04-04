using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] Image imgHealth;
    [SerializeField] Image imgShield;
    [SerializeField] private TextMeshProUGUI scrapText;
    [SerializeField] private TextMeshProUGUI foodText;

    [Header("Other attributs")]
    [SerializeField] float fadeDuration;
    [SerializeField] float fillSpeed;

    GameObject player;
    PlayerLife playerStats;
    private PlayerInventory playerInventory;

    int previousLife = 0;
    private int previousScrap = 0;
    private int previousFood = 0;
    int previousShield = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GetComponent<PlayerLife>();
        playerInventory = GetComponent<PlayerInventory>();
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
        foodText.text = playerInventory.food.ToString();

        previousFood = playerInventory.food;
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
