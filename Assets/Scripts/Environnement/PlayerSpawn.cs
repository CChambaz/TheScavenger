using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    private Transform playerTransform;

    private bool isOpen = false;

    private GameManager gameManager;
    private Animator animController;
    private InterLevelMenu interLevelMenu;

    private bool isPlayerOnSpawn = false;

    private void Awake()
    {
        animController = GetComponent<Animator>();
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        playerTransform = gameManager.GetPlayerTranform();
        interLevelMenu = FindObjectOfType<InterLevelMenu>();
    }

    private void Update()
    {
        if ((gameManager.gameState == GameManager.GameState.INGAMEDAY || gameManager.gameState == GameManager.GameState.INGAMENIGHT) && isPlayerOnSpawn && Input.GetKey(KeyCode.E))
        {
            EndLevel();
        }
    }

    public void SpawnPlayer()
    {
        playerTransform.position = transform.position;
    }

    void EndLevel()
    {
        interLevelMenu.ApplyEndLevelRequirement();
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
