using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public struct GridUpdateJob : IJob
{
    public GridNode[,] nodes;
    public MapParameters parameters;
    public MapGenerator mapGenerator;

    public bool isRunning;
    
    public void Execute()
    {
        isRunning = true;
        
        // Update the state of the nodes
        for (int x = 0; x < parameters.mapSizeX - 1; x++)
        {
            for (int y = 0; y < parameters.mapSizeY - 1; y++)
            {
                if (!mapGenerator.cells[x, y].walkable ||
                    !mapGenerator.cells[x + 1, y].walkable ||
                    !mapGenerator.cells[x, y + 1].walkable ||
                    !mapGenerator.cells[x + 1, y + 1].walkable)
                    nodes[x, y].movementCost = 0f;
                else
                    nodes[x, y].movementCost = 1f;
            }
        }
        
        // Update the neighbours of all the nodes
        for (int x = 0; x < parameters.mapSizeX - 1; x++)
        {
            for (int y = 0; y < parameters.mapSizeY - 1; y++)
            {
                nodes[x, y].neighbours = GetNeighbours(nodes[x, y]);
            }
        }

        isRunning = false;
    }
    
    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Check if equivalent to the current node
                if (x == 0 && y == 0)
                    continue;

                // Set the neighbour ID
                int indexX = node.gridIndexX + x;
                int indexY = node.gridIndexY + y;
                    
                // Check if the node exist)
                if (indexX >= 0 && indexX < parameters.mapSizeX - 1 && indexY >= 0 && indexY < parameters.mapSizeY - 1)
                {
                    // Check if the node is walkable
                    if (!nodes[indexX, indexY].walkable)
                        continue;
                    
                    neighbours.Add(nodes[indexX, indexY]);
                }
            }
        }

        return neighbours;
    }
}
