using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    private Transform playerTransform;

    private bool isOpen = false;

    private GameManager gameManager;
    private Animator animController;

    private bool isPlayerOnSpawn = false;
    
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        animController = GetComponent<Animator>();
        playerTransform = gameManager.GetPlayerTranform();
    }

    private void Update()
    {
        if (isPlayerOnSpawn && Input.GetKey(KeyCode.E))
            EndLevel();
    }

    public void SpawnPlayer()
    {
        playerTransform.position = transform.position;
    }

    void EndLevel()
    {
        gameManager.gameState = GameManager.GameState.INTERLEVEL;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            isPlayerOnSpawn = true;
            animController.SetBool("isPlayerOnManhole", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            isPlayerOnSpawn = false;
            animController.SetBool("isPlayerOnManhole", false);
        }
    }
}
