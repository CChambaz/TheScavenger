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
    
    public Cell[,] FillMap(Cell[,] cells, MapGenerator.MapArea area)
    {
        List<Cell> visitedCellsID = new List<Cell>();
        
        for (int y = area.startIndex.y; y < area.endIndex.y; y++)
        {
            for (int x = area.startIndex.x; x < area.endIndex.x; x++)
            {
                // Check if this node has already been visited
                if (visitedCellsID.Contains(cells[x, y]))
                    continue;
                
                if (cells[x, y].state == Cell.CellState.OPENDBUILDING)
                {
                    // Check if this building has already been visited
                    if (cells[x, y - 1].state == Cell.CellState.OPENDBUILDING ||
                        cells[x, y - 1].state == Cell.CellState.OPENBUILDINGGATE)
                    {
                        // Go to the end of the building
                        x += GetOpenBuildingSize(cells, x, y).x - 1;
                        continue;
                    }

                    if (cells[x - 1, y].state != Cell.CellState.WALKABLE)
                        continue;

                    cells = FillOpenBuildingAt(cells, x, y);
                    
                    Vector2Int currentBuildingSize = GetOpenBuildingSize(cells, x, y);

                    for (int i = x; i < x + currentBuildingSize.x; i++)
                    {
                        for (int j = y; j < y + currentBuildingSize.y; j++)
                        {
                            visitedCellsID.Add(cells[i, j]);
                        }
                    }
                    
                    continue;
                }

                if (cells[x, y].state == Cell.CellState.WALKABLE)
                {
                    cells = SpawnFoesOnStreet(cells, area, x, y);
                    
                    Vector2Int currentFreeAreaSize = GetFreeAreaSize(cells, area, x, y);

                    for (int i = x; i < x + currentFreeAreaSize.x; i++)
                    {
                        for (int j = y; j < y + currentFreeAreaSize.y; j++)
                        {
                            visitedCellsID.Add(cells[i, j]);
                        }
                    }
                }
            }
        }

        return cells;
    }

    Cell[,] FillOpenBuildingAt(Cell[,] cells, int indexX, int indexY)
    {
        Vector2Int buildingSize = GetOpenBuildingSize(cells, indexX, indexY);

        Vector2Int buildingGatePosition = GetOpenBuildingGatePosition(cells, indexX, indexY, indexX + buildingSize.x, indexY + buildingSize.y);

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
                    cells[i, indexY + 1].state = Cell.CellState.SCRAPITEM;
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
                    cells[indexX + 1, i].state = Cell.CellState.SCRAPITEM;
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
                    cells[indexX + buildingSize.x - 2, i].state = Cell.CellState.SCRAPITEM;
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
                    cells[i, indexY + buildingSize.y - 2].state = Cell.CellState.SCRAPITEM;
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
                
                if (cells[x, y].state == Cell.CellState.WALKABLE)
                {
                    cells[x, y].state = Cell.CellState.FOESPAWN;
                    foesToSpawn--;
                }
            }
        }

        return cells;
    }

    Cell[,] SpawnFoesOnStreet(Cell[,] cells, MapGenerator.MapArea area, int indexX, int indexY)
    {
        Vector2Int freeAreaSize = GetFreeAreaSize(cells, area, indexX, indexY);
        
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
                
                if (cells[x, y].state == Cell.CellState.WALKABLE)
                {
                    cells[x, y].state = Cell.CellState.FOESPAWN;
                    foesToSpawn--;
                }
            }
        }

        return cells;
    }

    Vector2Int GetOpenBuildingSize(Cell[,] cells, int indexX, int indexY)
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
                if (cells[indexX + iterator, indexY].state != Cell.CellState.OPENDBUILDING && 
                    cells[indexX + iterator, indexY].state != Cell.CellState.OPENBUILDINGGATE)
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
                if (cells[indexX, indexY + iterator].state != Cell.CellState.OPENDBUILDING &&
                    cells[indexX, indexY + iterator].state != Cell.CellState.OPENBUILDINGGATE)
                    break;

                iterator++;
            }
            else
                break;
        }

        buildingSize.y = iterator;

        return buildingSize;
    }

    Vector2Int GetOpenBuildingGatePosition(Cell[,] cells, int startIndexX, int startIndexY, int endIndexX, int endIndexY)
    {
        Vector2Int gatePosition = Vector2Int.zero;

        for (int y = startIndexY; y < endIndexY; y++)
        {
            for (int x = startIndexX; x < endIndexX; x++)
            {
                if (cells[x, y].state == Cell.CellState.OPENBUILDINGGATE)
                {
                    gatePosition.x = x;
                    gatePosition.y = y;

                    return gatePosition;
                }
            }
        }

        return gatePosition;
    }
    Vector2Int GetFreeAreaSize(Cell[,] cells, MapGenerator.MapArea area, int indexX, int indexY)
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
                if (cells[indexX + iterator, indexY].state != Cell.CellState.WALKABLE)
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
                    if (cells[x, indexY + iterator].state != Cell.CellState.WALKABLE)
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
