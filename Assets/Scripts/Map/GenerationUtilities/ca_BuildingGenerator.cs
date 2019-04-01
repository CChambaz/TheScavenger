using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ca_BuildingGenerator
{
     private MapParameters parameters;
    enum OpenBorder
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    private int obNotSpawnSince = 0;

    public ca_BuildingGenerator(MapParameters parameters)
    {
        this.parameters = parameters;
    }
    
    public Cell[,] GenerateBuildingsInArea(Cell[,] cells, int startIndexX, int startIndexY, int endIndexX, int endIndexY)
    {
        // Apply an offset in order to always have a gap between the area border and the building
        startIndexY += parameters.minEmptyCellsBetweenBuilding;
        endIndexY -= parameters.minEmptyCellsBetweenBuilding;
        startIndexX += parameters.minEmptyCellsBetweenBuilding;
        endIndexX -= parameters.minEmptyCellsBetweenBuilding;

        for (int y = startIndexY; y < endIndexY; y++)
        {
            for (int x = startIndexX; x < endIndexX; x++)
            {
                // if the current cell has already been set to a special state, go to the next
                if (cells[x, y].state != Cell.CellState.WALKABLE)
                    continue;

                // Random value that define if a building is going to be created
                float rnd = Random.Range(0f, 1f);

                int buildingSizeX = 0;
                int buildingSizeY = 0;

                // Define the size of the building
                if (rnd <= parameters.cbSpawnChance && obNotSpawnSince < parameters.forceOBSpawnAfter)
                {
                    buildingSizeX = Random.Range(parameters.cbMinSizeX, parameters.cbMaxSizeX);
                    buildingSizeY = Random.Range(parameters.cbMinSizeY, parameters.cbMaxSizeY);
                }
                else if (rnd <= parameters.obSpawnChance || obNotSpawnSince >= parameters.forceOBSpawnAfter)
                {
                    buildingSizeX = Random.Range(parameters.obMinSizeX, parameters.obMaxSizeX);
                    buildingSizeY = Random.Range(parameters.obMinSizeY, parameters.obMaxSizeY);
                }
                else
                    continue;

                // Check if the size of the building is not outside of the map
                if (y + buildingSizeY > parameters.mapSizeY)
                    buildingSizeY = parameters.mapSizeY - y;

                if (x + buildingSizeX > parameters.mapSizeX)
                    buildingSizeX = parameters.mapSizeX - x;

                // Check if the building can be contained in the area
                if (y + buildingSizeY > endIndexY)
                    buildingSizeY = endIndexY - y;

                if (x + buildingSizeX > endIndexX)
                    buildingSizeX = endIndexX - x;
                
                // Check if the building can be built here
                if (!CheckBuildingPosition(cells, parameters.minEmptyCellsBetweenBuilding, x, y, buildingSizeX))
                    continue;

                // Create the building in the array
                if (rnd <= parameters.cbSpawnChance && obNotSpawnSince < parameters.forceOBSpawnAfter)
                {
                    // Check if the building size is still above the minimum
                    if (buildingSizeY < parameters.cbMinSizeY || buildingSizeX < parameters.cbMinSizeX)
                        continue;
                    
                    cells = SetCellsAsClosedBuilding(cells, x, y, buildingSizeX, buildingSizeY);

                    // Push to the last cell of the building
                    x += buildingSizeX;

                    obNotSpawnSince++;
                }
                else if (rnd <= parameters.obSpawnChance || obNotSpawnSince >= parameters.forceOBSpawnAfter)
                {
                    // Check if the building size is still above the minimum
                    if (buildingSizeY < parameters.obMinSizeY || buildingSizeX < parameters.obMinSizeX)
                        continue;
                    
                    cells = SetCellsAsOpenBuilding(cells, x, y, buildingSizeX, buildingSizeY);

                    // Push to the last cell of the building
                    x += buildingSizeX;

                    obNotSpawnSince = 0;
                }
            }
        }

        //return cells;
        return cells;
    }

    Cell[,] SetCellsAsClosedBuilding(Cell[,] cells, int indexX, int indexY, int sizeX, int sizeY)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int yb = 0; yb < sizeY; yb++)
            {
                cells[indexX + x, indexY + yb].state = Cell.CellState.CLOSEDBUILDING;
            }
        }

        return cells;
    }

    Cell[,] SetCellsAsOpenBuilding(Cell[,] cells, int indexX, int indexY, int sizeX, int sizeY)
    {
        // Create the base structure
        for (int y = 0; y < sizeY; y++)
        {
            // Check if the current cell is the first or last line (need to be fullfiled)
            if (y == 0 || y == sizeY - 1)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    cells[indexX + x, indexY + y].state = Cell.CellState.OPENDBUILDING;
                }
            }
            // Otherwise, assigne the state to the first and last cell on the line
            else
            {
                cells[indexX, indexY + y].state = Cell.CellState.OPENDBUILDING;
                cells[indexX + sizeX - 1, indexY + y].state = Cell.CellState.OPENDBUILDING;
            }
        }

        // Define where to put the entrance
        float rnd = Random.Range(0f, 1f);

        OpenBorder openBorder;

        bool isOnTheBottom = indexY <= 0;
        bool isOnTheTop = indexY + sizeY >= parameters.mapSizeY;
        bool isOnTheLeft = indexX <= 0;
        bool isOnTheRight = indexX + sizeX >= parameters.mapSizeX;

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
            cells[fixedValue, indexY + (sizeY / 2)].state = Cell.CellState.OPENBUILDINGGATE;
            cells[fixedValue, indexY + (sizeY / 2) + 1].state = Cell.CellState.OPENBUILDINGGATE;
        }
        else
        {
            cells[indexX + (sizeX / 2), fixedValue].state = Cell.CellState.OPENBUILDINGGATE;
            cells[indexX + (sizeX / 2) + 1, fixedValue].state = Cell.CellState.OPENBUILDINGGATE;
        }

        return cells;
    }
    
    bool CheckBuildingPosition(Cell[,] cells, int minEmptyCellsBetweenBuilding, int indexX, int indexY, int sizeX)
    {
        int leftOffset;
        int rightOffset;

        // Check if too close from the left edge of the map to apply the minimum spacing
        if (indexX < minEmptyCellsBetweenBuilding)
            leftOffset = -indexX;
        else
            leftOffset = -minEmptyCellsBetweenBuilding;

        // Check if too close from the right edge of the map to apply the minimum spacing
        if (indexX + sizeX > parameters.mapSizeX - minEmptyCellsBetweenBuilding)
            rightOffset = parameters.mapSizeX - indexX - sizeX;
        else
            rightOffset = minEmptyCellsBetweenBuilding;

        // Check if the building can be built
        for (int x = leftOffset; x < sizeX + rightOffset; x++)
        {
            if (indexY >= minEmptyCellsBetweenBuilding)
            {
                for (int i = minEmptyCellsBetweenBuilding; i >= 0; i--)
                {
                    if (cells[indexX + x, indexY - i].state != Cell.CellState.WALKABLE)
                        return false;
                }
            }
            else
            {
                for (int i = indexY; i >= 0; i--)
                {
                    if (cells[indexX + x, indexY - i].state != Cell.CellState.WALKABLE)
                        return false;
                }
            }
        }
        return true;
    }
}
