using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public enum CellState
    {
        WALKABLE,
        CLOSEDBUILDING,
        OPENDBUILDING,
        OPENBUILDINGGATE,
        SCRAPITEM,
        FOESPAWN,
        PLAYERSPAWN
    }
    
    public int indexX;
    public int indexY;
    public float positionX;
    public float positionY;

    public CellState state;

    public Cell(int indexX, int indexY, float posX, float posY)
    {
        this.indexX = indexX;
        this.indexY = indexY;
        positionX = posX;
        positionY = posY;
    }

    public bool walkable
    {
        get { return state == CellState.WALKABLE || state == CellState.OPENBUILDINGGATE || state == CellState.PLAYERSPAWN; }
    }
}
