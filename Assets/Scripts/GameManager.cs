using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEditor;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [Header("Menu canvas group")]
    [SerializeField] public CanvasGroup mainMenuCanvas;
    [SerializeField] public CanvasGroup interCanvas;
    [SerializeField] public CanvasGroup pauseCanvas;

    [Header("Fade attributs")]
    [SerializeField] float fadeSpeed;
    [SerializeField] float fadeApproximation;

    [Header("Game objects")]
    [SerializeField] private GameObject playerSpawnPrefab;
    [SerializeField] private GameObject playerPrefab;

    [Header("Parameters")] 
    [SerializeField] public MapParameters parameters;
    
    public enum GameState
    {
        MAINMENU,
        INGAME,
        INTERLEVEL,
        PAUSE,
        DEATH,
        NONE
    }

    public MapDrawer mapDrawer;
    public PlayerSpawn playerSpawn;
    
    public PlayerController player;
    
    public GameState gameState = GameState.MAINMENU;
    GameState previousGameState = GameState.NONE;
    public Grid grid;
    public AStarPathfinding pathfinder;

    // Start is called before the first frame update
    void Start()
    {
        mapDrawer = FindObjectOfType<MapDrawer>();
        
        grid = new Grid(parameters, mapDrawer);
        pathfinder = new AStarPathfinding(parameters);
        
        // Instantiate base objects
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerController>();
        playerSpawn = Instantiate(playerSpawnPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerSpawn>();
        
        mapDrawer.playerSpawnTransform = playerSpawn.transform;
        
        mapDrawer.DrawMapBorder();
        grid.CreateGrid();
        grid.CreateMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (previousGameState != gameState)
            ApplyChangeState();

        if (Input.GetKeyDown(KeyCode.Escape) && gameState == GameState.INGAME)
            gameState = GameState.PAUSE;
    }

    public void StartLevel()
    {
        gameState = GameState.INGAME;

        // Spawn the player
        playerSpawn.SpawnPlayer();
    }

    void ApplyChangeState()
    {
        switch (previousGameState)
        {
            case GameState.MAINMENU:
                StartCoroutine(Fade(mainMenuCanvas, 0));
                break;
            case GameState.PAUSE:
                Time.timeScale = 1;
                StartCoroutine(Fade(pauseCanvas, 0));
                break;
            case GameState.INTERLEVEL:
                StartCoroutine(Fade(interCanvas, 0));
                break;
            case GameState.DEATH:
                StartCoroutine(Fade(mainMenuCanvas, 0));
                break;
            default:
                break;
        }

        previousGameState = gameState;

        switch (gameState)
        {
            case GameState.MAINMENU:
                StartCoroutine(Fade(mainMenuCanvas, 1));
                break;
            case GameState.PAUSE:
                Time.timeScale = 0;
                StartCoroutine(Fade(pauseCanvas, 1));
                break;
            case GameState.INTERLEVEL:
                StartCoroutine(Fade(interCanvas, 1));
                //StartCoroutine(mapGenerator.GenerateMap());
                grid.CreateMap();
                break;
            case GameState.DEATH:
                StartCoroutine(Fade(mainMenuCanvas, 1));
                break;
            default:
                break;
        }
    }

    public Transform GetPlayerTranform()
    {
        return player.GetComponent<Transform>();
    }
    
    public List<Vector3> GetPathTo(Vector2 startPosition, Vector2 targetPosition)
    {
        return pathfinder.GetPathTo(grid, startPosition, targetPosition);
    }
    
    IEnumerator Fade(CanvasGroup canvasGroup, float fadeGoal)
    {
        while (canvasGroup.alpha != fadeGoal)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, fadeGoal, fadeSpeed);

            if (fadeGoal == 1 && canvasGroup.alpha + fadeApproximation >= fadeGoal)
                canvasGroup.alpha = fadeGoal;
            else if (fadeGoal == 0 && canvasGroup.alpha - fadeApproximation <= fadeGoal)
                canvasGroup.alpha = fadeGoal;

            yield return new WaitForEndOfFrame();
        }

        if (fadeGoal == 0)
        {
            canvasGroup.interactable = false;
            Time.timeScale = 1;
        }
        else
        {
            canvasGroup.interactable = true;
            Time.timeScale = 0;
        }
    }
}