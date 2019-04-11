using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine.Serialization;
using Unity.Mathematics;

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

    [Header("Level parameters")] 
    [SerializeField] public float dayDuration;
    
    public uint seed;

    public uint levelNumber = 0;

    public bool generationInProgress = false;
    
    public enum GameState
    {
        MAINMENU,
        INGAMEDAY,
        INGAMENIGHT,
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

    public float actualDayDuration;
    
    private void Awake()
    {
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerController>();
        seed = (uint) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    // Start is called before the first frame update
    void Start()
    {
        mapDrawer = FindObjectOfType<MapDrawer>();
        
        mapGenerator = new MapGenerator(parameters, mapDrawer, seed);
        grid = new Grid(parameters, mapGenerator);
        
        // Instantiate base objects
        playerSpawn = Instantiate(playerSpawnPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerSpawn>();
        
        mapDrawer.playerSpawnTransform = playerSpawn.transform;
        mapGenerator.PrepareMapGenerationJob();
        grid.PrepareGridUpdateJob();
        mapDrawer.DrawMapBorder();
        grid.CreateGrid();
        player.ResetPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.INGAMEDAY)
            actualDayDuration += Time.deltaTime;

        if (actualDayDuration >= dayDuration && gameState == GameState.INGAMEDAY)
            gameState = GameState.INGAMENIGHT;
        
        if (previousGameState != gameState)
            ApplyChangeState();
    }

    public IEnumerator GenerateMap()
    {
        generationInProgress = true;
        
        // Increment the seed of the random
        mapGenerator.mapGenerationJob.random = new Unity.Mathematics.Random(seed + levelNumber);
        
        // Prepare base array for the jobs
        NativeArray<int> mapResult = new NativeArray<int>(parameters.mapSizeX * parameters.mapSizeY, Allocator.TempJob);

        mapGenerator.mapGenerationJob.result = mapResult;
        
        // Start the map generation job
        JobHandle jobHandle = mapGenerator.mapGenerationJob.Schedule();

        // Wait until map generation job is completed
        yield return new WaitUntil(() => jobHandle.IsCompleted); 
        
        // Ensure that the job is completed
        jobHandle.Complete();
        
        // Give the simple map to the grid update job
        grid.gridUpdateJob.cells = mapGenerator.mapGenerationJob.result;

        NativeArray<int> nodeResult = new NativeArray<int>((parameters.mapSizeX - 1) * (parameters.mapSizeY - 1), Allocator.TempJob);
        
        grid.gridUpdateJob.result = nodeResult;
        
        // Start the update of the grid
        jobHandle = grid.gridUpdateJob.Schedule();
        
        // Wait until the grid update job is completed
        yield return new WaitUntil(() => jobHandle.IsCompleted); 
        
        // Ensure that the job is completed
        jobHandle.Complete();
        
        // Translate the map result to actual cells
        mapGenerator.TranslateNativeArrayToCellBiArray();
        
        // Translate the node result to the actual nodes
        grid.TranslateGridUpdateJobResult();
        
        // Draw the map
        mapDrawer.DrawMap(mapGenerator.cells);
        
        // Prepare the level number for the next generation
        levelNumber++;

        generationInProgress = false;
        
        // Spawn the player
        playerSpawn.SpawnPlayer();

        actualDayDuration = 0;
        
        // Start the level
        gameState = GameState.INGAMEDAY;
    }
    
    void ApplyChangeState()
    {
        switch (previousGameState)
        {
            case GameState.MAINMENU:
                ResetGame();
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
                ResetGame();
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
                break;
            case GameState.DEATH:
                StartCoroutine(Fade(mainMenuCanvas, 1));
                break;
            default:
                break;
        }
        
        previousGameState = gameState;
    }

    private void ResetGame()
    {
        player.ResetPlayer();
        levelNumber = 1;
    }
    
    public Transform GetPlayerTranform()
    {
        return player.GetComponent<Transform>();
    }

    public bool isInOpenBuilding(Vector2 position)
    {
        Vector2Int cellID = new Vector2Int(-1, -1);
        
        // Get the cell at the given position
        for (int y = 0; y < parameters.mapSizeY; y++)
        {
            for (int x = 0; x < parameters.mapSizeX; x++)
            {
                if (mapGenerator.cells[x, y].positionX > position.x - (parameters.cellSize.x / 4) &&
                    mapGenerator.cells[x, y].positionX < position.x + (parameters.cellSize.x / 4) &&
                    mapGenerator.cells[x, y].positionX > position.y - (parameters.cellSize.y / 4) &&
                    mapGenerator.cells[x, y].positionX < position.y + (parameters.cellSize.y / 4))
                {
                    cellID = new Vector2Int(x, y);
                    break;
                }
            }

            if (cellID.x != -1 && cellID.y != -1)
                break;
        }

        // Check if a cell has been found
        if (cellID.x == -1 && cellID.y == -1)
            return false;
        
        // Check the cell neighbours
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                // Check if outside of the map
                if (cellID.x + x < 0 || cellID.x + x >= parameters.mapSizeX - 1 || cellID.y + y < 0 || cellID.y + y >= parameters.mapSizeY - 1)
                    continue;
                    
                if (mapGenerator.cells[cellID.x + x, cellID.y + y].state == Cell.CellState.OPENDBUILDING ||
                    mapGenerator.cells[cellID.x + x, cellID.y + y].state == Cell.CellState.OPENBUILDINGGATE)
                    return true;
            }
        }
        
        return false;
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

    public bool gameRunning
    {
        get { return gameState == GameState.INGAMEDAY || gameState == GameState.INGAMENIGHT; }
    }

    public int playerDamage
    {
        get { return player.attackDamage; }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (false)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    switch (x)
                    {
                        case 0:
                            Gizmos.color = Color.white;
                            break;
                        case 1:
                            Gizmos.color = Color.cyan;
                            break;
                        case 2:
                            Gizmos.color = Color.green;
                            break;
                        case 3:
                            Gizmos.color = Color.yellow;
                            break;
                        case 4:
                            Gizmos.color = Color.blue;
                            break;
                        case 5:
                            Gizmos.color = Color.red;
                            break;
                        case 6:
                            Gizmos.color = Color.magenta;
                            break;
                        case 7:
                            Gizmos.color = Color.gray;
                            break;
                        case 8:
                            Gizmos.color = Color.grey;
                            break;
                        case 9:
                            Gizmos.color = Color.black;
                            break;
                    }
                    
                    Gizmos.DrawCube(new Vector3(grid.nodes[x,y].gridPositionX, grid.nodes[x,y].gridPositionY), new Vector3(0.1f, 0.1f));
                    Gizmos.DrawCube(new Vector3(mapGenerator.cells[x,y].positionX, mapGenerator.cells[x,y].positionY), new Vector3(0.1f, 0.1f));
                }
            }
            
            /*foreach (var node in grid.nodes)
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