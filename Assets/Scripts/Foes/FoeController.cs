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
    
    [SerializeField] private CircleCollider2D hitBox;

    [SerializeField] public GameObject wanderingPointPrefab;

    [SerializeField] private LayerMask rayLayerMask;
    
    [Header("Morals parameters")]
    [SerializeField] private int maxMorals;
    [SerializeField] private int individualMoralsDamageFromHit;
    [SerializeField] private int surroundingMoralsDamageFromHit;
    [SerializeField] private int surroundingmoralsDamageFromDeath;
    [SerializeField] private float reinforcementRange;
    
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

    private enum Type
    {
        CQB,
        RANGED
    }
    
    public enum State
    {
        IDLE,
        PATROL,
        WANDERING,
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
    public List<Vector3> patrolPath = null;
    public GameObject wanderingPoint;
    public Vector3 fleePoint = Vector3.zero;
    private int nextPatrolPointID = 0;
    private int life;
    private GameManager gameManager;
    private Animator animator;
    private BoxCollider2D collider;
    private Coroutine attackCoroutine = null;
    public Transform target;
    private float attackStartAt;
    private float lastPathGetAt = 0;

    private PathFindingManager pathFindingManager;
    private PatrolPathGenerator patrolPathGenerator;
    private FoesManager foesManager;
    
    private int itemToSpawn;
    private float hittedAt;
    public bool isActive = false;
    public int morals;
    public Vector3 avoidanceVector;

    private AvoidanceBehavior avoidanceBehavior;
    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        pathFindingManager = FindObjectOfType<PathFindingManager>();
        playerTransform = gameManager.GetPlayerTranform();
        patrolPathGenerator = FindObjectOfType<PatrolPathGenerator>();
        foesManager = FindObjectOfType<FoesManager>();
        avoidanceBehavior = GetComponentInChildren<AvoidanceBehavior>();
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
        
        // Spawn a wandering point
        wanderingPoint = Instantiate(wanderingPointPrefab, Vector3.zero, Quaternion.identity);
        
        SetLootAmount();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameRunning && isActive)
        {
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
        if (gameManager.gameRunning && isActive)
        {
            switch (state)
            {
                case State.IDLE:
                    break;
                case State.PATROL:
                    UpdateState();
                    avoidanceVector = avoidanceBehavior.GetAvoidanceVector();
                    moveToNextPatrolPoint();
                    break;
                case State.WANDERING:
                    UpdateState();
                    avoidanceVector = avoidanceBehavior.GetAvoidanceVector();
                    Wandering();
                    break;
                case State.POSITIONING:
                    GoAtAttackRange();
                    avoidanceVector = avoidanceBehavior.GetAvoidanceVector();
                    break;
                case State.FLEE:
                    GoAtReinforcementRange();
                    break;
                case State.HITTED:
                    // TODO: Apply push while hitted
                    break;
                case State.DEAD:
                    break;
            }
        }
    }

    public void ResetFoe()
    {
        state = State.IDLE;
        animator.SetBool("isDead", false);
        life = maxLife;
        morals = maxMorals;

        // Check if inside a building
        if (patrolPathGenerator.isInOpenBuilding(transform.position))
        {
            return;
        }
        
        // Try to get a patrol path
        patrolPath = patrolPathGenerator.GeneratePatrolPath(transform.position);

        // If no patrol path has been return, prepare to wander around the map
        if (patrolPath == null)
        {
            wanderingPoint.transform.position = patrolPathGenerator.GetRandomWalkableNode();
            target = wanderingPoint.transform;
            pathFindingManager.RegisterToQueue(this);
        }
        else
            wanderingPoint.transform.position = Vector3.zero;
    }
    
    private void UpdateState()
    {
        // Check if at detection range
        if ((playerTransform.position - transform.position).magnitude <= detectionRange)
        {
            RaycastHit2D ray = Physics2D.Raycast(transform.position, playerTransform.position - transform.position, Mathf.Infinity, rayLayerMask);

            // Check if the foe can see the player
            if (ray.collider.tag == "PlayerFeet" || ray.collider.tag == "Player" || ray.collider.tag == "PlayerAttack")
            {
                Debug.Log("Want to attack");
                rigid.velocity = Vector2.zero;
                foesManager.RegisterToFightingList(this);
                target = playerTransform;
                state = State.POSITIONING;
                return;
            }
        }
        
        // Check if a patrol path has been assigned
        if (state != State.PATROL && patrolPath != null && patrolPath.Count > 0)
        {
            nextPatrolPointID = 0;
            
            // Get the nearest patrol point
            for (int i = 0; i < patrolPath.Count; i++)
            {
                if ((patrolPath[i] - transform.position).magnitude <
                    (patrolPath[nextPatrolPointID] - transform.position).magnitude)
                    nextPatrolPointID = i;
            }

            state = State.PATROL;
            return;
        }

        if (state != State.WANDERING && wanderingPoint.transform.position != Vector3.zero)
            state = State.WANDERING;
    }

    private void GoAtAttackRange()
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
        
        RaycastHit2D ray = Physics2D.Raycast(transform.position, playerTransform.position - transform.position, Mathf.Infinity, rayLayerMask);
        
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

    private void GoAtReinforcementRange()
    {
        // Check if the target is at range
        if ((target.position - transform.position).magnitude <= reinforcementRange)
        {
            // Get all the foes at range   
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, reinforcementRange);

            FoeController foe;
            
            foreach (Collider2D col in colliders)
            {
                // Check if it is a foe
                if (col.tag == "Foe" || col.tag == "FoeAttack")
                {
                    foe = col.GetComponentInParent<FoeController>();
                    foe.state = State.POSITIONING;
                    foe.target = playerTransform;
                    foesManager.RegisterToFightingList(foe);
                }
            }

            morals = maxMorals;
            target = playerTransform;
            state = State.POSITIONING;
            rigid.velocity = Vector2.zero;
            path = null;
            return;
        }
        
        RaycastHit2D ray = Physics2D.Raycast(transform.position + (target.position - transform.position).normalized * hitBox.radius, target.position - transform.position);
        
        // Check if the foe has a direct line of view to the target
        if (ray.collider.tag == "Foe")
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
        // Set speed to rush strait to the target
        Vector3 moveVector = target.position - transform.position;

        rigid.velocity = (moveVector + avoidanceVector).normalized * moveSpeed * Time.deltaTime;
        
        renderer.flipX = rigid.velocity.x < 0;
        
        animator.SetBool("isMoving", true);
    }

    private void moveToNextPatrolPoint()
    {
        Vector3 moveVector = patrolPath[nextPatrolPointID] - transform.position;
        
        rigid.velocity = (moveVector + avoidanceVector).normalized * moveSpeed * Time.deltaTime;
        
        renderer.flipX = rigid.velocity.x < 0;
        
        // Check if the node has been reach
        if (patrolPath[nextPatrolPointID].x > transform.position.x - (gameManager.parameters.cellSize.x / 2) &&
            patrolPath[nextPatrolPointID].x < transform.position.x + (gameManager.parameters.cellSize.x / 2) &&
            patrolPath[nextPatrolPointID].y > transform.position.y - (gameManager.parameters.cellSize.y / 2) &&
            patrolPath[nextPatrolPointID].y < transform.position.y + (gameManager.parameters.cellSize.y / 2))  
        {
            // Check if it was the last node
            if (nextPatrolPointID == patrolPath.Count - 1)
                nextPatrolPointID = 0;
            else
                nextPatrolPointID++;
        }
        
        animator.SetBool("isMoving", true);
    }

    private void moveFollowingPath()
    {
        if (path == null || path.Count <= 0)
            return;
        
        Vector3 moveVector = path[path.Count - 1] - transform.position;
        
        rigid.velocity = (moveVector + avoidanceVector).normalized * moveSpeed * Time.deltaTime;
        
        renderer.flipX = rigid.velocity.x < 0;
        
        // Check if the node has been reach
        if (path[path.Count - 1].x > transform.position.x - (gameManager.parameters.cellSize.x) &&
            path[path.Count - 1].x < transform.position.x + (gameManager.parameters.cellSize.x) &&
            path[path.Count - 1].y > transform.position.y - (gameManager.parameters.cellSize.y) &&
            path[path.Count - 1].y < transform.position.y + (gameManager.parameters.cellSize.y))
        {
            //rigid.velocity = Vector2.zero;
            path.RemoveAt(path.Count - 1);
        }
    }

    private void Wandering()
    {
        if (path == null || path.Count <= 0)
        {
            // Check if the wondering point has been reached
            if(wanderingPoint.transform.position.x > transform.position.x - (gameManager.parameters.cellSize.x) &&
               wanderingPoint.transform.position.x < transform.position.x + (gameManager.parameters.cellSize.x) &&
               wanderingPoint.transform.position.y > transform.position.y - (gameManager.parameters.cellSize.y) &&
               wanderingPoint.transform.position.y < transform.position.y + (gameManager.parameters.cellSize.y))
                    wanderingPoint.transform.position = patrolPathGenerator.GetRandomWalkableNode();
            
            // Ask for a path
            pathFindingManager.RegisterToQueue(this);
            
            moveToTarget();
        }
        else
        {
            moveFollowingPath();   
        }
    }
    
    private void Die()
    {
        if(attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        
        foesManager.ReduceFightingFoesMoral(this, surroundingmoralsDamageFromDeath);
        foesManager.UnregisterToFightingList(this);
        
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
        if (state != State.DEAD)
        {
            if (other.tag == "PlayerAttack")
            {
                life -= gameManager.playerDamage;

                if (life <= 0)
                {
                    Die();
                    return;
                }

                morals -= individualMoralsDamageFromHit;
                foesManager.ReduceFightingFoesMoral(this, surroundingMoralsDamageFromHit);
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
        if (false)
        {
            Gizmos.DrawCube(fleePoint, new Vector3(0.5f, 0.5f));
            Gizmos.DrawLine(transform.position, fleePoint);
        }
        // Draw path gizmos
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

        // Draw patrol path gizmos
        if (false && patrolPath != null && patrolPath.Count == 4)
        {
            for(int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawCube(patrolPath[i], new Vector3(0.1f, 0.1f));
                        Gizmos.DrawLine(new Vector3(patrolPath[i].x, patrolPath[i].y), new Vector3(patrolPath[i + 1].x, patrolPath[i + 1].y));
                        break;
                    case 1:
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(patrolPath[i], new Vector3(0.1f, 0.1f));
                        Gizmos.DrawLine(new Vector3(patrolPath[i].x, patrolPath[i].y), new Vector3(patrolPath[i + 1].x, patrolPath[i + 1].y));
                        break;
                    case 2:
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawCube(patrolPath[i], new Vector3(0.1f, 0.1f));
                        Gizmos.DrawLine(new Vector3(patrolPath[i].x, patrolPath[i].y), new Vector3(patrolPath[i + 1].x, patrolPath[i + 1].y));
                        break;
                    case 3:
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(patrolPath[i], new Vector3(0.1f, 0.1f));
                        Gizmos.DrawLine(new Vector3(patrolPath[i].x, patrolPath[i].y), new Vector3(patrolPath[0].x, patrolPath[0].y));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
