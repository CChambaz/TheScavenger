using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public struct MapArea
    {
        public Vector2Int startIndex;
        public Vector2Int endIndex;
    }
    
    public GridNode[,] nodes;

    private MapParameters parameters;
    
    private bsp_MapDivider mapDivider;
    private ca_BuildingGenerator buildingGenerator;
    private ca_FillMap fillMap;
    private MapDrawer mapDrawer;

    public List<MapArea> mapAreaList = new List<MapArea>();
    
    public Grid(MapParameters parameters, MapDrawer mapGene)
    {
        this.parameters = parameters;

        mapDrawer = mapGene;
        
        mapDivider = new bsp_MapDivider(this.parameters);
        buildingGenerator = new ca_BuildingGenerator(this.parameters);
        fillMap = new ca_FillMap(this.parameters);
    }
    
    public void CreateGrid()
    {
        nodes = new GridNode[parameters.mapSizeX, parameters.mapSizeY];

        for (int x = 0; x < parameters.mapSizeX; x++)
        {
            float nodePosX = (x * parameters.cellSize.x) + (parameters.cellSize.x / 2);
            
            for (int y = 0; y < parameters.mapSizeY; y++)
            {
                float nodePosY = (y * parameters.cellSize.y) + (parameters.cellSize.y / 2);
                
                nodes[x, y] = new GridNode(x, y, nodePosX, nodePosY);
            }
        }
    }

    public void CreateMap()
    {
        for (int x = 0; x < parameters.mapSizeX; x++) {
            for (int y = 0; y < parameters.mapSizeY; y++)
                nodes[x, y].state = GridNode.NodeState.WALKABLE;

        }

        // Generate the map areas
        mapAreaList = mapDivider.DividMap();

        // Generate the buildings in all the areas
        for (int i = 0; i < mapAreaList.Count; i++)
            nodes = buildingGenerator.GenerateBuildingsInArea(nodes, mapAreaList[i].startIndex.x, mapAreaList[i].startIndex.y, mapAreaList[i].endIndex.x, mapAreaList[i].endIndex.y);

        // Place the player spawn in the central area of the map
        CreatePlayerSpawn(parameters.mapSizeX / 2, parameters.mapSizeY / 2, parameters.spawnAreaSize.x + (parameters.mapSizeX / 2), parameters.spawnAreaSize.y + (parameters.mapSizeY / 2));

        // Fill the map with items and foes
        for (int i = 0; i < mapAreaList.Count; i++)
            nodes = fillMap.FillMap(nodes, mapAreaList[i]);

        mapDrawer.DrawMap(nodes);
    }
    
    public void CreatePlayerSpawn(int startIndexX, int startIndexY, int endIndexX, int endIndexY)
    {
        for (int y = startIndexY; y < endIndexY; y++)
        {
            for (int x = startIndexX; x < endIndexX; x++)
            {
                // Check if the cell is free
                if (nodes[x, y].state == GridNode.NodeState.WALKABLE)
                {
                    nodes[x, y].state = GridNode.NodeState.PLAYERSPAWN;
                }
            }
        }
    }
    
    public List<GridNode> GetNeighbours(GridNode node)
    {
        List<GridNode> neighbours = new List<GridNode>();
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Check if equivalent to the current node
                if (x == 0 && y == 0)
                    continue;

                // Set the neighbour ID
                int indexX = node.gridIndexX + x;
                int indexY = node.gridIndexY + y;
                
                // Check if outside of the map (=> check if the node exist)
                if (indexX >= 0 && indexX < parameters.mapSizeX && indexY >= 0 && indexY < parameters.mapSizeY)
                    neighbours.Add(nodes[indexX, indexY]);
            }
        }

        return neighbours;
    }
    
    public Vector2Int GetNodeIDFromPosition(Vector2 position)
    {
        Vector2Int index = Vector2Int.zero;

        for (int x = 0; x < parameters.mapSizeX; x++)
        {
            for (int y = 0; y < parameters.mapSizeY; y++)
            {
                float minPosX = nodes[x, y].gridPositionX - (parameters.cellSize.x / 2);
                float minPosY = nodes[x, y].gridPositionY - (parameters.cellSize.y / 2);

                float maxPosX = nodes[x, y].gridPositionX + (parameters.cellSize.x / 2);
                float maxPosY = nodes[x, y].gridPositionY + (parameters.cellSize.y / 2);
                
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
