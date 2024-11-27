using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] float walkSpeed = 1f;
    
    [Header("Vertical Movement Settings")]
    [SerializeField]float jumpForce = 45;
    float jumpBufferCounter = 0;
    [SerializeField]float jumpBufferTimer = 0.2f;
    float coyoteTimeCounter = 0;
    [SerializeField] float coyoteTime;
    int airJumpCounter = 0;
    [SerializeField] int maxAirJumps;

    [Header("Ground Check Settings")]    
    [SerializeField]Transform grounCheckPoint;
    [SerializeField]float groundCheckY = 0.2f;
    [SerializeField]float groundCheckX = 0.5f;
    [SerializeField]LayerMask groundLayer;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;
    [SerializeField] GameObject dashEffect;

    [Header("Attack Settings")]
    bool attack = false;
    float timeBetweenAttack, timeSinceAttack;
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;


    PlayerStateList pState;
    Rigidbody2D rb;
    Animator animator;
    float gravity;
    float xAxis;
    bool canDash = true;
    bool dashed;


    public static PlayerController Instance;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position ,SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position ,UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position ,DownAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();
        if(pState.dashing) return;
        Flip();
        Move();
        Jump();
        StartDash();
        Attack();
        
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        attack = Input.GetKeyDown(KeyCode.F);
    }

    void Flip(){
        if(xAxis < 0)
            transform.localScale = new Vector2(-1 , transform.localScale.y);
        else if(xAxis > 0)
            transform.localScale = new Vector2(1 , transform.localScale.y);
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if(timeSinceAttack > timeBetweenAttack && attack)
        {
            timeSinceAttack = 0;
            animator.SetTrigger("Attacking");
        }
    }

    void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis , rb.velocity.y);
        animator.SetBool("Walking", rb.velocity.x != 0 && Grounded() );
    }

    void StartDash()
    {
        if(Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if(Grounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        animator.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        if(Grounded()) Instantiate(dashEffect,transform);

        yield return new WaitForSeconds(dashTime);

        pState.dashing = false;
        rb.gravityScale = gravity;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    public bool Grounded()
    {
        if(Physics2D.Raycast(grounCheckPoint.position, Vector2.down, groundCheckY, groundLayer) 
        || Physics2D.Raycast(grounCheckPoint.position + new Vector3(groundCheckX,0,0), Vector2.down, groundCheckY, groundLayer) 
        || Physics2D.Raycast(grounCheckPoint.position + new Vector3(-groundCheckX,0,0), Vector2.down, groundCheckY, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    void Jump()
    {
        if(Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }

        if(!pState.jumping)
        {
            if(jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);

                pState.jumping = true;
            }
            else if(!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);

                pState.jumping = true;

                airJumpCounter++;
            }
        }

        animator.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables()
    {
        if(Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
            

        if(Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTimer;
        else
            jumpBufferCounter -= Time.deltaTime;
    }
}
