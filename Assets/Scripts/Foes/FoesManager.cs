using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoesManager : MonoBehaviour
{
    [Header("Foe pool object parameters")]
    [SerializeField] private Transform poolPosition;
    [SerializeField] private int initialPoolSize;
    [SerializeField] private GameObject[] foePrefabs;

    [Header("Night spawn parameters")] 
    [SerializeField] private float nightSpawnCoolDown;
    [SerializeField] private Transform[] nightSpawner;
    [SerializeField] private int maxNightSpawnInARow;
    
    private List<FoeController> activeFoes = new List<FoeController>();
    private List<FoeController> inactiveFoes = new List<FoeController>();
    private List<FoeController> fightingFoes = new List<FoeController>();

    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        // Prepare the object pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            // Prepare the position with the offset
            Vector3 nextPosition = new Vector3(poolPosition.position.x + i, poolPosition.position.y);
            
            // Instantiate the foe
            GameObject foe = Instantiate(foePrefabs[0], nextPosition, Quaternion.identity);
            
            // Add it to the inactive list
            inactiveFoes.Add(foe.GetComponent<FoeController>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Apply moral check if there is foes fighting and if the game is in the day state
        if (fightingFoes.Count > 0 && gameManager.gameState == GameManager.GameState.INGAMEDAY)
        {
            foreach (FoeController foe in fightingFoes)
            {
                // Check if the foe has to flee
                if (foe.state != FoeController.State.FLEE && foe.morals <= 0)
                {
                    // Set the nearest foe that is not fighting as a target
                    foe.target = GetNearestNonFightingFoe(foe.transform.position);
                    foe.state = FoeController.State.FLEE;
                }
            }
        }
    }

    public IEnumerator NightSpawn()
    {
        int nextSpawnerToUseID = 0;
        int spawnInARow = 0;
        Vector2Int spawnNodeID = Vector2Int.zero;
        Vector2 spawnPostion = Vector2.zero;
        
        while (true)
        {
            if (gameManager.gameState == GameManager.GameState.INGAMENIGHT)
            {
                spawnNodeID = gameManager.grid.GetNearestWalkableNode(nightSpawner[nextSpawnerToUseID].position);

                // Check if a walkable node has been found
                if (spawnNodeID.x != -1 && spawnNodeID.y != -1)
                {
                    spawnPostion.x = gameManager.grid.nodes[spawnNodeID.x, spawnNodeID.y].gridPositionX;
                    spawnPostion.y = gameManager.grid.nodes[spawnNodeID.x, spawnNodeID.y].gridPositionY;
                    
                    SpawnFoe(spawnPostion, true);

                    spawnInARow++;
                    
                    if (spawnInARow >= maxNightSpawnInARow)
                    {
                        spawnInARow = 0;
                        yield return new WaitForSeconds(nightSpawnCoolDown);
                    }
                }

                if (nextSpawnerToUseID < nightSpawner.Length - 1)
                    nextSpawnerToUseID++;
                else
                    nextSpawnerToUseID = 0;
            }
            
            yield return new WaitForEndOfFrame();
        }
    }

    public void RegisterToFightingList(FoeController foe)
    {
        if(!fightingFoes.Contains(foe))
            fightingFoes.Add(foe);
    }
    
    public void UnregisterToFightingList(FoeController foe)
    {
        if(fightingFoes.Contains(foe))
            fightingFoes.Remove(foe);
    }
    
    public void ReduceFightingFoesMoral(FoeController emitter, int amount)
    {
        if (gameManager.gameState != GameManager.GameState.INGAMEDAY)
            return;
        
        foreach (FoeController foe in fightingFoes)
        {
            // Check if not the emitter
            if(foe != emitter)
                foe.morals -= amount;
        }    
    }
    
    private Transform GetNearestNonFightingFoe(Vector3 position)
    {
        Transform nearestFoe = null;
        
        foreach (FoeController foe in activeFoes)
        {
            // Check if the foe is already fighting
            if (fightingFoes.Contains(foe) || foe.state == FoeController.State.DEAD)
                continue;

            if (nearestFoe == null ||
                (nearestFoe.position - position).magnitude > (foe.transform.position - position).magnitude)
            {
                nearestFoe = foe.transform;
            }
        }

        return nearestFoe;
    }

    public void SetAllActiveFoesHostile()
    {
        foreach (FoeController foe in activeFoes)
        {
            // Check if the foe has to be set as hostile
            if (foe.state == FoeController.State.DEAD || 
                foe.state == FoeController.State.ATTACK ||
                foe.state == FoeController.State.HITTED ||
                foe.state == FoeController.State.POSITIONING)
                continue;

            foe.target = foe.playerTransform;
            foe.state = FoeController.State.POSITIONING;
            RegisterToFightingList(foe);
        }
    }
    
    public void SpawnFoe(Vector3 position, bool hostileSpawn = false)
    {
        FoeController spawningFoe;
        
        // Check if all the foes have been spawned
        if (inactiveFoes.Count == 0)
        {
            // Instantiate a new foe
            GameObject foe = Instantiate(foePrefabs[0], position, Quaternion.identity);

            spawningFoe = foe.GetComponent<FoeController>();
        }
        else
        {
            // Get the last foe in the inactive list    
            spawningFoe = inactiveFoes[inactiveFoes.Count - 1];

            // Move the foe to the given position
            spawningFoe.transform.position = position;
            
            // Remove the foe from the inactive list
            inactiveFoes.Remove(spawningFoe);
        }
        
        // Add the foe to the active list
        activeFoes.Add(spawningFoe);

        spawningFoe.isActive = true;

        // Reset the foe
        spawningFoe.ResetFoe(hostileSpawn);
    }
    
    public void DestroyAllFoe()
    {
        while (activeFoes.Count > 0)
        {
            // Disable the first foe
            activeFoes[0].isActive = false;
            
            // Add it to the inactive list
            inactiveFoes.Add(activeFoes[0]);
            
            // Remove it from the active list
            activeFoes.RemoveAt(0);
            
            // Set the foe position
            inactiveFoes[inactiveFoes.Count - 1].transform.position = new Vector3(poolPosition.position.x + inactiveFoes.Count, poolPosition.position.y);
        }
    }
}
