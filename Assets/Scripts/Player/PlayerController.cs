using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public class PlayerController : MonoBehaviour
{
    enum Orientation
    {
        UP,
        DOWN,
        HORIZONTAL
    }

    [Header("Movement attributs")]
    [SerializeField] float moveSpeed;
    [SerializeField] float moveRecoil;

    [Header("Dash attributs")] [SerializeField]
    float dashSpeed;

    [SerializeField] float dashDuration;
    [SerializeField] float dashCoolDown;
    [SerializeField] private GameObject SmokeDash;
    [SerializeField] private Vector3 offSetSmokeDash;

    [Header("Attack attributs")] [SerializeField]
    float attackRange;

    [SerializeField] public int attackDamage;
    [SerializeField] float attackCoolDown;
    [SerializeField] BoxCollider2D attackRightCollider;
    [SerializeField] BoxCollider2D attackLeftCollider;
    [SerializeField] BoxCollider2D attackUpCollider;
    [SerializeField] BoxCollider2D attackDownCollider;

    [Header("Slash attributs")]
    [SerializeField] public Vector3 offsetPostion;
    [SerializeField] public GameObject slashRight;
    [SerializeField] public GameObject slashLeft;
    [SerializeField] public GameObject slashUp;
    [SerializeField] public GameObject slashDown;
    [SerializeField] private float timeToDestroySlash;

    [Header("Recovery attributs")]
    [SerializeField] private float timeToRecovery;
    private float recoveryAt;
    [SerializeField] private float timeToBlink;
    private float counterTime;
    private bool isRecovering = false;


    float movementUp;
    float movementDown;
    float movementRight;
    float movementLeft;
    public bool right;

    private SoundPlayerManager sound;

    Orientation actualOrientation;

    float dashStartAt = 0;
    float lastDashAt = 0;

    float lastAttackAt = 0;

    private GameManager gameManager;

    Animator animator;
    SpriteRenderer renderer;
    private Rigidbody2D rigid;

    private enum PlayerState
    {
        IDLE,
        WALKING,
        ATTACKING,
        DASHING,
        HITTED,
        DEAD
    }

    [SerializeField] private PlayerState state = PlayerState.IDLE;

    private float lastHitAt;
    private PlayerLife life;
    private PlayerInventory inventory;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        life = GetComponent<PlayerLife>();
        inventory = GetComponent<PlayerInventory>();
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        actualOrientation = Orientation.HORIZONTAL;
        sound = GetComponent<SoundPlayerManager>();

        Camera.main.GetComponent<CameraManager>().AddPlayer(transform);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the game is running
        if (!gameManager.gameRunning || state == PlayerState.DEAD)
            return;

        //Check if the player
        if (state == PlayerState.HITTED)
        {
            return;
        }

        if (isRecovering)
        {
            counterTime += Time.deltaTime;
            if (counterTime > timeToBlink)
            {
                counterTime = 0.0f;
                GetComponent<SpriteRenderer>().enabled = !GetComponent<SpriteRenderer>().enabled;
            }

            if(Time.time >= recoveryAt)
            {
                isRecovering = false;
                counterTime = 0.0f;
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        // Check if the user launched an attack
        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetAxis("Fire1") > 0) && state != PlayerState.DASHING &&
            Time.time > lastAttackAt + attackCoolDown)
            StartAttack();

        // Check if the user launched a dash
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxis("Jump") > 0) && state != PlayerState.DASHING &&
            Time.time > lastDashAt + dashCoolDown)
            StartDash();

        switch (state)
        {
            case PlayerState.WALKING:
                Move();
                break;
            case PlayerState.DASHING:
                if (Time.time > dashStartAt + dashDuration)
                    EndDash();
                else
                {
                    Dash();
                    return;
                }

                break;
            case PlayerState.ATTACKING:
                break;
            case PlayerState.DEAD:
                break;
            case PlayerState.IDLE:
                if (state != PlayerState.ATTACKING)
                {
                    Move();
                }

                break;
        }

        if (state == PlayerState.IDLE)
        {
            switch (actualOrientation)
            {
                case Orientation.UP:
                    animator.SetBool("isIdleUp", true);
                    animator.SetBool("isIdleDown", false);
                    animator.SetBool("isIdle", false);
                    break;
                case Orientation.DOWN:
                    animator.SetBool("isIdleUp", false);
                    animator.SetBool("isIdleDown", true);
                    animator.SetBool("isIdle", false);
                    break;
                case Orientation.HORIZONTAL:
                    animator.SetBool("isIdleUp", false);
                    animator.SetBool("isIdleDown", false);
                    animator.SetBool("isIdle", true);
                    break;
            }
        }
    }

    private void Move()
    {

        if (Input.GetKey(KeyCode.W) || Input.GetAxisRaw("Vertical") > 0)
            movementUp = 1;
        else
            movementUp = 0;

        if (Input.GetKey(KeyCode.S) || Input.GetAxisRaw("Vertical") < 0)
            movementDown = 1;
        else
            movementDown = 0;

        if (Input.GetKey(KeyCode.D) || Input.GetAxisRaw("Horizontal") > 0)
        {
            movementRight = 1;
            right = true;
        }
        else
            movementRight = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetAxisRaw("Horizontal") < 0)
        {
            movementLeft = 1;
            right = false;
        }
        else
            movementLeft = 0;

        if (movementRight + movementLeft + movementUp + movementDown > 1)
        {
            movementRight *= Mathf.Sqrt(0.5f);
            movementLeft *= Mathf.Sqrt(0.5f);
            movementDown *= Mathf.Sqrt(0.5f);
            movementUp *= Mathf.Sqrt(0.5f);
        }

        Vector3 moveVector = new Vector3(movementRight - movementLeft, movementUp - movementDown);
        moveVector *= moveSpeed * Time.deltaTime;
        
        if (moveVector == Vector3.zero)
        {
            state = PlayerState.IDLE;
            animator.SetBool("isWalking", false);
            animator.SetBool("isWalkingUp", false);
            animator.SetBool("isWalkingDown", false);
        }
        else
        {
            state = PlayerState.WALKING;
            animator.SetBool("isIdleUp", false);
            animator.SetBool("isIdleDown", false);
            animator.SetBool("isIdle", false);
        }

        rigid.velocity = moveVector;

        // Animator managing state
        if (movementRight > 0 || movementLeft > 0)
        {
            if (movementRight > 0)
                renderer.flipX = false;
            else
                renderer.flipX = true;

            animator.SetBool("isWalkingUp", false);
            animator.SetBool("isWalkingDown", false);
            animator.SetBool("isWalking", true);

            actualOrientation = Orientation.HORIZONTAL;
        }
        else if (movementUp > 0)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isWalkingDown", false);
            animator.SetBool("isWalkingUp", true);

            actualOrientation = Orientation.UP;
        }
        else if (movementDown > 0)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isWalkingUp", false);
            animator.SetBool("isWalkingDown", true);

            actualOrientation = Orientation.DOWN;
        }
    }

    void StartDash()
    {
        state = PlayerState.DASHING;

        animator.SetBool("isWalking", false);
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);

        switch (actualOrientation)
        {
            case Orientation.DOWN:
                animator.SetBool("isDashingDown", true);
                break;
            case Orientation.UP:
                animator.SetBool("isDashingUp", true);
                break;
            case Orientation.HORIZONTAL:
                animator.SetBool("isDashing", true);
                break;
        }

        dashStartAt = Time.time;
    }

    void Dash()
    {
        Vector3 dashVector = new Vector3(movementRight - movementLeft, movementUp - movementDown);

        dashVector *= dashSpeed * Time.deltaTime;
        
        rigid.velocity = dashVector;
    }

    void EndDash()
    {
        animator.SetBool("isDashing", false);
        animator.SetBool("isDashingUp", false);
        animator.SetBool("isDashingDown", false);

        lastDashAt = Time.time;
        state = PlayerState.IDLE;
    }

    void StartAttack()
    {
        state = PlayerState.ATTACKING;

        animator.SetBool("isWalking", false);
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);

        switch (actualOrientation)
        {
            case Orientation.UP:
                attackUpCollider.gameObject.SetActive(true);
                animator.SetBool("isAttackingUp", true);
                break;
            case Orientation.DOWN:
                attackDownCollider.gameObject.SetActive(true);
                animator.SetBool("isAttackingDown", true);
                break;
            case Orientation.HORIZONTAL:
                if (renderer.flipX)
                    attackLeftCollider.gameObject.SetActive(true);
                else
                    attackRightCollider.gameObject.SetActive(true);
                animator.SetBool("isAttacking", true);
                break;
        }
    }

    void EndAttack()
    {
        attackUpCollider.gameObject.SetActive(false);
        attackDownCollider.gameObject.SetActive(false);
        attackRightCollider.gameObject.SetActive(false);
        attackLeftCollider.gameObject.SetActive(false);

        animator.SetBool("isAttacking", false);
        animator.SetBool("isAttackingUp", false);
        animator.SetBool("isAttackingDown", false);

        state = PlayerState.IDLE;
        lastAttackAt = Time.time;
    }

    public void IncreaseDamage(int amount)
    {
        attackDamage += amount;
    }

    public int GetForceAttack()
    {
        return attackDamage;
    }

    public void ResetPlayer()
    {
        inventory.ResetInventory();
        life.ResetLife();
        
        animator.SetBool("isDead", false);
        rigid.isKinematic = false;
        state = PlayerState.IDLE;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isRecovering && state != PlayerState.HITTED && other.tag == "FoeAttack")
        {
            lastHitAt = Time.time;
            state = PlayerState.HITTED;

            animator.SetBool("isIdle", false);
            animator.SetBool("isIdleUp", false);
            animator.SetBool("isIdleDown", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isWalkingUp", false);
            animator.SetBool("isWalkingDown", false);
            animator.SetBool("isDashing", false);
            animator.SetBool("isDashingDown", false);
            animator.SetBool("isDashingUp", false);
            animator.SetBool("isHurting", true);

            if (life.ApplyDamage(other.GetComponentInParent<FoeController>().attackDamage))
            {
                rigid.isKinematic = true;
                animator.SetBool("isDead", true);
                rigid.velocity = Vector2.zero;
                state = PlayerState.DEAD;
                gameManager.gameState = GameManager.GameState.DEATH;
            }
        }
    }

    //Recoil after attack
    void Recoil()
    {
        rigid.velocity = Vector2.zero;
        Vector3 recoilVector = new Vector3(movementRight - movementLeft, movementUp - movementDown);
        recoilVector *= -moveRecoil * Time.deltaTime;
        rigid.velocity = recoilVector;
    }

    //Function for instantiate a slash object for attack
    void Slash()
    {
        Vector3 positionSpawn = Vector3.zero;
        Vector3 Dir = Vector3.zero;
        GameObject slash;
       
        //Check dîrection for instantiate
        switch (actualOrientation)
        {
            case Orientation.UP:
                //pos and dir
                Dir = Vector3.up/2;
                positionSpawn = new Vector3(0.0f, +offsetPostion.y, 0.0f);
                slash = Instantiate(slashUp, (transform.position + positionSpawn), Quaternion.identity);
                slash.GetComponent<Rigidbody2D>().velocity = Dir;
                Destroy(slash, timeToDestroySlash);
                break;

            case Orientation.DOWN:
                Dir = Vector3.down/2;
                positionSpawn = new Vector3(0.0f, -offsetPostion.y, 0.0f);
                slash = Instantiate(slashDown, (transform.position + positionSpawn), Quaternion.identity);
                slash.GetComponent<Rigidbody2D>().velocity = Dir;
                Destroy(slash, timeToDestroySlash);
                break;

            case Orientation.HORIZONTAL:
                if (right)
                {
                    Dir = Vector3.right/2;
                    positionSpawn = new Vector3(offsetPostion.x, 0.0f ,0.0f );
                    slash = Instantiate(slashRight, (transform.position + positionSpawn), Quaternion.identity);
                    slash.GetComponent<Rigidbody2D>().velocity = Dir;


                    Destroy(slash, timeToDestroySlash);
                }
                else
                {
                    Dir = Vector3.left/2;
                    positionSpawn = new Vector3(-offsetPostion.x, 0.0f, 0.0f);
                    slash = Instantiate(slashLeft, (transform.position + positionSpawn), Quaternion.identity);
                    slash.GetComponent<Rigidbody2D>().velocity = Dir;
                    Destroy(slash, timeToDestroySlash);
                }
                break;
        }


    }

    //Instantiate prefab smoke
    public void InstanceSmoke()
    {
        Destroy(Instantiate(SmokeDash, transform.position + offSetSmokeDash, Quaternion.identity), 0.5f);
    }

    public void EndHurt()
    {
        animator.SetBool("isHurting", false);
        state = PlayerState.IDLE;
        isRecovering = true;
        recoveryAt = Time.time + timeToRecovery;
    }
}
