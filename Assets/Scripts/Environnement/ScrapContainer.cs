using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapContainer : MonoBehaviour
{
    [Header("Object attribut")]
    [SerializeField] private int maxLife;

    [Header("Item spawn parameters")]
    [SerializeField] private int minScrapSpawn;
    [SerializeField] private int maxScrapSpawn;
    [Range(0f, 1f)] [SerializeField] private float lootSpawnChance;
    [Range(0f, 1f)] [SerializeField] private float scrapSmallSpawnChance;
    [Range(0f, 1f)] [SerializeField] private float scrapMediumSpawnChance;
    [Range(0f, 1f)] [SerializeField] private float scrapBigSpawnChance;

    [Header("Scrap prefab reference")]
    [SerializeField] private GameObject scrapSmallPrefab;
    [SerializeField] private GameObject scrapMediumPrefab;
    [SerializeField] private GameObject scrapBigPrefab;

    private int itemToSpawn = 0;
    private int life = 0;
    private int playerDamage = 0;
    private Vector2Int nodeID;
    
    private GameManager gameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        nodeID = gameManager.grid.GetNodeIDFromPosition(transform.position);
        life = maxLife;

        if(FindObjectOfType<PlayerController>() != null)
            playerDamage = FindObjectOfType<PlayerController>().attackDamage;

        // Define wheter the container will instantiate scrap when destroy or not
        float rnd = Random.Range(0f, 1f);

        if (rnd <= lootSpawnChance)
        {
            itemToSpawn = Random.Range(minScrapSpawn, maxScrapSpawn);
        }
    }

    void SpawnItems()
    {
        for (int i = 0; i < itemToSpawn; i++)
        {
            float rnd = Random.Range(0f, 1f);

            if (rnd <= scrapSmallSpawnChance)
                Instantiate(scrapSmallPrefab, transform.position, Quaternion.identity);
            else if (rnd <= scrapMediumSpawnChance)
                Instantiate(scrapMediumPrefab, transform.position, Quaternion.identity);
            else if (rnd <= scrapBigSpawnChance)
                Instantiate(scrapBigPrefab, transform.position, Quaternion.identity);
        }
        
        // TODO: Find a way to not destroy it or to update all the nodes and cell
        // Set the node on which the container was as walkable
        /*gameManager.grid.nodes[nodeID.x, nodeID.y].movementCost = 1f;
        
        Destroy(gameObject);*/
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "PlayerAttack")
        {
            if (!(playerDamage > 0))
                playerDamage = FindObjectOfType<PlayerController>().attackDamage;

            life -= playerDamage;

            if (life <= 0)
            {
                SpawnItems();
            }
        }
    }
}
