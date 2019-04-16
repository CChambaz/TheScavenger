using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class MapGenerator
{
    public Cell[,] cells;
    
    private MapParameters parameters;
    
    public MapGenerationJob mapGenerationJob;
    
    // Start is called before the first frame update
    public MapGenerator(MapParameters parameters)
    {
        this.parameters = parameters;
        
        // Init the map array
        cells = new Cell[parameters.mapSizeX, parameters.mapSizeY];
        
        for (int x = 0; x < parameters.mapSizeX; x++) 
        {
            float cellPosX = (x * parameters.cellSize.x) + (parameters.cellSize.x / 2);

            for (int y = 0; y < parameters.mapSizeY; y++)
            {
                float cellPosY = (y * parameters.cellSize.y) + (parameters.cellSize.y / 2);

                cells[x, y] = new Cell(x, y, cellPosX, cellPosY);
            }
        }
    }

    public void PrepareMapGenerationJob()
    {
        mapGenerationJob = new MapGenerationJob();
        
        mapGenerationJob.mapSizeX = parameters.mapSizeX;
        mapGenerationJob.mapSizeY = parameters.mapSizeY;
        mapGenerationJob.spawnAreaSizeX = parameters.spawnAreaSize.x;
        mapGenerationJob.spawnAreaSizeY = parameters.spawnAreaSize.y;
        mapGenerationJob.minAreaSize = parameters.minAreaSize;
        mapGenerationJob.maxAreaSize = parameters.maxAreaSize;
        mapGenerationJob.minEmptyCellsBetweenBuilding = parameters.minEmptyCellsBetweenBuilding;
        mapGenerationJob.cbMinSizeX = parameters.cbMinSizeX;
        mapGenerationJob.cbMinSizeY = parameters.cbMinSizeY;
        mapGenerationJob.cbMaxSizeX = parameters.cbMaxSizeX;
        mapGenerationJob.cbMaxSizeY = parameters.cbMaxSizeY;
        mapGenerationJob.obMinSizeX = parameters.obMinSizeX;
        mapGenerationJob.obMinSizeY = parameters.obMinSizeY;
        mapGenerationJob.obMaxSizeX = parameters.obMaxSizeX;
        mapGenerationJob.obMaxSizeY = parameters.obMaxSizeY;
        mapGenerationJob.cbSpawnChance = parameters.cbSpawnChance;
        mapGenerationJob.obSpawnChance = parameters.obSpawnChance;
        mapGenerationJob.minOpenBuildingFoes = parameters.minOpenBuildingFoes;
        mapGenerationJob.maxOpenBuildingFoes = parameters.maxOpenBuildingFoes;
        mapGenerationJob.minStreetFoes = parameters.minStreetFoes;
        mapGenerationJob.maxStreetFoes = parameters.maxStreetFoes;
        mapGenerationJob.minOpenBuildingItem = parameters.minOpenBuildingItem;
        mapGenerationJob.maxOpenBuildingItem = parameters.maxOpenBuildingItem;
        mapGenerationJob.minOpenBuildingAreaToSpawnFoes = parameters.minOpenBuildingAreaToSpawnFoes;
        mapGenerationJob.minStreetAreaToSpawnFoes = parameters.minStreetAreaToSpawnFoes;
        mapGenerationJob.obFoesSpawnChance = parameters.obFoesSpawnChance;
        mapGenerationJob.freeAreaFoesSpawnChance = parameters.freeAreaFoesSpawnChance;
        mapGenerationJob.itemSpawnChance = parameters.itemSpawnChance;
        mapGenerationJob.forceSpawnScrapWhenHasNotSpawnSince = parameters.forceSpawnScrapWhenHasNotSpawnSince;
        mapGenerationJob.forceOBSpawnAfter = parameters.forceOBSpawnAfter;
    }

    public void TranslateNativeArrayToCellBiArray()
    {
        for (int y = 0; y < parameters.mapSizeY; y++)
        {
            for (int x = 0; x < parameters.mapSizeX; x++)
            {
                switch (mapGenerationJob.result[x + (y * parameters.mapSizeX)])
                {
                    case 2:
                        cells[x, y].state = Cell.CellState.CLOSEDBUILDING;
                        break;
                    case 3:
                        cells[x, y].state = Cell.CellState.OPENDBUILDING;
                        break;
                    case 4:
                        cells[x, y].state = Cell.CellState.OPENBUILDINGGATE;
                        break;
                    case 5:
                        cells[x, y].state = Cell.CellState.SCRAPITEM;
                        break;
                    case 6:
                        cells[x, y].state = Cell.CellState.FOESPAWN;
                        break;
                    case 7:
                        cells[x, y].state = Cell.CellState.PLAYERSPAWN;
                        break;
                    default:
                        cells[x, y].state = Cell.CellState.WALKABLE;
                        break;
                }
            }
        }
        
        // Free the result native array
        mapGenerationJob.result.Dispose();
    }

    public Vector2Int GetCellID(Vector2 position)
    {
        Vector2Int nullVector = new Vector2Int(-1,-1);
        Vector2Int index = nullVector;

        for (int x = 0; x < parameters.mapSizeX; x++)
        {
            for (int y = 0; y < parameters.mapSizeY; y++)
            {
                float minPosX = cells[x, y].positionX - parameters.cellSize.x;
                float minPosY = cells[x, y].positionY - parameters.cellSize.y;

                float maxPosX = cells[x, y].positionX + parameters.cellSize.x;
                float maxPosY = cells[x, y].positionY + parameters.cellSize.y;
                
                if(position.x > minPosX && position.x < maxPosX && position.y > minPosY && position.y < maxPosY)
                {
                    index.x = x;
                    index.y = y;

                    return index;
                }
            }
        }

        return index;
    }
}
