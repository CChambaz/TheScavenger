using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingManager : MonoBehaviour
{
    Queue<FoeController> pathAskedBy = new Queue<FoeController>();

    private FoeController currentPathAskBy;
    private GameManager gameManager;
    private Pathfinding pathFinder;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        pathFinder = new Pathfinding(gameManager.parameters);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if a path has been requested
        if (pathAskedBy.Count > 0)
        {
            // Get the first foe that has requested a path and remove it from the queue
            currentPathAskBy = pathAskedBy.Dequeue();

            // Check if the requested path does not need the full path
            if (currentPathAskBy.state == FoeController.State.WANDERING || currentPathAskBy.state == FoeController.State.FLEE)
                currentPathAskBy.path = pathFinder.GetAStarPathTo(gameManager.grid, currentPathAskBy.transform.position,
                    currentPathAskBy.target.position, true);
            else
                currentPathAskBy.path = pathFinder.GetAStarPathTo(gameManager.grid, currentPathAskBy.transform.position,
                    currentPathAskBy.target.position);
        }
    }

    public void RegisterToQueue(FoeController foeController)
    {
        // Check if the requester is already in the queue
        if (pathAskedBy.Contains(foeController))
            return;
        
        pathAskedBy.Enqueue(foeController);
    }
}
