using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map parameters")]
public class MapParameters : ScriptableObject
{
    [Header("Map size")]
    [SerializeField] public int mapSizeX;
    [SerializeField] public int mapSizeY;

    [Header("Cell size")]
    [SerializeField] public Vector3 cellSize;

    [Header("BSP parameter")]
    [SerializeField] public int minAreaSize;
    [SerializeField] public int maxAreaSize;
    
    [Header("Common building parameters")]
    [Range(2, 15)] [SerializeField] public int minEmptyCellsBetweenBuilding;

    [Header("Closed building parameters")]
    [Range(3, 15)] [SerializeField] public int cbMinSizeX;
    [Range(6, 25)] [SerializeField] public int cbMaxSizeX;

    [Range(3, 15)] [SerializeField] public int cbMinSizeY;
    [Range(6, 25)] [SerializeField] public int cbMaxSizeY;

    [Header("Open building parameters")]
    [Range(3, 15)] [SerializeField] public int obMinSizeX;
    [Range(6, 25)] [SerializeField] public int obMaxSizeX;

    [Range(3, 15)] [SerializeField] public int obMinSizeY;
    [Range(6, 25)] [SerializeField] public int obMaxSizeY;

    [Header("Spawn chance")]
    [Range(0f, 1f)] [SerializeField] public float cbSpawnChance;
    [Range(0f, 1f)] [SerializeField] public float obSpawnChance;
    [Range(1, 15)] [SerializeField] public int forceOBSpawnAfter;
    
    [Header("Player spawn")]
    [SerializeField] public Vector2Int spawnAreaSize;
    
    [Header("Foes parameters")]
    [Range(0f, 1f)] [SerializeField] public float obFoesSpawnChance;
    [Range(0f, 1f)] [SerializeField] public float freeAreaFoesSpawnChance;
    [Range(10, 30)] [SerializeField] public int minStreetAreaToSpawnFoes;
    [Range(0, 5)] [SerializeField] public int minStreetFoes;
    [Range(1, 7)] [SerializeField] public int maxStreetFoes;
    [Range(10, 30)] [SerializeField] public int minOpenBuildingAreaToSpawnFoes;
    [Range(0, 5)] [SerializeField] public int minOpenBuildingFoes;
    [Range(1, 7)] [SerializeField] public int maxOpenBuildingFoes;

    [Header("Items parameters")]
    [Range(0f, 1f)] [SerializeField] public float itemSpawnChance;
    [Range(0, 5)] [SerializeField] public int minOpenBuildingItem;
    [Range(1, 7)] [SerializeField] public int maxOpenBuildingItem;
    [Range(1, 15)] [SerializeField] public int forceSpawnScrapWhenHasNotSpawnSince;

    [Header("Tiles")] 
    [SerializeField] public TilesReferences tilesReferences;

    [Header("Pathfinding parameters")]
    [SerializeField] public int pathfindingMaxCheckedNode;
}
