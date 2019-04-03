using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public struct GridUpdateJob : IJob
{
    public NativeArray<int> result;
    public NativeArray<int> cells;
    
    public int mapSizeX;
    public int mapSizeY;
    
    public void Execute()
    {
        // Update the state of the nodes
        for (int x = 0; x < mapSizeX - 1; x++)
        {
            for (int y = 0; y < mapSizeY - 1; y++)
            {
                if (!NativeCellIsWalkable(x + (y * mapSizeX)) ||
                    !NativeCellIsWalkable(x + 1 + (y * mapSizeX)) ||
                    !NativeCellIsWalkable(x + (y + 1) * mapSizeX) ||
                    !NativeCellIsWalkable(x + 1 + (y + 1) * mapSizeX))
                    result[x + (y * (mapSizeX - 1))] = 0;
                else
                    result[x + (y * (mapSizeX - 1))] = 1;
            }
        }
    }

    private bool NativeCellIsWalkable(int cellID)
    {
        if (cells[cellID] != 1 &&
            cells[cellID] != 4 &&
            cells[cellID] != 6 &&
            cells[cellID] != 7)
            return false;

        return true;
    }
}
