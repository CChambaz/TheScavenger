using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoesManager : MonoBehaviour
{
    [Header("Foe pool object parameters")]
    [SerializeField] private Transform poolPosition;
    [SerializeField] private int initialPoolSize;
    [SerializeField] private GameObject[] foePrefabs;
    
    private List<FoeController> activeFoes = new List<FoeController>();
    private List<FoeController> inactiveFoes = new List<FoeController>();
    private List<FoeController> fightingFoes = new List<FoeController>();
    
    // Start is called before the first frame update
    void Start()
    {
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
        if (fightingFoes.Count > 0)
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
    
    public void SpawnFoe(Vector3 position)
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
        spawningFoe.ResetFoe();
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
