using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles references")]
public class TilesReferences : ScriptableObject
{
    [Header("Ground tile")]
    [SerializeField] public Tile ground;
    
    [Header("Border tile")]
    [SerializeField] public Tile simpleWall;
    
    [Header("Closed building tile")]
    [SerializeField] public Tile cbBaseWallBottom;
    [SerializeField] public Tile cbBaseWallMid;
    [SerializeField] public Tile cbBaseWallTop;
    [SerializeField] public Tile cbTopWall;
    [SerializeField] public Tile cbBottomCornerLeftBottom;
    [SerializeField] public Tile cbBottomCornerLeftMid;
    [SerializeField] public Tile cbBottomCornerLeftTop;
    [SerializeField] public Tile cbBottomCornerRightBottom;
    [SerializeField] public Tile cbBottomCornerRightMid;
    [SerializeField] public Tile cbBottomCornerRightTop;
    [SerializeField] public Tile cbTopCornerLeft;
    [SerializeField] public Tile cbTopCornerRight;
    [SerializeField] public Tile cbInnerRoof;
    [SerializeField] public Tile cbInnerRoofLeftSide;
    [SerializeField] public Tile cbInnerRoofRightSide;
    [SerializeField] public Tile cbInnerRoofBottom;
    [SerializeField] public Tile cbInnerRoofBottomLeftSide;
    [SerializeField] public Tile cbInnerRoofBottomRightSide;

    [Header("Open building tile")]
    [SerializeField] public Tile obWallTop;
    [SerializeField] public Tile obWallBottom;
    [SerializeField] public Tile obWallLeft;
    [SerializeField] public Tile obWallRight;
    [SerializeField] public Tile obCornerTopLeft;
    [SerializeField] public Tile obCornerTopRight;
    [SerializeField] public Tile obCornerBottomLeft;
    [SerializeField] public Tile obCornerBottomRight;
}
