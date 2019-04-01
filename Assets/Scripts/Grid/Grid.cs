using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public GridNode[,] nodes;

    private MapParameters parameters;

    private MapGenerator mapGenerator;

    public GridUpdateJob gridUpdateJob;

    public bool isRunning = false;
    
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
        
        gridUpdateJob = new GridUpdateJob();

        gridUpdateJob.nodes = nodes;
        gridUpdateJob.parameters = parameters;
        gridUpdateJob.mapGenerator = mapGenerator;
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
    }

    public IEnumerator UpdateGrid()
    {
        gridUpdateJob.Execute();
        
        while (gridUpdateJob.isRunning)
            yield return new WaitForEndOfFrame();

        nodes = gridUpdateJob.nodes;
    }
    
    public Vector2Int GetNodeIDFromPosition(Vector2 position)
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
