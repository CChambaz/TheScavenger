﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public GridNode[,] nodes;
    public List<GridNode> walkableNodes = new List<GridNode>();
    
    private MapParameters parameters;

    private MapGenerator mapGenerator;

    public GridUpdateJob gridUpdateJob;

    public Grid(MapParameters parameters, MapGenerator generator)
    {
        this.parameters = parameters;
        mapGenerator = generator;
        
        nodes = new GridNode[parameters.mapSizeX - 1, parameters.mapSizeY - 1];

        for (int x = 0; x < parameters.mapSizeX - 1; x++)
        {
            float nodePosX = (x * parameters.cellSize.x) + parameters.cellSize.x;
            
            for (int y = 0; y < parameters.mapSizeY - 1; y++)
            {
                float nodePosY = (y * parameters.cellSize.y) + parameters.cellSize.y;
                
                nodes[x, y] = new GridNode(x, y, nodePosX, nodePosY);
            }
        }
        
    }

    public void PrepareGridUpdateJob()
    {
        
        gridUpdateJob = new GridUpdateJob();

        gridUpdateJob.mapSizeX = parameters.mapSizeX;
        gridUpdateJob.mapSizeY = parameters.mapSizeY;
    }
    
    public void CreateGrid()
    {
        nodes = new GridNode[parameters.mapSizeX - 1, parameters.mapSizeY - 1];

        for (int x = 0; x < parameters.mapSizeX - 1; x++)
        {
            float nodePosX = (x * parameters.cellSize.x) + parameters.cellSize.x;
            
            for (int y = 0; y < parameters.mapSizeY - 1; y++)
            {
                float nodePosY = (y * parameters.cellSize.y) + parameters.cellSize.y;
                
                nodes[x, y] = new GridNode(x, y, nodePosX, nodePosY);
            }
        }

        foreach (GridNode node in nodes)
        {
            node.neighbours = GetAllNeighbours(node);
        }
    }

    public void TranslateGridUpdateJobResult()
    {
        walkableNodes.Clear();
        
        for (int y = 0; y < parameters.mapSizeY - 1; y++)
        {
            for (int x = 0; x < parameters.mapSizeX - 1; x++)
            {
                // Define wheter or not the node is walkable
                nodes[x, y].movementCost = gridUpdateJob.result[x + (y * (parameters.mapSizeX - 1))] == 1 ? 1f : 0f;

                if (nodes[x, y].walkable)
                    walkableNodes.Add(nodes[x, y]);
            }
        }
        
        gridUpdateJob.result.Dispose();
    }
    
    public List<GridNode> GetAllNeighbours(GridNode node)
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
                    
                // Check if the node exist
                if (indexX >= 0 && indexX < parameters.mapSizeX - 1 && indexY >= 0 && indexY < parameters.mapSizeY - 1)
                {
                    neighbours.Add(nodes[indexX, indexY]);
                }
            }
        }

        return neighbours;
    }
    
    public Vector2Int GetNearestWalkableNode(Vector2 position)
    {
        Vector2Int nullVector = new Vector2Int(-1,-1);
        Vector2Int index = nullVector;

        for (int x = 0; x < parameters.mapSizeX - 1; x++)
        {
            for (int y = 0; y < parameters.mapSizeY - 1; y++)
            {
                float minPosX = nodes[x, y].gridPositionX - parameters.cellSize.x;
                float minPosY = nodes[x, y].gridPositionY - parameters.cellSize.y;

                float maxPosX = nodes[x, y].gridPositionX + parameters.cellSize.x;
                float maxPosY = nodes[x, y].gridPositionY + parameters.cellSize.y;
                
                if(nodes[x,y].walkable && position.x > minPosX && position.x < maxPosX && position.y > minPosY && position.y < maxPosY)
                {
                    index.x = x;
                    index.y = y;

                    return index;
                }
            }
        }

        // If can't find a walkable cell, extend the search area
        if (index == nullVector)
        {
            for (int x = 0; x < parameters.mapSizeX - 1; x++)
            {
                for (int y = 0; y < parameters.mapSizeY - 1; y++)
                {
                    float minPosX = nodes[x, y].gridPositionX - (parameters.cellSize.x * 2);
                    float minPosY = nodes[x, y].gridPositionY - (parameters.cellSize.y * 2);
                    
                    float maxPosX = nodes[x, y].gridPositionX + (parameters.cellSize.x * 2);
                    float maxPosY = nodes[x, y].gridPositionY + (parameters.cellSize.y * 2);
                
                    if(nodes[x,y].walkable && position.x >= minPosX && position.x <= maxPosX && position.y >= minPosY && position.y <= maxPosY)
                    {
                        index.x = x;
                        index.y = y;

                        return index;
                    }
                }
            }
        }
        
        return index;
    }
}
