using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            if (gameManager.gameState == GameManager.GameState.PAUSE)
            {
                gameManager.gameState = GameManager.GameState.INGAMEDAY;
                return;
            }

            if (gameManager.gameRunning)
                gameManager.gameState = GameManager.GameState.PAUSE;
        }
    }

    public void ShowMainMenu()
    {
        gameManager.gameState = GameManager.GameState.MAINMENU;
    }

    public void Resume()
    {
        gameManager.gameState = GameManager.GameState.INGAMEDAY;
    }
}
