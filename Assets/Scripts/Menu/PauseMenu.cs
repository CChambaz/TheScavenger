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
        if (gameManager.gameState == GameManager.GameState.PAUSE && Input.GetKey(KeyCode.Escape))
            gameManager.gameState = GameManager.GameState.INGAME;
    }

    public void ShowMainMenu()
    {
        gameManager.gameState = GameManager.GameState.MAINMENU;
    }

    public void Resume()
    {
        gameManager.gameState = GameManager.GameState.INGAME;
    }
}
