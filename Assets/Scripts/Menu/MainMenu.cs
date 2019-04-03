using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Menu components")]
    [SerializeField] TextMeshProUGUI deathText;
    [SerializeField] Text startText;
    [SerializeField] private Button startButton;
    
    private GameManager gameManager;
    private MapDrawer mapDrawer;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        mapDrawer = FindObjectOfType<MapDrawer>();
    }

    void Update()
    {
        if (gameManager.gameState == GameManager.GameState.MAINMENU)
        {
            deathText.gameObject.SetActive(false);

            if (gameManager.generationInProgress)
            {
                startText.text = "Loading...";
                startButton.interactable = false;
            }
            else
            {
               startText.text = "Start";
               startButton.interactable = true;
            }
        }

        if (gameManager.gameState == GameManager.GameState.DEATH)
        {
            deathText.gameObject.SetActive(true);
            deathText.text = "You bravely survived " + gameManager.levelNumber + " days.";
            
            if (gameManager.generationInProgress)
            {
                startText.text = "Loading...";
                startButton.interactable = false;
            }
            else
            {
                startText.text = "Retry";
                startButton.interactable = true;
            }
        }
    }

    public void StartGame()
    {
        startText.text = "Loading...";
        startButton.interactable = false;
        
        StartCoroutine(gameManager.GenerateMap());
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
