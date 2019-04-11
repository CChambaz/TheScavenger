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

        mapGenerator = gameManager.mapGenerator;
        grid = gameManager.grid;
        parameters = gameManager.parameters;
    }

    private void Start()
    {
        random = new Random(gameManager.seed);
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

    public Vector2 GetRandomReachablePoint(Vector2 position, Vector2 direction)
    {
        // Check if no direction has been provided
        if (direction == Vector2.zero)
        {
            int rnd = random.NextInt(0, 100);
            
            //Define the random direction
            if(rnd < 25)
                direction = Vector2.up;
            else if(rnd < 50)
                direction = Vector2.right;
            else if(rnd < 75)
                direction = Vector2.down;
            else
                direction = Vector2.left;
        }
        
        RaycastHit2D ray = Physics2D.Raycast(position, direction);

        return ray.point;
    }

    public Vector2 GetRandomWalkableNode()
    {
        Vector2Int nodeID = new Vector2Int();

        // Search a walkable node
        while (true)
        {
            nodeID.x = random.NextInt(0, gameManager.parameters.mapSizeX - 1);
            nodeID.y = random.NextInt(0, gameManager.parameters.mapSizeY - 1);

            if (grid.nodes[nodeID.x, nodeID.y].walkable)
                break;
        }

        Vector2 nodePosition = new Vector2();

        nodePosition.x = grid.nodes[nodeID.x, nodeID.y].gridPositionX;
        nodePosition.y = grid.nodes[nodeID.x, nodeID.y].gridPositionY;

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
