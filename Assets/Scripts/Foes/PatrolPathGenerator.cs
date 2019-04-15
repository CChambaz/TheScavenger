using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class PatrolPathGenerator : MonoBehaviour
{
    private GameManager gameManager;
    private MapGenerator mapGenerator;
    private MapParameters parameters;
    private Grid grid;
    private int offset = 1;
    private Random random;
    
    // Start is called before the first frame update
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();       
    }

    private void Start()
    {
        random = new Random(gameManager.seed);
        mapGenerator = gameManager.mapGenerator;
        grid = gameManager.grid;
        parameters = gameManager.parameters;
    }

    public List<Vector3> GeneratePatrolPath(Vector2 position)
    {
        if (mapGenerator == null)
            mapGenerator = gameManager.mapGenerator;

        if (grid == null)
            grid = gameManager.grid;
        
        List<Vector3> patrolPath = null;
        
        Vector2Int currentNodeID = grid.GetNearestWalkableNode(position);

        Vector2Int firstBuldingNodeEncountered = GetNearestBuilding(currentNodeID);

        if (CheckIfOpenBuilding(firstBuldingNodeEncountered))
            return patrolPath;
        
        patrolPath = new List<Vector3>();

        // Move the node ID to the bottom left corner
        while (firstBuldingNodeEncountered.y - 1 > 0 && !grid.nodes[firstBuldingNodeEncountered.x, firstBuldingNodeEncountered.y - 1].walkable)
            firstBuldingNodeEncountered.y--;
        
        while (firstBuldingNodeEncountered.x - 1 > 0 && !grid.nodes[firstBuldingNodeEncountered.x - 1, firstBuldingNodeEncountered.y].walkable)
            firstBuldingNodeEncountered.x--;
        
        Vector2Int buildingSize = GetBuildingSize(firstBuldingNodeEncountered);
        
        // Get the walkable corners surrounding the building
        GridNode bottomLeftCornerNode = grid.nodes[firstBuldingNodeEncountered.x - offset, firstBuldingNodeEncountered.y - offset];
        GridNode bottomRightCornerNode = grid.nodes[firstBuldingNodeEncountered.x + buildingSize.x + offset, firstBuldingNodeEncountered.y - offset];
        GridNode topLeftCornerNode = grid.nodes[firstBuldingNodeEncountered.x - offset, firstBuldingNodeEncountered.y + buildingSize.y + offset];
        GridNode topRightCornerNode = grid.nodes[firstBuldingNodeEncountered.x + buildingSize.x + offset, firstBuldingNodeEncountered.y + buildingSize.y + offset];
        
        // Get the corners position and add them to the patrol path
        patrolPath.Add(new Vector3(bottomLeftCornerNode.gridPositionX, bottomLeftCornerNode.gridPositionY));
        patrolPath.Add(new Vector3(topLeftCornerNode.gridPositionX, topLeftCornerNode.gridPositionY));
        patrolPath.Add(new Vector3(topRightCornerNode.gridPositionX, topRightCornerNode.gridPositionY));
        patrolPath.Add(new Vector3(bottomRightCornerNode.gridPositionX, bottomRightCornerNode.gridPositionY));
        
        return patrolPath;
    }

    public Vector2 GetRandomWalkableNode()
    {
        // Get a walkable node
        GridNode node = grid.walkableNodes[random.NextInt(0, grid.walkableNodes.Count - 1)];

        Vector2 nodePosition = new Vector2();

        nodePosition.x = node.gridPositionX;
        nodePosition.y = node.gridPositionY;

        return nodePosition;
    }
    
    private Vector2Int GetNearestBuilding(Vector2Int startingNodeID)
    {
        List<GridNode> openList = new List<GridNode>();
        List<GridNode> closedList = new List<GridNode>();
        
        // Add the starting node to the open list
        openList.Add(grid.nodes[startingNodeID.x, startingNodeID.y]);

        GridNode currentNode;
        
        while (openList.Count > 0)
        {
            // Get the first node of the list
            currentNode = openList[0];

            openList.Remove(currentNode);
            closedList.Add(currentNode);
            
            foreach (GridNode node in currentNode.neighbours)
            {
                // Check if the node has already been visited
                if (closedList.Contains(node))
                    continue;
                
                // Check if the search is over
                if (!node.walkable)
                {
                    return new Vector2Int(node.gridIndexX, node.gridIndexY);
                }
                
                openList.Add(node);
            }
        }

        return new Vector2Int(-1, -1);
    }

    private Vector2Int GetBuildingSize(Vector2Int bottomCornerLeftNodeID)
    {
        Vector2Int buildingSize = Vector2Int.zero;
        
        while (!grid.nodes[bottomCornerLeftNodeID.x + buildingSize.x + 1, bottomCornerLeftNodeID.y].walkable)
            buildingSize.x++;
        
        while (!grid.nodes[bottomCornerLeftNodeID.x, bottomCornerLeftNodeID.y + buildingSize.y + 1].walkable)
            buildingSize.y++;
        
        return buildingSize;
    }

    private bool CheckIfOpenBuilding(Vector2Int nodeID)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Set the neighbour ID
                int indexX = mapGenerator.cells[nodeID.x, nodeID.y].indexX + x;
                int indexY = mapGenerator.cells[nodeID.x, nodeID.y].indexY + y;
                    
                // Check if the node exist
                if (indexX >= 0 && indexX < parameters.mapSizeX - 1 && indexY >= 0 && indexY < parameters.mapSizeY - 1)
                {
                    if (mapGenerator.cells[indexX, indexY].state == Cell.CellState.OPENDBUILDING ||
                        mapGenerator.cells[indexX, indexY].state == Cell.CellState.OPENBUILDINGGATE)
                        return true;
                }
            }
        }

        return false;
    }
    
    public bool isInOpenBuilding(Vector2 position)
    {
        Vector2Int cellID = new Vector2Int(-1, -1);
        
        // Get the cell at the given position
        cellID = mapGenerator.GetCellID(position);

        // Check if a cell has been found
        if (cellID.x == -1 && cellID.y == -1)
        {
            return false;
        }

        Vector2Int buildingCellID = new Vector2Int(-1, -1);
        
        // Check the cell neighbours
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                // Check if outside of the map
                if (cellID.x + x < 0 || cellID.x + x > parameters.mapSizeX || cellID.y + y < 0 || cellID.y + y > parameters.mapSizeY)
                    continue;

                // Check if the cell is a part of an open building
                if (mapGenerator.cells[cellID.x + x, cellID.y + y].state == Cell.CellState.OPENDBUILDING ||
                    mapGenerator.cells[cellID.x + x, cellID.y + y].state == Cell.CellState.OPENBUILDINGGATE)
                {
                    buildingCellID.x = cellID.x + x;
                    buildingCellID.y = cellID.y + y;
                }
            }
        }
        
        // Check if an open building has been found
        if (buildingCellID.x == -1 && buildingCellID.y == -1)
        {
            return false;
        }
        
        // Check if it has to start to move the Y first
        if (mapGenerator.cells[buildingCellID.x + 1, buildingCellID.y].state == Cell.CellState.WALKABLE &&
            mapGenerator.cells[buildingCellID.x - 1, buildingCellID.y].state == Cell.CellState.WALKABLE)
        {
            // Move the cell ID to the bottom left corner of the building
            while (buildingCellID.y - 1 > 0 && (mapGenerator.cells[buildingCellID.x, buildingCellID.y - 1].state ==
                                        Cell.CellState.OPENDBUILDING ||
                                        mapGenerator.cells[buildingCellID.x, buildingCellID.y - 1].state ==
                                        Cell.CellState.OPENBUILDINGGATE))
                buildingCellID.y--;
            
            while (buildingCellID.x - 1 > 0 && (mapGenerator.cells[buildingCellID.x - 1, buildingCellID.y].state ==
                                                Cell.CellState.OPENDBUILDING ||
                                                mapGenerator.cells[buildingCellID.x - 1, buildingCellID.y].state ==
                                                Cell.CellState.OPENBUILDINGGATE))
                buildingCellID.x--;
        }
        else
        {
            // Move the cell ID to the bottom left corner of the building
            while (buildingCellID.x - 1 > 0 && (mapGenerator.cells[buildingCellID.x - 1, buildingCellID.y].state ==
                                                Cell.CellState.OPENDBUILDING ||
                                                mapGenerator.cells[buildingCellID.x - 1, buildingCellID.y].state ==
                                                Cell.CellState.OPENBUILDINGGATE))
                buildingCellID.x--;
            
            while (buildingCellID.y - 1 > 0 && (mapGenerator.cells[buildingCellID.x, buildingCellID.y - 1].state ==
                                        Cell.CellState.OPENDBUILDING ||
                                        mapGenerator.cells[buildingCellID.x, buildingCellID.y - 1].state ==
                                        Cell.CellState.OPENBUILDINGGATE))
                buildingCellID.y--;
        }

        Vector2Int buildingSize = GetOpenBuildingSize(buildingCellID.x, buildingCellID.y);
        
        // Check if the given position is beetwen the min and max position value of the building
        bool yPosIsGreaterThanMinBuildingPosY = mapGenerator.cells[buildingCellID.x, buildingCellID.y].positionY < position.y; 
        bool yPosIsSmallerThanMaxBuildingPosY = mapGenerator.cells[buildingCellID.x, buildingCellID.y + buildingSize.y].positionY > position.y; 
        bool xPosIsGreaterThanMinBuildingPosX = mapGenerator.cells[buildingCellID.x, buildingCellID.y].positionX < position.x; 
        bool xPosIsSmallerThanMaxBuildingPosX = mapGenerator.cells[buildingCellID.x + buildingSize.x, buildingCellID.y].positionX > position.x;

        if (yPosIsGreaterThanMinBuildingPosY && yPosIsSmallerThanMaxBuildingPosY &&
            xPosIsGreaterThanMinBuildingPosX && xPosIsSmallerThanMaxBuildingPosX)
            return true;
        
        return false;
    }
    
    Vector2Int GetOpenBuildingSize(int indexX, int indexY)
    {
        Vector2Int buildingSize = Vector2Int.zero;

        int iterator = 0;

        // Define the X size of the building
        while (true)
        {
            // Check if not on the edge of the map
            if (indexX + iterator < parameters.mapSizeX)
            {
                // Check if still in the building
                if (mapGenerator.cells[indexX + iterator, indexY].state != Cell.CellState.OPENDBUILDING && 
                    mapGenerator.cells[indexX + iterator, indexY].state != Cell.CellState.OPENBUILDINGGATE)
                    break;

                iterator++;
            }
            else
                break;
        }

        buildingSize.x = iterator;
        iterator = 0;

        // Define the Y size of the building
        while (true)
        {
            // Check if not on the edge of the map
            if (indexY + iterator < parameters.mapSizeY)
            {
                // Check if still in the building
                if (mapGenerator.cells[indexX, indexY + iterator].state != Cell.CellState.OPENDBUILDING &&
                    mapGenerator.cells[indexX, indexY + iterator].state != Cell.CellState.OPENBUILDINGGATE)
                    break;

                iterator++;
            }
            else
                break;
        }

        buildingSize.y = iterator;

        return buildingSize;
    }
    
    private void OnDrawGizmos()
    {
        /*foreach (Cell cell in debugStartCell)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(cell.positionX, cell.positionY), new Vector3(0.5f, 0.5f, 0.5f));
        }*\/

        foreach (GridNode node in debugStartCell)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(node.gridPositionX, node.gridPositionY), new Vector3(0.25f, 0.25f, 0.25f));
        }*/
    }
}
