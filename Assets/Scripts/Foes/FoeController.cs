using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FoeController : MonoBehaviour
{
    [SerializeField] private float detectionRange;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float attackRange;
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
        ATTACK,
        FLEE
    }

    public State state = State.IDLE;
    private Rigidbody2D rigid;
    public Transform playerTransform;
    private List<Vector3> path = null;
    private int nextPathPointID = 0;
    
    private GameManager gameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        rigid = GetComponent<Rigidbody2D>();
        playerTransform = gameManager.GetPlayerTranform();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameState == GameManager.GameState.INGAME)
        {
            switch (state)
            {
                case State.IDLE:
                    break;
                case State.PATROL:
                    break;
                case State.ATTACK:
                    path = gameManager.GetPathTo(transform.position, playerTransform.position);
                    break;
                case State.FLEE:
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.IDLE:
                break;
            case State.PATROL:
                break;
            case State.ATTACK:
                moveFollowingPath();
                break;
            case State.FLEE:
                break;
        }
    }

    private void moveToTarget()
    {
        // Set speed to rush strait to the player
        rigid.velocity = playerTransform.position - transform.position;
        rigid.velocity = rigid.velocity.normalized * maxSpeed;
    }

    private void moveToNextPatrolPoint()
    {
        // Set speed to rush strait to the next patrol point
        //rigid.velocity = nextPatrolPointID - transform.position;
        rigid.velocity = rigid.velocity.normalized * maxSpeed;
    }

    private void moveFollowingPath()
    {
        if (path == null)
            return;
        
        Vector3 moveVector = path[path.Count - 1] - transform.position;
        
        rigid.velocity = moveVector.normalized * maxSpeed * Time.deltaTime;
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
