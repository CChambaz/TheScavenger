using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    [Header("Dash attributs")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCoolDown;

    [Header("Attack attributs")]
    [SerializeField] float attackRange;
    [SerializeField] public int attackDamage;
    [SerializeField] float attackCoolDown;
    [SerializeField] BoxCollider2D attackRightCollider;
    [SerializeField] BoxCollider2D attackLeftCollider;
    [SerializeField] BoxCollider2D attackUpCollider;
    [SerializeField] BoxCollider2D attackDownCollider;

    [SerializeField] private float invicibilityTimeAfterHit;
    
    int movementUp;
    int movementDown;
    int movementRight;
    int movementLeft;

    private SoundPlayerManager sound;

    Orientation actualOrientation;

    float dashStartAt = 0;
    float lastDashAt = 0;

    float lastAttackAt = 0;

    PlayerRage rage;
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
        DEAD
    }

    private PlayerState state = PlayerState.IDLE;
    
    private float lastHitAt;
    private PlayerLife life;
    
    private void Start()
    {
        rage = GetComponent<PlayerRage>();
        gameManager = FindObjectOfType<GameManager>();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        actualOrientation = Orientation.HORIZONTAL;
        sound = GetComponent<SoundPlayerManager>();
        life = GetComponent<PlayerLife>();

        Camera.main.GetComponent<CameraManager>().AddPlayer(transform);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the game is running
        if (gameManager.gameState != GameManager.GameState.INGAME || state == PlayerState.DEAD)
            return;
        
        // Check if the user launched an attack
        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetAxis("Fire1") > 0) && state != PlayerState.ATTACKING && Time.time > lastAttackAt + attackCoolDown)
            StartAttack();

        // Check if the user launched a dash
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxis("Jump") > 0) && Time.time > lastDashAt + dashCoolDown)
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
                Move();
                break;
        }

        if(state == PlayerState.IDLE)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isWalkingUp", false);
            animator.SetBool("isWalkingDown", false);

            switch (actualOrientation)
            {
                case Orientation.UP:
                    animator.SetBool("isIdleUp", true);
                    animator.SetBool("isIdleDown", false);
                    break;
                case Orientation.DOWN:
                    animator.SetBool("isIdleUp", false);
                    animator.SetBool("isIdleDown", true);
                    break;
                case Orientation.HORIZONTAL:
                    animator.SetBool("isIdleUp", false);
                    animator.SetBool("isIdleDown", false);
                    break;
            }
        }
        else
        {
            animator.SetBool("isIdleUp", false);
            animator.SetBool("isIdleDown", false);
        }
    }

    private void Move()
    {
        if (Input.GetKey(KeyCode.W)||Input.GetAxis("Vertical") > 0)
            movementUp = 1;
        else
            movementUp = 0;

        if (Input.GetKey(KeyCode.S) || Input.GetAxis("Vertical") < 0)
            movementDown = 1;
        else
            movementDown = 0;

        if (Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0)
            movementRight = 1;
        else
            movementRight = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0)
            movementLeft = 1;
        else
            movementLeft = 0;

        Vector3 moveVector = new Vector3(movementRight - movementLeft, movementUp - movementDown);

        moveVector *= moveSpeed * rage.activeRageMultiplier * Time.deltaTime;
        
        if (moveVector == Vector3.zero)
            state = PlayerState.IDLE;
        else
            state = PlayerState.WALKING;
        
        rigid.velocity = moveVector;

        // Animator managing state
        if(movementRight > 0 || movementLeft > 0)
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
        else if(movementUp > 0)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isWalkingDown", false);
            animator.SetBool("isWalkingUp", true);

            actualOrientation = Orientation.UP;
        }
        else if(movementDown > 0)
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

        switch(actualOrientation)
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

        dashVector *= dashSpeed * rage.activeRageMultiplier * Time.deltaTime;
        
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
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time > lastHitAt + invicibilityTimeAfterHit && other.tag == "FoeAttack")
        {
            lastHitAt = Time.time;
            if (life.ApplyDamage(other.GetComponentInParent<FoeController>().attackDamage))
            {
                rigid.isKinematic = true;
                animator.SetBool("isIdleUp", false);
                animator.SetBool("isIdleDown", false);
                animator.SetBool("isWalking", false);
                animator.SetBool("isWalkingUp", false);
                animator.SetBool("isWalkingDown", false);
                animator.SetBool("isDead", true);
                rigid.velocity = Vector2.zero;
                state = PlayerState.DEAD;
                gameManager.gameState = GameManager.GameState.DEATH;
            }
        }
    }
}
