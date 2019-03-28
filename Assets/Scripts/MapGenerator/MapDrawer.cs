using System.Collections;
using System.Collections.Generic;
//using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDrawer : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] public Tilemap solidTileMap;
    [SerializeField] private Tilemap freeTileMap;

    [Header("Game objects")]
    [SerializeField] private GameObject scrapContainerPrefab;
    [SerializeField] private GameObject playerSpawnPrefab;
    [SerializeField] private GameObject foePrefab;

    private GameManager gameManager;
    
    // Storage list
    private List<GameObject> scrapContainerList = new List<GameObject>();
    private List<GameObject> foesList = new List<GameObject>();

    public Transform playerSpawnTransform;
    private TilesReferences tilesReferences;
    private MapParameters parameters;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        parameters = gameManager.parameters;
        tilesReferences = parameters.tilesReferences;
    }

    public void DrawMap(Cell[,] cells)
    {
        ClearMap();
        
        for (int x = 0; x < parameters.mapSizeX; x++)
        {
            for (int y = 0; y < parameters.mapSizeY; y++)
            {
                Vector3Int currentCellPosition = solidTileMap.WorldToCell(new Vector3(x * parameters.cellSize.x, y * parameters.cellSize.y));
                Vector3 currentObjectPosition = solidTileMap.CellToWorld(currentCellPosition) + (parameters.cellSize / 2);
                        
                switch (cells[x, y].state)
                {
                    case Cell.CellState.CLOSEDBUILDING:
                        if(solidTileMap.GetTile(currentCellPosition) == null)
                            DrawClosedBuildingPart(cells, currentCellPosition, x, y);
                        break;
                    case Cell.CellState.OPENDBUILDING:
                        if (solidTileMap.GetTile(currentCellPosition) == null)
                            DrawOpenBuildingPart(cells, currentCellPosition, x, y);
                        break;
                    case Cell.CellState.SCRAPITEM:
                        freeTileMap.SetTile(currentCellPosition, tilesReferences.ground);
                        scrapContainerList.Add(Instantiate(scrapContainerPrefab, currentObjectPosition, Quaternion.identity));
                        break;
                    case Cell.CellState.PLAYERSPAWN:
                        freeTileMap.SetTile(currentCellPosition, tilesReferences.ground);
                        playerSpawnTransform.position = currentObjectPosition;
                        cells[x, y].state = Cell.CellState.WALKABLE;
                        break;
                    case Cell.CellState.FOESPAWN:
                        freeTileMap.SetTile(currentCellPosition, tilesReferences.ground);
                        foesList.Add(Instantiate(foePrefab, currentObjectPosition, Quaternion.identity));
                        cells[x, y].state = Cell.CellState.WALKABLE;
                        break;
                    default:
                        freeTileMap.SetTile(currentCellPosition, tilesReferences.ground);
                        break;
                }
            }
        }
    }

    public void ClearMap()
    {
        // Remove all existing object of the previous map
        foreach (GameObject obj in scrapContainerList)
        {
            if (obj != null)
                Destroy(obj);
        }
        
        scrapContainerList.Clear();

        foreach (GameObject obj in foesList)
        {
            if (obj != null)
                Destroy(obj);
        }
            
        foesList.Clear();
        
        for (int x = 0; x < parameters.mapSizeX; x++)
        {
            for (int y = 0; y < parameters.mapSizeY; y++)
            {
                Vector3Int currentCellPosition = solidTileMap.WorldToCell(new Vector3(x * parameters.cellSize.x, y * parameters.cellSize.y));
                
                freeTileMap.SetTile(currentCellPosition, null);
                solidTileMap.SetTile(currentCellPosition, null);
            }
        }
    }
    
    void DrawClosedBuildingPart(Cell[,] cells, Vector3Int currentCellPosition, int indexX, int indexY)
    {
        // Check if at the bottom of the building
        if (cells[indexX, indexY - 1].state != Cell.CellState.CLOSEDBUILDING)
        {
            // Check if on the left corner
            if (cells[indexX - 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBottomCornerLeftBottom);
                return;
            }

            // Check if on the right corner
            if (cells[indexX + 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBottomCornerRightBottom);
                return;
            }

            solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBaseWallBottom);
            return;
        }

        // Check if at the second level of the building
        if (cells[indexX, indexY - 2].state != Cell.CellState.CLOSEDBUILDING)
        {
            // Check if on the left corner
            if (cells[indexX - 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBottomCornerLeftMid);
                return;
            }

            // Check if on the right corner
            if (cells[indexX + 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBottomCornerRightMid);
                return;
            }

            solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBaseWallMid);
            return;
        }

        // Check if at the third level of the building
        if (cells[indexX, indexY - 3].state != Cell.CellState.CLOSEDBUILDING)
        {
            // Check if on the left corner
            if (cells[indexX - 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBottomCornerLeftTop);
                return;
            }

            // Check if on the right corner
            if (cells[indexX + 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBottomCornerRightTop);
                return;
            }

            solidTileMap.SetTile(currentCellPosition, tilesReferences.cbBaseWallTop);
            return;
        }

        // Check if at the fourth level of the building
        if (cells[indexX, indexY - 4].state != Cell.CellState.CLOSEDBUILDING)
        {
            // Check if on the left side
            if (cells[indexX - 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbInnerRoofBottomLeftSide);
                return;
            }

            // Check if on the right side
            if (cells[indexX + 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbInnerRoofBottomRightSide);
                return;
            }

            solidTileMap.SetTile(currentCellPosition, tilesReferences.cbInnerRoofBottom);
            return;
        }

        // Check if at the last level of the building
        if (cells[indexX, indexY + 1].state != Cell.CellState.CLOSEDBUILDING)
        {
            // Check if on the left side
            if (cells[indexX - 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbTopCornerLeft);
                return;
            }

            // Check if on the right side
            if (cells[indexX + 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.cbTopCornerRight);
                return;
            }

            solidTileMap.SetTile(currentCellPosition, tilesReferences.cbTopWall);
            return;
        }

        // Check if on the left side
        if (cells[indexX - 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
        {
            solidTileMap.SetTile(currentCellPosition, tilesReferences.cbInnerRoofLeftSide);
            return;
        }

        // Check if on the right side
        if (cells[indexX + 1, indexY].state != Cell.CellState.CLOSEDBUILDING)
        {
            solidTileMap.SetTile(currentCellPosition, tilesReferences.cbInnerRoofRightSide);
            return;
        }

        // Otherwise, the cell is inside the building
        solidTileMap.SetTile(currentCellPosition, tilesReferences.cbInnerRoof);
    }

    void DrawOpenBuildingPart(Cell[,] cells, Vector3Int currentCellPosition, int indexX, int indexY)
    {
        int iterator = 0;

        bool isOnLeft = false;
        bool isOnRight = false;
        bool isOnTop = false;

        // Check the position relatively to the building
        while (true)
        {
            // Check when the iterator reach the end of the building
            if (cells[indexX, indexY - iterator - 1].state != Cell.CellState.OPENDBUILDING &&
                cells[indexX, indexY - iterator - 1].state != Cell.CellState.OPENBUILDINGGATE)
            {
                // Check if the bottom cell is on left
                if (cells[indexX - 1, indexY - iterator].state != Cell.CellState.OPENDBUILDING &&
                    cells[indexX - 1, indexY - iterator].state != Cell.CellState.OPENBUILDINGGATE)
                {
                    isOnLeft = true;
                }

                // Check if the bottom cell is on right
                if (cells[indexX + 1, indexY - iterator].state != Cell.CellState.OPENDBUILDING &&
                    cells[indexX + 1, indexY - iterator].state != Cell.CellState.OPENBUILDINGGATE)
                {
                    isOnRight = true;
                }

                if (iterator > 0)
                    isOnTop = true;

                break;
            }

            iterator++;
        }

        if (isOnRight)
        {
            // Check if on the top
            if (cells[indexX, indexY + 1].state != Cell.CellState.OPENDBUILDING &&
                cells[indexX, indexY + 1].state != Cell.CellState.OPENBUILDINGGATE)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.obCornerTopRight);
                return;
            }

            // Check if on the bottom
            if (cells[indexX, indexY - 1].state != Cell.CellState.OPENDBUILDING &&
                cells[indexX, indexY - 1].state != Cell.CellState.OPENBUILDINGGATE)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.obCornerBottomRight);
                return;
            }

            solidTileMap.SetTile(currentCellPosition, tilesReferences.obWallRight);
            return;
        }

        if (isOnLeft)
        {
            // Check if on the top
            if (cells[indexX, indexY + 1].state != Cell.CellState.OPENDBUILDING &&
                cells[indexX, indexY + 1].state != Cell.CellState.OPENBUILDINGGATE)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.obCornerTopLeft);
                return;
            }

            // Check if on the bottom
            if (cells[indexX, indexY - 1].state != Cell.CellState.OPENDBUILDING &&
                cells[indexX, indexY - 1].state != Cell.CellState.OPENBUILDINGGATE)
            {
                solidTileMap.SetTile(currentCellPosition, tilesReferences.obCornerBottomLeft);
                return;
            }

            solidTileMap.SetTile(currentCellPosition, tilesReferences.obWallLeft);
            return;
        }

        if(isOnTop)
            solidTileMap.SetTile(currentCellPosition, tilesReferences.obWallTop);
        else
            solidTileMap.SetTile(currentCellPosition, tilesReferences.obWallBottom);
    }

    public void DrawMapBorder()
    {
        Vector3Int currentCellPosition;
        
        // Draw bottom border
        for (int x = -1; x <= parameters.mapSizeX; x++)
        {
            currentCellPosition = solidTileMap.WorldToCell(new Vector3(x * parameters.cellSize.x, -1 * parameters.cellSize.y));

            solidTileMap.SetTile(currentCellPosition, tilesReferences.simpleWall);
        }
        
        // Draw left border
        for (int y = 0; y < parameters.mapSizeY; y++)
        {
            currentCellPosition = solidTileMap.WorldToCell(new Vector3(-1 * parameters.cellSize.x, y * parameters.cellSize.y));
            
            solidTileMap.SetTile(currentCellPosition, tilesReferences.simpleWall);
        }
        
        // Draw right border
        for (int y = 0; y < parameters.mapSizeY; y++)
        {
            currentCellPosition = solidTileMap.WorldToCell(new Vector3(parameters.mapSizeX * parameters.cellSize.x, y * parameters.cellSize.y));
            
            solidTileMap.SetTile(currentCellPosition, tilesReferences.simpleWall);
        }
        
        // Draw top border
        for (int x = -1; x <= parameters.mapSizeX; x++)
        {
            currentCellPosition = solidTileMap.WorldToCell(new Vector3(x * parameters.cellSize.x, parameters.mapSizeY * parameters.cellSize.y));

            solidTileMap.SetTile(currentCellPosition, tilesReferences.simpleWall);
        }
    }
}
