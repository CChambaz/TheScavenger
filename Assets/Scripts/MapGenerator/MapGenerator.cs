using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    public struct MapArea
    {
        public Vector2Int startIndex;
        public Vector2Int endIndex;
    }
    
    public Cell[,] cells;

    private List<MapArea> mapAreaList;
    private MapParameters parameters;
    
    private bsp_MapDivider mapDivider;
    private ca_BuildingGenerator buildingGenerator;
    private ca_FillMap fillMap;
    private MapDrawer mapDrawer;
    
    // Start is called before the first frame update
    public MapGenerator(MapParameters parameters, MapDrawer drawer)
    {
        this.parameters = parameters;

        mapDrawer = drawer;
        mapDivider = new bsp_MapDivider(this.parameters);
        buildingGenerator = new ca_BuildingGenerator(this.parameters);
        fillMap = new ca_FillMap(this.parameters);
        
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

    public void CreateMap()
    {
        for (int x = 0; x < parameters.mapSizeX; x++) {
            for (int y = 0; y < parameters.mapSizeY; y++)
                cells[x, y].state = Cell.CellState.WALKABLE;
        }

        // Generate the map areas
        mapAreaList = mapDivider.DividMap();

        // Generate the buildings in all the areas
        for (int i = 0; i < mapAreaList.Count; i++)
            cells = buildingGenerator.GenerateBuildingsInArea(cells, mapAreaList[i].startIndex.x, mapAreaList[i].startIndex.y, mapAreaList[i].endIndex.x, mapAreaList[i].endIndex.y);

        // Place the player spawn in the central area of the map
        CreatePlayerSpawn(parameters.mapSizeX / 2, parameters.mapSizeY / 2, parameters.spawnAreaSize.x + (parameters.mapSizeX / 2), parameters.spawnAreaSize.y + (parameters.mapSizeY / 2));

        // Fill the map with items and foes
        for (int i = 0; i < mapAreaList.Count; i++)
            cells = fillMap.FillMap(cells, mapAreaList[i]);

        mapDrawer.DrawMap(cells);
    }
    
    public void CreatePlayerSpawn(int startIndexX, int startIndexY, int endIndexX, int endIndexY)
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
    
    public Vector2Int GetCellIDFromPosition(Vector2 position)
    {
        Vector2Int index = Vector2Int.zero;

        for (int x = 0; x < parameters.mapSizeX; x++)
        {
            for (int y = 0; y < parameters.mapSizeY; y++)
            {
                float minPosX = cells[x, y].positionX - (parameters.cellSize.x / 2);
                float minPosY = cells[x, y].positionY - (parameters.cellSize.y / 2);

                float maxPosX = cells[x, y].positionX + (parameters.cellSize.x / 2);
                float maxPosY = cells[x, y].positionY + (parameters.cellSize.y / 2);
                
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
