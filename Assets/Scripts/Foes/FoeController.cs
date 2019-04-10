using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class FoeController : MonoBehaviour
{
    [SerializeField] private int maxLife;
    
    [SerializeField] private float detectionRange;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackSpeed;
    
    [SerializeField] private float attackDuration;
    [SerializeField] public int attackDamage;

    [SerializeField] private float hittedDuration;
    
    [SerializeField] public float getPathCD;
    
    [SerializeField] private CircleCollider2D hitBox;
    
    [Header("Food prefab reference")]
    [SerializeField] private GameObject foodSmallPrefab;
    [SerializeField] private GameObject foodMediumPrefab;
    [SerializeField] private GameObject foodBigPrefab;
    
    [Header("Item spawn parameters")]
    [SerializeField] private int minFoodSpawn;
    [SerializeField] private int maxFoodSpawn;
    [Range(0f, 1f)] [SerializeField] private float lootSpawnChance;
    [Range(0f, 1f)] [SerializeField] private float foodSmallSpawnChance;
    [Range(0f, 1f)] [SerializeField] private float foodMediumSpawnChance;
    [Range(0f, 1f)] [SerializeField] private float foodBigSpawnChance;

    private bool isInvicible = false;
    [SerializeField] private float timeToRecovery = 0.4f;
    private float LastHit;
    
    private enum Type
    {
        CQB,
        RANGED
    }
    
    public enum State
    {
        IDLE,
        PATROL,
        POSITIONING,
        ATTACK,
        FLEE,
        HITTED,
        DEAD
    }

    private float attackRange;
    public State state = State.IDLE;
    private Rigidbody2D rigid;
    private SpriteRenderer renderer;
    public Transform playerTransform;
    public List<Vector3> path = null;
    private int nextPathPointID = 0;
    private int life;
    private GameManager gameManager;
    private Animator animator;
    private BoxCollider2D collider;
    private Coroutine attackCoroutine = null;
    public Vector3 target;
    private float attackStartAt;
    private float lastPathGetAt = 0;

    private PathFindingManager pathFindingManager;

    private int itemToSpawn;
    private float hittedAt;
    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        pathFindingManager = FindObjectOfType<PathFindingManager>();
        playerTransform = gameManager.GetPlayerTranform();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        
        // Define the attack range
        attackRange = attackSpeed * attackDuration * Time.fixedDeltaTime;

        life = maxLife;
        
        SetLootAmount();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameState == GameManager.GameState.INGAMEDAY || gameManager.gameState == GameManager.GameState.INGAMENIGHT)
        {
            if (isInvicible)
            {
                if (Time.time > LastHit + timeToRecovery) ;
                {
                    isInvicible = false;
                }
            }

            switch (state)
            {
                case State.IDLE:
                    UpdateState();
                    break;
                case State.PATROL:
                    UpdateState();
                    break;
                case State.POSITIONING:
                    break;
                case State.ATTACK:
                    break;
                case State.FLEE:
                    break;
                case State.HITTED:
                    if (Time.time > hittedAt + hittedDuration)
                        state = State.IDLE;
                    break;
                case State.DEAD:
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (gameManager.gameState == GameManager.GameState.INGAMEDAY || gameManager.gameState == GameManager.GameState.INGAMENIGHT)
        {
            switch (state)
            {
                case State.IDLE:
                    break;
                case State.PATROL:
                    break;
                case State.POSITIONING:
                    target = playerTransform.position;
                    GoAtRange();
                    break;
                case State.FLEE:
                    break;
                case State.HITTED:
                    // TODO: Apply push while hitted
                    break;
                case State.DEAD:
                    break;
            }
        }
    }

    private void UpdateState()
    {
        // Check if at detection range
        if ((playerTransform.position - transform.position).magnitude <= detectionRange)
        {
            RaycastHit2D ray = Physics2D.Raycast(transform.position, playerTransform.position - transform.position);
        
            // Check if the foe can see the player
            if (ray.collider.tag == "PlayerFeet" || ray.collider.tag == "Player" || ray.collider.tag == "PlayerAttack")
                state = State.POSITIONING;
        }
    }

    private void GoAtRange()
    {
        // Check if the player is at range
        if ((playerTransform.position - transform.position).magnitude <= attackRange)
        {
            // Launch the attack
            state = State.ATTACK;
            rigid.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            animator.SetBool("isPreparingAttack", true);
            path = null;
            return;
        }
        
        RaycastHit2D ray = Physics2D.Raycast(transform.position, playerTransform.position - transform.position);
        
        // Check if the foe has a direct line of view to the player
        if (ray.collider.tag == "PlayerFeet" || ray.collider.tag == "Player" || ray.collider.tag == "PlayerAttack")
        {
            path = null;
            moveToTarget();
        } 
        else if (path != null && path.Count > 0)
        {
            // Follow the path
            moveFollowingPath();
        }
        else
        {
            path = null;
            
            // Get path to the player
            pathFindingManager.RegisterToQueue(this);
            
            // Go straight to the player
            moveToTarget();
        }
        
        animator.SetBool("isMoving", true);
    }

    private void LaunchAttack()
    {
        animator.SetBool("isPreparingAttack", false);
        animator.SetBool("isAttacking", true);
        attackCoroutine = StartCoroutine(Attack());
    }
    
    IEnumerator Attack()
    {
        attackStartAt = Time.time;   
        Vector3 attackVector = playerTransform.position - transform.position;

        renderer.flipX = attackVector.x < 0;

        hitBox.tag = "FoeAttack";
        
        while (Time.time < attackStartAt + attackDuration)
        {
            rigid.velocity = attackVector.normalized * attackSpeed * Time.deltaTime;
            
            yield return new WaitForFixedUpdate();
        }

        animator.SetBool("isAttacking", false);
        
        hitBox.tag = "Foe";
        
        attackCoroutine = null;
        
        state = State.IDLE;
    }
    
    private void moveToTarget()
    {
        // Set speed to rush strait to the player
        Vector3 moveVector = target - transform.position;
        
        rigid.velocity = moveVector.normalized * moveSpeed * Time.deltaTime;
        renderer.flipX = rigid.velocity.x < 0;
    }

    private void moveToNextPatrolPoint()
    {
        // Set speed to rush strait to the next patrol point
        //rigid.velocity = nextPatrolPointID - transform.position;
        rigid.velocity = rigid.velocity.normalized * moveSpeed;
    }

    private void moveFollowingPath()
    {
        if (path == null || path.Count <= 0)
            return;
        
        Vector3 moveVector = path[path.Count - 1] - transform.position;
        
        rigid.velocity = moveVector.normalized * moveSpeed * Time.deltaTime;
        
        renderer.flipX = rigid.velocity.x < 0;
        
        // Check if the node has been reach
        if(path[path.Count - 1].x > transform.position.x - (gameManager.parameters.cellSize.x / 4) &&
           path[path.Count - 1].x < transform.position.x + (gameManager.parameters.cellSize.x / 4) &&
           path[path.Count - 1].y > transform.position.y - (gameManager.parameters.cellSize.y / 4) &&
           path[path.Count - 1].y < transform.position.y + (gameManager.parameters.cellSize.y / 4))
            path.RemoveAt(path.Count - 1);
    }

    private void Die()
    {
        if(attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        
        hitBox.tag = "Foe";
        animator.SetBool("isMoving", false);
        animator.SetBool("isPreparingAttack", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", true);

        rigid.isKinematic = true;
        collider.isTrigger = true;
        rigid.velocity = Vector2.zero;
        state = State.DEAD;
        SpawnItems();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.DEAD && !isInvicible)
        {
            if (other.tag == "PlayerAttack")
            {
                isInvicible = true;
                LastHit = Time.time;
                animator.SetTrigger("isHurting");
                life -= playerTransform.GetComponent<PlayerController>().attackDamage;

                if (life <= 0)
                {
                    Die();
                    return;
                }

                hittedAt = Time.time;
                state = State.HITTED;
            }
        }
    }

    void SpawnItems()
    {
        for (int i = 0; i < itemToSpawn; i++)
        {
            float rnd = Random.Range(0f, 1f);

            if (rnd <= foodSmallSpawnChance)
                Instantiate(foodSmallPrefab, transform.position, Quaternion.identity);
            else if (rnd <= foodMediumSpawnChance)
                Instantiate(foodMediumPrefab, transform.position, Quaternion.identity);
            else if (rnd <= foodBigSpawnChance)
                Instantiate(foodBigPrefab, transform.position, Quaternion.identity);
        }
    }
    
    public void SetLootAmount()
    {
        // Define wheter the container will instantiate scrap when destroy or not
        float rnd = Random.Range(0f, 1f);

        if (rnd <= lootSpawnChance)
        {
            itemToSpawn = Random.Range(minFoodSpawn, maxFoodSpawn);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (false && path != null)
        {
            // Draw the path
            for (int i = 0; i < path.Count; i++)
            {
                /*if (i == path.Count - 1)
                    break;*/
                
                if(i == path.Count - 1)
                    Gizmos.color = Color.cyan;
                else
                    Gizmos.color = Color.red;
                
                Gizmos.DrawCube(path[i], new Vector3(0.1f, 0.1f));
                Gizmos.DrawLine(new Vector3(path[i].x, path[i].y), new Vector3(path[i + 1].x, path[i + 1].y));
            }
            
            /*Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(rigid.velocity.x, rigid.velocity.y));
            // Draw the movement vector
            /*Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(rigid.velocity.x, rigid.velocity.y));*/
        }
    }
}
