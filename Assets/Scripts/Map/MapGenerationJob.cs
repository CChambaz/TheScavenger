using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public struct MapGenerationJob : IJob
{
    enum OpenBorder
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }
    
    /* Native array emulating the state of the cells, values results:
        1 = WALKABLE
        2 = CLOSEDBUILDING
        3 = OPENDBUILDING
        4 = OPENBUILDINGGATE
        5 = SCRAPITEM
        6 = FOESPAWN
        7 = PLAYERSPAWN
    */
    public NativeArray<int> result;
    
    public int mapSizeX;
    public int mapSizeY;
    public int spawnAreaSizeX;
    public int spawnAreaSizeY;
    public int minAreaSize;
    public int maxAreaSize;
    public int minEmptyCellsBetweenBuilding;
    public int cbMinSizeX;
    public int cbMinSizeY;
    public int cbMaxSizeX;
    public int cbMaxSizeY;
    public int obMinSizeX;
    public int obMinSizeY;
    public int obMaxSizeX;
    public int obMaxSizeY;
    public float cbSpawnChance;
    public float obSpawnChance;
    public int minOpenBuildingFoes;
    public int maxOpenBuildingFoes;
    public int minStreetFoes;
    public int maxStreetFoes;
    public int minOpenBuildingItem;
    public int maxOpenBuildingItem;
    public int minOpenBuildingAreaToSpawnFoes;
    public int minStreetAreaToSpawnFoes;
    public float foesSpawnChance;
    public float itemSpawnChance;

    public Random random;
    
    // Start is called before the first frame update
    public void Execute()
    {
        // Set base value of the array
        for (int y = 0; y < mapSizeY; y++){
            for (int x = 0; x < mapSizeX; x++)
                result[x + (y * mapSizeX)] = 1;
        }
        
        List<int[]> mapAreas = new List<int[]>();
        
        // Generate the map areas
        mapAreas = DividMap();
        
        // Generate the buildings in all the areas
        for (int i = 0; i < mapAreas.Count; i++)
            result = GenerateBuildingsInArea(result, mapAreas[i][0], mapAreas[i][1], mapAreas[i][2], mapAreas[i][3]);

        // Place the player spawn in the central area of the map
        CreatePlayerSpawn(mapSizeX / 2, mapSizeY / 2, spawnAreaSizeX + (mapSizeX / 2), spawnAreaSizeY + (mapSizeY / 2));

        // Fill the map with items and foes
        for (int i = 0; i < mapAreas.Count; i += 4)
            result = FillMap(result, mapAreas[i]);
    }
    
    private void CreatePlayerSpawn(int startIndexX, int startIndexY, int endIndexX, int endIndexY)
    {
        for (int y = startIndexY; y < endIndexY; y++)
        {
            for (int x = startIndexX; x < endIndexX; x++)
            {
                // Check if the cell is free
                if (result[x + (y * mapSizeX)] == 1)
                {
                    result[x + (y * mapSizeX)] = 7;
                }
            }
        }
    }

#region Map divider
public List<int[]> DividMap()
{
    List<int[]> mapAreaList = new List<int[]>();
    
    for (int y = 0; y < mapSizeY; y++)
    {
        // Define the Y size of the area line
        int areaYSize = random.NextInt(minAreaSize, maxAreaSize);

        // Check if the area is still on the map limit
        if (y + areaYSize > mapSizeY)
            areaYSize = mapSizeY - y;

        for (int x = 0; x < mapSizeX; x++)
        {
            // Define the X size of the area
            int areaXSize = random.NextInt(minAreaSize, maxAreaSize);

            // Check if the area is still on the map limit
            if (x + areaXSize > mapSizeX)
                areaXSize = mapSizeX - x;

            // Create the current area
            //MapGenerator.MapArea currentArea;
            int[] currentArea = {0, 0, 0, 0};

            // Assign the min and max y index for to the current area
            currentArea[0] = x;
            currentArea[2] = x + areaXSize;

            // Assign the min and max y index for to the current area
            currentArea[1] = y;
            currentArea[3] = y + areaYSize;

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
#endregion
    
#region Building generation
    public NativeArray<int> GenerateBuildingsInArea(NativeArray<int> cells, int startIndexX, int startIndexY, int endIndexX, int endIndexY)
    {
        // Apply an offset in order to always have a gap between the area border and the building
        startIndexY += minEmptyCellsBetweenBuilding;
        endIndexY -= minEmptyCellsBetweenBuilding;
        startIndexX += minEmptyCellsBetweenBuilding;
        endIndexX -= minEmptyCellsBetweenBuilding;

        for (int y = startIndexY; y < endIndexY; y++)
        {
            for (int x = startIndexX; x < endIndexX; x++)
            {
                // if the current cell has already been set to a special state, go to the next
                if (cells[x + (y * mapSizeX)] != 1)
                    continue;

                // Random value that define if a building is going to be created
                float rnd = random.NextFloat(0f, 1f);

                int buildingSizeX = 0;
                int buildingSizeY = 0;

                // Define the size of the building
                if (rnd <= cbSpawnChance)
                {
                    buildingSizeX = random.NextInt(cbMinSizeX, cbMaxSizeX);
                    buildingSizeY = random.NextInt(cbMinSizeX, cbMaxSizeX);
                }
                else if (rnd <= obSpawnChance)
                {
                    buildingSizeX = random.NextInt(obMinSizeX, obMaxSizeX);
                    buildingSizeY = random.NextInt(obMinSizeX, obMaxSizeX);
                }
                else
                    continue;

                // Check if the size of the building is not outside of the map
                if (y + buildingSizeY > mapSizeY)
                    buildingSizeY = mapSizeY - y;

                if (x + buildingSizeX > mapSizeX)
                    buildingSizeX = mapSizeX - x;

                // Check if the building can be contained in the area
                if (y + buildingSizeY > endIndexY)
                    buildingSizeY = endIndexY - y;

                if (x + buildingSizeX > endIndexX)
                    buildingSizeX = endIndexX - x;
                
                // Check if the building can be built here
                if (!CheckBuildingPosition(cells, minEmptyCellsBetweenBuilding, x, y, buildingSizeX))
                    continue;

                // Create the building in the array
                if (rnd <= cbSpawnChance)
                {
                    // Check if the building size is still above the minimum
                    if (buildingSizeY < cbMinSizeY || buildingSizeX < cbMinSizeX)
                        continue;
                    
                    cells = SetCellsAsClosedBuilding(cells, x, y, buildingSizeX, buildingSizeY);

                    // Push to the last cell of the building
                    x += buildingSizeX;
                }
                else if (rnd <= obSpawnChance)
                {
                    // Check if the building size is still above the minimum
                    if (buildingSizeY < obMinSizeY || buildingSizeX < obMinSizeX)
                        continue;
                    
                    cells = SetCellsAsOpenBuilding(cells, x, y, buildingSizeX, buildingSizeY);

                    // Push to the last cell of the building
                    x += buildingSizeX;
                }
            }
        }

        //return cells;
        return cells;
    }

    NativeArray<int> SetCellsAsClosedBuilding(NativeArray<int> cells, int indexX, int indexY, int sizeX, int sizeY)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int yb = 0; yb < sizeY; yb++)
            {
                cells[indexX + x + (indexY + yb) * mapSizeX] = 2;
            }
        }

        return cells;
    }

    NativeArray<int> SetCellsAsOpenBuilding(NativeArray<int> cells, int indexX, int indexY, int sizeX, int sizeY)
    {
        // Create the base structure
        for (int y = 0; y < sizeY; y++)
        {
            // Check if the current cell is the first or last line (need to be fullfiled)
            if (y == 0 || y == sizeY - 1)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    cells[indexX + x + ((indexY + y) * mapSizeX)] = 3;
                }
            }
            // Otherwise, assigne the state to the first and last cell on the line
            else
            {
                cells[indexX + (indexY + y) * mapSizeX] = 3;
                cells[indexX + sizeX - 1 + (indexY + y) * mapSizeX] = 3;
            }
        }

        // Define where to put the entrance
        float rnd = random.NextFloat(0f, 1f);

        OpenBorder openBorder;

        bool isOnTheBottom = indexY <= 0;
        bool isOnTheTop = indexY + sizeY >= mapSizeY;
        bool isOnTheLeft = indexX <= 0;
        bool isOnTheRight = indexX + sizeX >= mapSizeX;

        // Check if the building is on the bottom left corner
        if (isOnTheBottom && isOnTheLeft)
            openBorder = rnd <= 0.5f ? OpenBorder.TOP : OpenBorder.RIGHT;
        // Check if the building is on the bottom right corner
        else if (isOnTheBottom && isOnTheRight)
            openBorder = rnd <= 0.5f ? OpenBorder.TOP : OpenBorder.LEFT;
        // Check if the building is on the top left corner
        else if (isOnTheTop && isOnTheLeft)
            openBorder = rnd <= 0.5f ? OpenBorder.BOTTOM : OpenBorder.RIGHT;
        // Check if the building is on the top right corner
        else if (isOnTheTop && isOnTheRight)
            openBorder = rnd <= 0.5f ? OpenBorder.BOTTOM : OpenBorder.LEFT;
        // Check if the building is on the bottom
        else if(isOnTheBottom)
        {
            if (rnd <= 0.33f)
                openBorder = OpenBorder.TOP;
            else if (rnd <= 0.66f)
                openBorder = OpenBorder.LEFT;
            else
                openBorder = OpenBorder.RIGHT;
        }
        // Check if the building is on the left
        else if (isOnTheLeft)
        {
            if (rnd <= 0.33f)
                openBorder = OpenBorder.TOP;
            else if (rnd <= 0.66f)
                openBorder = OpenBorder.BOTTOM;
            else
                openBorder = OpenBorder.RIGHT;
        }
        // Check if the building is on the top
        else if (isOnTheTop)
        {
            if (rnd <= 0.33f)
                openBorder = OpenBorder.LEFT;
            else if (rnd <= 0.66f)
                openBorder = OpenBorder.BOTTOM;
            else
                openBorder = OpenBorder.RIGHT;
        }
        // Check if the building is on the right
        else if (isOnTheRight)
        {
            if (rnd <= 0.33f)
                openBorder = OpenBorder.LEFT;
            else if (rnd <= 0.66f)
                openBorder = OpenBorder.BOTTOM;
            else
                openBorder = OpenBorder.TOP;
        }
        else
        {
            if (rnd <= 0.25f)
                openBorder = OpenBorder.TOP;
            else if (rnd <= 0.5f)
                openBorder = OpenBorder.BOTTOM;
            else if (rnd <= 0.75f)
                openBorder = OpenBorder.LEFT;
            else
                openBorder = OpenBorder.RIGHT;
        }

        int fixedValue = 0;
        bool xIsStatic = false;
        
        switch (openBorder)
        {
            case OpenBorder.TOP:
                fixedValue = indexY + sizeY - 1;
                break;
            case OpenBorder.BOTTOM:
                fixedValue = indexY;
                break;
            case OpenBorder.LEFT:
                xIsStatic = true;
                fixedValue = indexX;
                break;
            case OpenBorder.RIGHT:
                xIsStatic = true;
                fixedValue = indexX + sizeX - 1;
                break;
        }

        // Add a way to enter the structure
        if (xIsStatic)
        {
            cells[fixedValue + (indexY + (sizeY / 2)) * mapSizeX] = 4;
            cells[fixedValue + (indexY + (sizeY / 2) + 1) * mapSizeX] = 4;
        }
        else
        {
            cells[indexX + (sizeX / 2) + (fixedValue * mapSizeX)] = 4;
            cells[indexX + (sizeX / 2) + 1 + (fixedValue * mapSizeX)] = 4;
        }

        return cells;
    }
    
    bool CheckBuildingPosition(NativeArray<int> cells, int minEmptyCellsBetweenBuilding, int indexX, int indexY, int sizeX)
    {
        int leftOffset;
        int rightOffset;

        // Check if too close from the left edge of the map to apply the minimum spacing
        if (indexX < minEmptyCellsBetweenBuilding)
            leftOffset = -indexX;
        else
            leftOffset = -minEmptyCellsBetweenBuilding;

        // Check if too close from the right edge of the map to apply the minimum spacing
        if (indexX + sizeX > mapSizeX - minEmptyCellsBetweenBuilding)
            rightOffset = mapSizeX - indexX - sizeX;
        else
            rightOffset = minEmptyCellsBetweenBuilding;

        // Check if the building can be built
        for (int x = leftOffset; x < sizeX + rightOffset; x++)
        {
            if (indexY >= minEmptyCellsBetweenBuilding)
            {
                for (int i = minEmptyCellsBetweenBuilding; i >= 0; i--)
                {
                    if (cells[indexX + x + (indexY - i) * mapSizeX] != 1)
                        return false;
                }
            }
            else
            {
                for (int i = indexY; i >= 0; i--)
                {
                    if (cells[indexX + x + (indexY - i) * mapSizeX] != 1)
                        return false;
                }
            }
        }
        return true;
    }
#endregion

#region Filling map
    public NativeArray<int> FillMap(NativeArray<int> cells, int[] area)
    {
        List<int[]> visitedCellsID = new List<int[]>();
        
        for (int y = area[1]; y < area[3]; y++)
        {
            for (int x = area[0]; x < area[2]; x++)
            {
                int[] currentCellID = {x, y};
                // Check if this node has already been visited
                if (visitedCellsID.Contains(currentCellID))
                    continue;
                
                if (cells[x + (y * mapSizeX)] == 3)
                {
                    // Check if this building has already been visited
                    if (cells[x + (y - 1) * mapSizeX] == 3 ||
                        cells[x + (y - 1) * mapSizeX] == 4)
                    {
                        // Go to the end of the building
                        x += GetOpenBuildingSize(cells, x, y)[0] - 1;
                        continue;
                    }

                    if (cells[x - 1 + (y * mapSizeX)] != 1)
                        continue;

                    cells = FillOpenBuildingAt(cells, x, y);
                    
                    int[] currentBuildingSize = GetOpenBuildingSize(cells, x, y);

                    for (int i = x; i < x + currentBuildingSize[0]; i++)
                    {
                        int[] cellID = {x, 0};
                        for (int j = y; j < y + currentBuildingSize[1]; j++)
                        {
                            cellID[1] = j;
                            visitedCellsID.Add(cellID);
                        }
                    }
                    
                    continue;
                }

                if (cells[x + (y * mapSizeX)] == 1)
                {
                    cells = SpawnFoesOnStreet(cells, area, x, y);
                    
                    int[] currentFreeAreaSize = GetFreeAreaSize(cells, area, x, y);

                    for (int i = x; i < x + currentFreeAreaSize[0]; i++)
                    {
                        int[] cellID = {x, 0};
                        for (int j = y; j < y + currentFreeAreaSize[1]; j++)
                        {
                            cellID[1] = j;
                            visitedCellsID.Add(cellID);
                        }
                    }
                }
            }
        }

        return cells;
    }

    NativeArray<int> FillOpenBuildingAt(NativeArray<int> cells, int indexX, int indexY)
    {
        int[] buildingSize = GetOpenBuildingSize(cells, indexX, indexY);

        int[] buildingGatePosition = GetOpenBuildingGatePosition(cells, indexX, indexY, indexX + buildingSize[0], indexY + buildingSize[1]);

        int foesToSpawn = random.NextInt(minOpenBuildingFoes, maxOpenBuildingFoes);
        int itemToSpawn = random.NextInt(minOpenBuildingFoes, maxOpenBuildingFoes);

        float rnd = random.NextFloat(0f, 1f);

        // Define if there is enough place to spawn foes
        if ((buildingSize[0] - 2) * (buildingSize[1] - 2) > minOpenBuildingAreaToSpawnFoes || rnd > foesSpawnChance)
            foesToSpawn = 0;

        if (rnd > itemSpawnChance)
            itemToSpawn = 0;

        int itemSpawnedCount = 0;

        // Check the gate position and spawn items containers on the opposite side
        // Gate on top
        if (buildingGatePosition[1] >= indexY + buildingSize[1] - 1)
        {
            for (int i = indexX + 1; i < indexX + buildingSize[0] - 1; i++)
            {
                if (itemSpawnedCount < itemToSpawn)
                {
                    cells[i + (indexY + 1) * mapSizeX] = 5;
                    itemSpawnedCount++;
                }
            }
        }
        // Gate on right
        else if (buildingGatePosition[0] >= indexX + buildingSize[0] - 1)
        {
            for (int i = indexY + 1; i < indexY + buildingSize[1] - 1; i++)
            {
                if (itemSpawnedCount < itemToSpawn)
                {
                    cells[indexX + 1 + (i * mapSizeX)] = 5;
                    itemSpawnedCount++;
                }
            }
        }
        // Gate on left
        else if (buildingGatePosition[1] > indexY && buildingGatePosition[0] == indexX)
        {
            for (int i = indexY + 1; i < indexY + buildingSize[1] - 1; i++)
            {
                if (itemSpawnedCount < itemToSpawn)
                {
                    cells[indexX + buildingSize[0] - 2 + (i * mapSizeX)] = 5;
                    itemSpawnedCount++;
                }
            }
        }
        // Gate on bottom
        else if(buildingGatePosition[0] > indexX && buildingGatePosition[1] == indexY)
        {
            for (int i = indexX + 1; i < indexX + buildingSize[1] - 1; i++)
            {
                if (itemSpawnedCount < itemToSpawn)
                {
                    cells[i + (indexY + buildingSize[1] - 2) * mapSizeX] = 5;
                    itemSpawnedCount++;
                }
            }
        }

        for (int y = indexY + 1; y < indexY + buildingSize[1] - 1; y++)
        {
            for (int x = indexX + 1; x < indexX + buildingSize[0] - 1; x++)
            {
                if (foesToSpawn <= 0)
                    break;
                
                if (cells[x + (y * mapSizeX)] == 1)
                {
                    cells[x + (y * mapSizeX)] = 6;
                    foesToSpawn--;
                }
            }
        }

        return cells;
    }

    NativeArray<int> SpawnFoesOnStreet(NativeArray<int> cells, int[] area, int indexX, int indexY)
    {
        int[] freeAreaSize = GetFreeAreaSize(cells, area, indexX, indexY);
        
        int foesToSpawn = random.NextInt(minStreetFoes, maxStreetFoes);

        float rnd = random.NextFloat(0f, 1f);

        if (freeAreaSize[0] * freeAreaSize[1] > minStreetAreaToSpawnFoes && rnd > foesSpawnChance)
            foesToSpawn = 0;

        for (int y = indexY; y < indexY + freeAreaSize[1]; y++)
        {
            for (int x = indexX; x < indexX + freeAreaSize[0]; x++)
            {
                if (foesToSpawn <= 0)
                    break;
                
                if (cells[x + (y * mapSizeX)] == 1)
                {
                    cells[x + (y * mapSizeX)] = 6;
                    foesToSpawn--;
                }
            }
        }

        return cells;
    }

    int[] GetOpenBuildingSize(NativeArray<int> cells, int indexX, int indexY)
    {
        int[] buildingSize = {0, 0};

        int iterator = 0;

        // Define the X size of the building
        while (true)
        {
            // Check if not on the edge of the map
            if (indexX + iterator < mapSizeX)
            {
                // Check if still in the building
                if (cells[indexX + iterator + (indexY * mapSizeX)] != 3 && 
                    cells[indexX + iterator + (indexY * mapSizeX)] != 4)
                    break;

                iterator++;
            }
            else
                break;
        }

        buildingSize[0] = iterator;
        iterator = 0;

        // Define the Y size of the building
        while (true)
        {
            // Check if not on the edge of the map
            if (indexY + iterator < mapSizeY)
            {
                // Check if still in the building
                if (cells[indexX + (indexY + iterator) * mapSizeX] != 3 &&
                    cells[indexX + (indexY + iterator) * mapSizeX] != 4)
                    break;

                iterator++;
            }
            else
                break;
        }

        buildingSize[1] = iterator;

        return buildingSize;
    }

    int[] GetOpenBuildingGatePosition(NativeArray<int> cells, int startIndexX, int startIndexY, int endIndexX, int endIndexY)
    {
        int[] gatePosition = {0, 0};

        for (int y = startIndexY; y < endIndexY; y++)
        {
            for (int x = startIndexX; x < endIndexX; x++)
            {
                if (cells[x + (y * mapSizeX)] == 4)
                {
                    gatePosition[0] = x;
                    gatePosition[1] = y;

                    return gatePosition;
                }
            }
        }

        return gatePosition;
    }
    int[] GetFreeAreaSize(NativeArray<int> cells, int[] area, int indexX, int indexY)
    {
        int[] freeAreaSize = {0, 0};

        int iterator = 0;

        // Define the X size of the free area
        while (true)
        {
            // Check if still in the area
            if (indexX + iterator < area[2])
            {
                // Check if still in a free area
                if (cells[indexX + iterator + (indexY * mapSizeX)] != 1)
                    break;

                iterator++;
            }
            else
            {
                break;
            }
        }

        freeAreaSize[0] = iterator;
        iterator = 0;

        bool stillInFreeArea = true;

        // Define the Y size of the building
        while (true)
        {
            // Check if still in the area
            if (indexY + iterator < area[3])
            {
                // Check if still in a free area
                for (int x = indexX; x < indexX + freeAreaSize[0]; x++)
                {
                    if (cells[x + (indexY + iterator) * mapSizeX] != 1)
                        stillInFreeArea = false;
                }

                if (!stillInFreeArea)
                    break;

                iterator++;
            }
            else
                break;
        }

        freeAreaSize[1] = iterator;

        return freeAreaSize;
    }
#endregion
}
