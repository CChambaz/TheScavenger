using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    /*public enum NodeState
    {
        WALKABLE,
        CLOSEDBUILDING,
        OPENDBUILDING,
        OPENBUILDINGGATE,
        SCRAPITEM,
        FOESPAWN,
        PLAYERSPAWN
    }
    
    public NodeState state;*/
    public int gridIndexX;
    public int gridIndexY;
    public float gridPositionX;
    public float gridPositionY;
    
    public float movementCost;
    public float heuristicCost;
    public List<GridNode> neighbours;
    public GridNode parent;

    public GridNode(int indexX, int indexY, float positionX, float positionY)
    {
        gridIndexX = indexX;
        gridIndexY = indexY;
        gridPositionX = positionX;
        gridPositionY = positionY;
    }
    
    public float fullMovementCost
    {
        get { return movementCost + heuristicCost; }
    }

    public bool walkable
    {
        get { return movementCost > 0f; }
    }
}
