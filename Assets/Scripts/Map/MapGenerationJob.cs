using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

public struct MapGenerationJob : IJob
{
    //public NativeArray<Cell> result;

    public bool isRunning;
    public Cell[,] cells;
    public MapParameters parameters;
    public List<MapGenerator.MapArea> mapAreas;
    public ca_BuildingGenerator buildingGenerator;
    public bsp_MapDivider mapDivider;
    public ca_FillMap fillMap;
    
    // Start is called before the first frame update
    public void Execute()
    {
        isRunning = true;
        
        for (int x = 0; x < parameters.mapSizeX; x++) {
            for (int y = 0; y < parameters.mapSizeY; y++)
                cells[x, y].state = Cell.CellState.WALKABLE;
        }

        // Generate the map areas
        mapAreas = mapDivider.DividMap();

        // Generate the buildings in all the areas
        for (int i = 0; i < mapAreas.Count; i++)
            cells = buildingGenerator.GenerateBuildingsInArea(cells, mapAreas[i].startIndex.x, mapAreas[i].startIndex.y, mapAreas[i].endIndex.x, mapAreas[i].endIndex.y);

        // Place the player spawn in the central area of the map
        CreatePlayerSpawn(parameters.mapSizeX / 2, parameters.mapSizeY / 2, parameters.spawnAreaSize.x + (parameters.mapSizeX / 2), parameters.spawnAreaSize.y + (parameters.mapSizeY / 2));

        // Fill the map with items and foes
        for (int i = 0; i < mapAreas.Count; i++)
            cells = fillMap.FillMap(cells, mapAreas[i]);

        isRunning = false;
    }
    
    private void CreatePlayerSpawn(int startIndexX, int startIndexY, int endIndexX, int endIndexY)
    {
        for (int y = startIndexY; y < endIndexY; y++)
        {
            for (int x = startIndexX; x < endIndexX; x++)
            {
                // Check if the cell is free
                if (cells[x, y].state == Cell.CellState.WALKABLE)
                {
                    cells[x, y].state = Cell.CellState.PLAYERSPAWN;
                }
            }
        }
    }
}
