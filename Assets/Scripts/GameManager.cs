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
    public MapGenerator mapGenerator;
    public PlayerSpawn playerSpawn;
    
    public PlayerController player;
    
    public GameState gameState = GameState.MAINMENU;
    GameState previousGameState = GameState.NONE;
    public Grid grid;
    public AStarPathfinding pathfinder;

    private void Awake()
    {
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        mapDrawer = FindObjectOfType<MapDrawer>();
        
        mapGenerator = new MapGenerator(parameters, mapDrawer);
        grid = new Grid(parameters, mapGenerator);
        pathfinder = new AStarPathfinding(parameters);
        
        // Instantiate base objects
        playerSpawn = Instantiate(playerSpawnPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerSpawn>();
        
        mapDrawer.playerSpawnTransform = playerSpawn.transform;
        
        mapDrawer.DrawMapBorder();
        grid.CreateGrid();
        
        GenerateMap();
    }

    private void GenerateMap()
    {
        mapGenerator.CreateMap();
        grid.UpdateGridState();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (previousGameState != gameState)
            ApplyChangeState();
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
                break;
            case GameState.DEATH:
                StartCoroutine(Fade(mainMenuCanvas, 1));
                break;
            default:
                break;
        }
        
        previousGameState = gameState;
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

    private void OnDrawGizmosSelected()
    {
        if (true)
        {
            
            foreach (var node in grid.nodes)
            {
                if (node.walkable)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                
                Gizmos.DrawCube(new Vector3(node.gridPositionX, node.gridPositionY), new Vector3(0.1f, 0.1f));
                
                Gizmos.color = Color.white;
                
                foreach (var neighbours in node.neighbours)
                {
                    Gizmos.DrawLine(new Vector3(node.gridPositionX, node.gridPositionY), new Vector3(neighbours.gridPositionX, neighbours.gridPositionY));
                }
            }
            
            /*Gizmos.color = Color.green;
            foreach (var cell in mapGenerator.cells)
            {
                Gizmos.DrawCube(new Vector3(cell.positionX, cell.positionY), new Vector3(0.1f, 0.1f));
            }*/
        }
    }
}