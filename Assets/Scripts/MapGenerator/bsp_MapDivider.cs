using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bsp_MapDivider
{
    private MapParameters parameters;
    public bsp_MapDivider(MapParameters parameters)
    {
        this.parameters = parameters;
    }
    
    public List<Grid.MapArea> DividMap()
    {
        List<Grid.MapArea> mapAreaList = new List<Grid.MapArea>();

        for (int y = 0; y < parameters.mapSizeY; y++)
        {
            // Define the Y size of the area line
            int areaYSize = Random.Range(parameters.minAreaSize, parameters.maxAreaSize);

            // Check if the area is still on the map limit
            if (y + areaYSize > parameters.mapSizeY)
                areaYSize = parameters.mapSizeY - y;

            for (int x = 0; x < parameters.mapSizeX; x++)
            {
                // Define the X size of the area
                int areaXSize = Random.Range(parameters.minAreaSize, parameters.maxAreaSize);

                // Check if the area is still on the map limit
                if (x + areaXSize > parameters.mapSizeX)
                    areaXSize = parameters.mapSizeX - x;

                // Create the current area
                //MapGenerator.MapArea currentArea;
                Grid.MapArea currentArea;

                // Set the values to zero
                currentArea.startIndex = Vector2Int.zero;
                currentArea.endIndex = Vector2Int.zero;

                // Assign the min and max y index for to the current area
                currentArea.startIndex.x = x;
                currentArea.endIndex.x = x + areaXSize;

                // Assign the min and max y index for to the current area
                currentArea.startIndex.y = y;
                currentArea.endIndex.y = y + areaYSize;

                // Add the current area to the area list
                mapAreaList.Add(currentArea);

                // Go to the next area
                x += areaXSize;
            }

            // Go to the next line of areas
            y += areaYSize;
        }
        
        return mapAreaList;
    }
}
