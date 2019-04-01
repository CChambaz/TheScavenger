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

    public MapGenerationJob mapGenerationJob;


    public bool isRunning = false;
    
    // Start is called before the first frame update
    public MapGenerator(MapParameters parameters, MapDrawer drawer)
    {
        mapGenerationJob = new MapGenerationJob();
        
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

        mapGenerationJob.cells = cells;
        mapGenerationJob.parameters = parameters;
        mapGenerationJob.fillMap = fillMap;
        mapGenerationJob.mapDivider = mapDivider;
        mapGenerationJob.buildingGenerator = buildingGenerator;
    }

    public IEnumerator GenerateMap()
    {
        isRunning = true;
        
        mapGenerationJob.Execute();

        while (mapGenerationJob.isRunning)
            yield return new WaitForEndOfFrame();

        cells = mapGenerationJob.cells;
        
        mapDrawer.DrawMap(cells);

        isRunning = false;
    }
}
