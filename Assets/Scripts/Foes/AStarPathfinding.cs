using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding
{
    private MapParameters parameters;

    public AStarPathfinding(MapParameters parameters)
    {
        this.parameters = parameters;
    }
    
    public List<Vector3> GetPathTo(Grid grid, Vector2 startPosition, Vector2 targetPosition)
    {
        // Check if the target is outside of the map
        if (targetPosition.x < 0 || targetPosition.x >= parameters.mapSizeX || targetPosition.y < 0 || targetPosition.y >= parameters.mapSizeY)
            return null;
        
        // Get the node IDs of the current position and of the target
        Vector2Int currentNodeID = grid.GetNearestWalkableNode(startPosition);
        Vector2Int targetNodeID = grid.GetNearestWalkableNode(targetPosition);
        
        // Get the node of the current position and of the target
        GridNode startingNode = grid.nodes[currentNodeID.x, currentNodeID.y];
        GridNode targetNode = grid.nodes[targetNodeID.x, targetNodeID.y];
        
        // Check if the target can be reached
        if (!targetNode.walkable)
        {
            targetNode = grid.nodes[targetNodeID.x, targetNodeID.y - 1];
        }

        List<GridNode> openList = new List<GridNode>();
        List<GridNode> closedList = new List<GridNode>();
        
        // Add the starting node to the open list
        openList.Add(startingNode);

        GridNode currentNode;
        
        while (openList.Count > 0)
        {
            // Get the first node of the list
            currentNode = openList[0];
            
            // Check if there is a node with a lower movement cost in the opent list
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fullMovementCost <= currentNode.fullMovementCost  &&
                    openList[i].heuristicCost < currentNode.heuristicCost)
                    currentNode = openList[i];
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
            
            // Check if target has been reach
            if (currentNode == targetNode)
                break;
            
            foreach (GridNode node in currentNode.neighbours)
            {
                // Check if the node is walkable
                if (!node.walkable || closedList.Contains(node))
                    continue;
                
                // Define the movement cost of the current neighbour
                float newMoveCost = currentNode.movementCost + GetDistanceBetween(currentNode, node);

                if (newMoveCost < node.movementCost || !openList.Contains(node))
                {
                    node.movementCost = newMoveCost;
                    node.heuristicCost = GetDistanceBetween(targetNode, node);
                    node.parent = currentNode;
                    
                    if(!openList.Contains(node))
                        openList.Add(node);
                }
            }
        }

        currentNode = targetNode;
        
        // Create the point forming the path to reach the target
        List<Vector3> path = new List<Vector3>();
        
        while (currentNode != startingNode)
        {
            path.Add(new Vector3(currentNode.gridPositionX, currentNode.gridPositionY));
            currentNode = currentNode.parent;
        }

        return path;
    }
    
    float GetDistanceBetween(GridNode target, GridNode origin)
    {
        return Mathf.Abs(target.gridPositionX - origin.gridPositionX) + Mathf.Abs(target.gridPositionY - origin.gridPositionY);
    }
}
