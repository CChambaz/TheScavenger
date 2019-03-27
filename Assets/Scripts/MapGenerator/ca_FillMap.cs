using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class ca_FillMap
{
    private MapParameters parameters;
    
    // Counter values to force spawn
    private int hasNotSpawnScrapSince = 0;

    public ca_FillMap(MapParameters parameters)
    {
        this.parameters = parameters;
    }
    
    public GridNode[,] FillMap(GridNode[,] nodes, Grid.MapArea area)
    {
        List<GridNode> visitedNodesIDs = new List<GridNode>();
        
        for (int y = area.startIndex.y; y < area.endIndex.y; y++)
        {
            for (int x = area.startIndex.x; x < area.endIndex.x; x++)
            {
                // Check if this node has already been visited
                if (visitedNodesIDs.Contains(nodes[x, y]))
                    continue;
                
                if (nodes[x, y].state == GridNode.NodeState.OPENDBUILDING)
                {
                    // Check if this building has already been visited
                    if (nodes[x, y - 1].state == GridNode.NodeState.OPENDBUILDING ||
                        nodes[x, y - 1].state == GridNode.NodeState.OPENBUILDINGGATE)
                    {
                        // Go to the end of the building
                        x += GetOpenBuildingSize(nodes, x, y).x - 1;
                        continue;
                    }

                    if (nodes[x - 1, y].state != GridNode.NodeState.WALKABLE)
                        continue;

                    nodes = FillOpenBuildingAt(nodes, x, y);
                    
                    Vector2Int currentBuildingSize = GetOpenBuildingSize(nodes, x, y);

                    for (int i = x; i < x + currentBuildingSize.x; i++)
                    {
                        for (int j = y; j < y + currentBuildingSize.y; j++)
                        {
                            visitedNodesIDs.Add(nodes[i, j]);
                        }
                    }
                    
                    continue;
                }

                if (nodes[x, y].state == GridNode.NodeState.WALKABLE)
                {
                    nodes = SpawnFoesOnStreet(nodes, area, x, y);
                    
                    Vector2Int currentFreeAreaSize = GetFreeAreaSize(nodes, area, x, y);

                    for (int i = x; i < x + currentFreeAreaSize.x; i++)
                    {
                        for (int j = y; j < y + currentFreeAreaSize.y; j++)
                        {
                            visitedNodesIDs.Add(nodes[i, j]);
                        }
                    }
                }
            }
        }

        return nodes;
    }

    GridNode[,] FillOpenBuildingAt(GridNode[,] nodes, int indexX, int indexY)
    {
        Vector2Int buildingSize = GetOpenBuildingSize(nodes, indexX, indexY);

        Vector2Int buildingGatePosition = GetOpenBuildingGatePosition(nodes, indexX, indexY, indexX + buildingSize.x, indexY + buildingSize.y);

        int foesToSpawn = Random.Range(parameters.minOpenBuildingFoes, parameters.maxOpenBuildingFoes);
        int itemToSpawn = Random.Range(parameters.minOpenBuildingItem, parameters.maxOpenBuildingItem);

        float rnd = Random.Range(0f, 1f);

        // Define if there is enough place to spawn foes
        if ((buildingSize.x - 2) * (buildingSize.y - 2) > parameters.minOpenBuildingAreaToSpawnFoes || rnd > parameters.foesSpawnChance)
            foesToSpawn = 0;

        if (hasNotSpawnScrapSince < parameters.forceSpawnScrapWhenHasNotSpawnSince && rnd > parameters.itemSpawnChance)
        {
            itemToSpawn = 0;
            hasNotSpawnScrapSince++;
        }
        else
            hasNotSpawnScrapSince = 0;

        int itemSpawnedCount = 0;

        // Check the gate position and spawn items containers on the opposite side
        // Gate on top
        if (buildingGatePosition.y >= indexY + buildingSize.y - 1)
        {
            for (int i = indexX + 1; i < indexX + buildingSize.x - 1; i++)
            {
                if (itemSpawnedCount < itemToSpawn)
                {
                    nodes[i, indexY + 1].state = GridNode.NodeState.SCRAPITEM;
                    itemSpawnedCount++;
                }
            }
        }
        // Gate on right
        else if (buildingGatePosition.x >= indexX + buildingSize.x - 1)
        {
            for (int i = indexY + 1; i < indexY + buildingSize.y - 1; i++)
            {
                if (itemSpawnedCount < itemToSpawn)
                {
                    nodes[indexX + 1, i].state = GridNode.NodeState.SCRAPITEM;
                    itemSpawnedCount++;
                }
            }
        }
        // Gate on left
        else if (buildingGatePosition.y > indexY && buildingGatePosition.x == indexX)
        {
            for (int i = indexY + 1; i < indexY + buildingSize.y - 1; i++)
            {
                if (itemSpawnedCount < itemToSpawn)
                {
                    nodes[indexX + buildingSize.x - 2, i].state = GridNode.NodeState.SCRAPITEM;
                    itemSpawnedCount++;
                }
            }
        }
        // Gate on bottom
        else if(buildingGatePosition.x > indexX && buildingGatePosition.y == indexY)
        {
            for (int i = indexX + 1; i < indexX + buildingSize.x - 1; i++)
            {
                if (itemSpawnedCount < itemToSpawn)
                {
                    nodes[i, indexY + buildingSize.y - 2].state = GridNode.NodeState.SCRAPITEM;
                    itemSpawnedCount++;
                }
            }
        }

        for (int y = indexY + 1; y < indexY + buildingSize.y - 1; y++)
        {
            for (int x = indexX + 1; x < indexX + buildingSize.x - 1; x++)
            {
                if (foesToSpawn <= 0)
                    break;
                
                if (nodes[x, y].walkable)
                {
                    nodes[x, y].state = GridNode.NodeState.FOESPAWN;
                    foesToSpawn--;
                }
            }
        }

        return nodes;
    }

    GridNode[,] SpawnFoesOnStreet(GridNode[,] nodes, Grid.MapArea area, int indexX, int indexY)
    {
        Vector2Int freeAreaSize = GetFreeAreaSize(nodes, area, indexX, indexY);
        
        int foesToSpawn = Random.Range(parameters.minStreetFoes, parameters.maxStreetFoes);

        float rnd = Random.Range(0f, 1f);

        if (freeAreaSize.x * freeAreaSize.y > parameters.minStreetAreaToSpawnFoes && rnd > parameters.foesSpawnChance)
            foesToSpawn = 0;

        for (int y = indexY; y < indexY + freeAreaSize.y; y++)
        {
            for (int x = indexX; x < indexX + freeAreaSize.x; x++)
            {
                if (foesToSpawn <= 0)
                    break;
                
                if (nodes[x, y].walkable)
                {
                    nodes[x, y].state = GridNode.NodeState.FOESPAWN;
                    foesToSpawn--;
                }
            }
        }

        return nodes;
    }

    Vector2Int GetOpenBuildingSize(GridNode[,] nodes, int indexX, int indexY)
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
                if (nodes[indexX + iterator, indexY].state != GridNode.NodeState.OPENDBUILDING && 
                    nodes[indexX + iterator, indexY].state != GridNode.NodeState.OPENBUILDINGGATE)
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
                if (nodes[indexX, indexY + iterator].state != GridNode.NodeState.OPENDBUILDING &&
                    nodes[indexX, indexY + iterator].state != GridNode.NodeState.OPENBUILDINGGATE)
                    break;

                iterator++;
            }
            else
                break;
        }

        buildingSize.y = iterator;

        return buildingSize;
    }

    Vector2Int GetOpenBuildingGatePosition(GridNode[,] nodes, int startIndexX, int startIndexY, int endIndexX, int endIndexY)
    {
        Vector2Int gatePosition = Vector2Int.zero;

        for (int y = startIndexY; y < endIndexY; y++)
        {
            for (int x = startIndexX; x < endIndexX; x++)
            {
                if (nodes[x, y].state == GridNode.NodeState.OPENBUILDINGGATE)
                {
                    gatePosition.x = x;
                    gatePosition.y = y;

                    return gatePosition;
                }
            }
        }

        return gatePosition;
    }
    Vector2Int GetFreeAreaSize(GridNode[,] nodes, Grid.MapArea area, int indexX, int indexY)
    {
        Vector2Int freeAreaSize = Vector2Int.zero;

        int iterator = 0;

        // Define the X size of the free area
        while (true)
        {
            // Check if still in the area
            if (indexX + iterator < area.endIndex.x)
            {
                // Check if still in a free area
                if (nodes[indexX + iterator, indexY].state != GridNode.NodeState.WALKABLE)
                    break;

                iterator++;
            }
            else
            {
                break;
            }
        }

        freeAreaSize.x = iterator;
        iterator = 0;

        bool stillInFreeArea = true;

        // Define the Y size of the building
        while (true)
        {
            // Check if still in the area
            if (indexY + iterator < area.endIndex.y)
            {
                // Check if still in a free area
                for (int x = indexX; x < indexX + freeAreaSize.x; x++)
                {
                    if (nodes[x, indexY + iterator].state != GridNode.NodeState.WALKABLE)
                        stillInFreeArea = false;
                }

                if (!stillInFreeArea)
                    break;

                iterator++;
            }
            else
                break;
        }

        freeAreaSize.y = iterator;

        return freeAreaSize;
    }
}
