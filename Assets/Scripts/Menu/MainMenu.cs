using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu components")]
    [SerializeField] GameObject deathText;
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
            deathText.SetActive(false);

            if (gameManager.mapGenerator.mapGenerationJob.isRunning)
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
            deathText.SetActive(true);

            if (gameManager.mapGenerator.mapGenerationJob.isRunning)
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
