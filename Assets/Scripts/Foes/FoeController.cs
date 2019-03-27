using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FoeController : MonoBehaviour
{
    [SerializeField] private int maxLife;
    
    [SerializeField] private float detectionRange;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackSpeed;
    
    [SerializeField] private float attackDuration;
    [SerializeField] private int attackDamage;
    
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
        DEAD
    }

    private float attackRange;
    public State state = State.IDLE;
    private Rigidbody2D rigid;
    private SpriteRenderer renderer;
    public Transform playerTransform;
    private List<Vector3> path = null;
    private int nextPathPointID = 0;
    private int life;
    private GameManager gameManager;
    private Animator animator;
    private BoxCollider2D collider;
    private Coroutine attackCoroutine = null;
    
    private float attackStartAt;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
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
        attackRange = attackSpeed * attackDuration * Time.deltaTime;

        life = maxLife;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameState == GameManager.GameState.INGAME)
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
                    GoAtRange();
                    break;
                case State.ATTACK:
                    break;
                case State.FLEE:
                    break;
                case State.DEAD:
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (gameManager.gameState == GameManager.GameState.INGAME)
        {
            switch (state)
            {
                case State.IDLE:
                    break;
                case State.PATROL:
                    break;
                case State.POSITIONING:
                    moveFollowingPath();
                    break;
                case State.FLEE:
                    break;
                case State.DEAD:
                    break;
            }
        }
    }

    private void UpdateState()
    {
        if ((playerTransform.position - transform.position).magnitude <= detectionRange)
        {
            state = State.POSITIONING;
        }
    }

    private void GoAtRange()
    {
        // Check if at range
        if ((playerTransform.position - transform.position).magnitude <= attackRange)
        {
            state = State.ATTACK;
            rigid.velocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            animator.SetBool("isPreparingAttack", true);
            return;
        }
        
        // Get path to the player
        path = gameManager.GetPathTo(transform.position, playerTransform.position);
        
        // Follow the path
        moveFollowingPath();
        
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
        
        while (Time.time < attackStartAt + attackDuration)
        {
            rigid.velocity = attackVector.normalized * attackSpeed * Time.deltaTime;
            
            yield return new WaitForFixedUpdate();
        }

        animator.SetBool("isAttacking", false);
        
        attackCoroutine = null;
        
        state = State.IDLE;
    }
    
    private void moveToTarget()
    {
        // Set speed to rush strait to the player
        rigid.velocity = playerTransform.position - transform.position;
        rigid.velocity = rigid.velocity.normalized * moveSpeed;
    }

    private void moveToNextPatrolPoint()
    {
        // Set speed to rush strait to the next patrol point
        //rigid.velocity = nextPatrolPointID - transform.position;
        rigid.velocity = rigid.velocity.normalized * moveSpeed;
    }

    private void moveFollowingPath()
    {
        if (path == null)
            return;
        
        Vector3 moveVector = path[path.Count - 1] - transform.position;
        
        rigid.velocity = moveVector.normalized * moveSpeed * Time.deltaTime;
        
        renderer.flipX = rigid.velocity.x < 0;
    }

    private void Die()
    {
        if(attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        
        animator.SetBool("isMoving", false);
        animator.SetBool("isPreparingAttack", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", true);

        rigid.isKinematic = true;
        collider.isTrigger = true;
        rigid.velocity = Vector2.zero;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.DEAD)
        {
            if (other.tag == "PlayerAttack")
            {
                life -= playerTransform.GetComponent<PlayerController>().attackDamage;

                if (life <= 0)
                    Die();

            }
        }
    }

    private void OnDrawGizmos()
    {
        if (false && path != null)
        {
            // Draw the path
            for (int i = 0; i < path.Count; i++)
            {
                if (i == path.Count - 1)
                    break;
                
                Gizmos.color = Color.red;
                Gizmos.DrawCube(path[i], new Vector3(0.1f, 0.1f));
                Gizmos.DrawLine(new Vector3(path[i].x, path[i].y), new Vector3(path[i + 1].x, path[i + 1].y));
            }
            
            // Draw the movement vector
            /*Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(rigid.velocity.x, rigid.velocity.y));*/
        }
    }
}
