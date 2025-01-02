using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] float walkSpeed = 1f;
    [Space(5)]
    
    [Header("Vertical Movement Settings")]
    [SerializeField]float jumpForce = 45;
    float jumpBufferCounter = 0;
    [SerializeField]float jumpBufferTimer = 0.2f;
    float coyoteTimeCounter = 0;
    [SerializeField] float coyoteTime;
    int airJumpCounter = 0;
    [SerializeField] int maxAirJumps;
    [Space(5)]

    [Header("Ground Check Settings")]    
    [SerializeField]Transform grounCheckPoint;
    [SerializeField]float groundCheckY = 0.2f;
    [SerializeField]float groundCheckX = 0.5f;
    [SerializeField]LayerMask groundLayer;
    [Space(5)]

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;
    [SerializeField] GameObject dashEffect;
    [Space(5)]


    [Header("Attack Settings")]
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    bool attack = false;
    [SerializeField] float timeBetweenAttack; 
    float timeSinceAttack;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;
    [SerializeField] GameObject slashEffect;

    bool restoreTime;
    float restoreTimeSpeed;
    [Space(5)]

    [Header("Recoil Settings")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;

    [Header("Health Settings")]
    [SerializeField]int health;
    [SerializeField]int maxHealth;
    public int MaxHealth{ get{ return maxHealth;}}
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    float healTimer;
    [SerializeField] float timeToHeal;

    [Header("Mana Settings")]
    [SerializeField]Image manaStorage;
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;    



    public PlayerStateList pState;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    float gravity;
    float xAxis, yAxis;
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
        Health = maxHealth;
    }

    
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gravity = rb.gravityScale;

        Mana = mana;
        manaStorage.fillAmount = Mana;
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
        RestoreTimeScale();
        FlashWhileInvincible();
        Heal();
        
    }

    void FixedUpdate()
    {
        if(pState.dashing)
            return;
        Recoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
    }

    void Flip(){
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-1 , transform.localScale.y);
            pState.lookingRight = false;
        }
            
        else if(xAxis > 0)
        {
            transform.localScale = new Vector2(1 , transform.localScale.y);
            pState.lookingRight = true;
        }
            
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if(timeSinceAttack > timeBetweenAttack && attack)
        {
            timeSinceAttack = 0;
            animator.SetTrigger("Attacking");

            if(yAxis == 0 || yAxis < 0  && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX , recoilXSpeed);
                Instantiate(slashEffect, SideAttackTransform);
            }
            else if(yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY , recoilYSpeed);
                SlashEffectAngle(slashEffect, 90, UpAttackTransform);
            }
            else if(yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY , recoilYSpeed);
                SlashEffectAngle(slashEffect, -90, DownAttackTransform);
            }
        }
    }

    void Hit(Transform attackTransform, Vector2 attackArea, ref bool recoilDir, float recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea , 0 , attackableLayer);

        if(objectsToHit.Length > 0)
        {
            recoilDir = true;
            
            foreach( var obj in objectsToHit)
            {
                if(obj.GetComponent<Enemy>() != null)
                {
                    obj.GetComponent<Enemy>().EnemyHit(damage, (transform.position - obj.transform.position).normalized, recoilStrength);
                    Mana += manaGain;
                }
            }
        }
    }

    void SlashEffectAngle(GameObject slashEffect, int effectAngle, Transform attackTransform)
    {
        slashEffect = Instantiate(slashEffect, attackTransform);
        slashEffect.transform.eulerAngles = new Vector3(0,0,effectAngle);
        slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
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

    void Recoil()
    {
        if(pState.recoilingX)
        {
            if(pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if(pState.recoilingY)
        {
            rb.gravityScale = 0;
            if(yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else if(yAxis > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        if(pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }

        if(pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if(Grounded())
        {
            StopRecoilY();
        }
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    public void TakeDamage(float damage)
    {
        Health -= Mathf.RoundToInt(damage);
        
        StartCoroutine(StopTakingDamage());
    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        GameObject bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(bloodSpurtParticles, 1.5f);
        animator.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    void FlashWhileInvincible()
    {
        spriteRenderer.material.color = pState.invincible ? 
            Color.Lerp(Color.white , Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1f)) : 
            Color.white;
    }

    void RestoreTimeScale()
    {
        if(restoreTime)
        {
            if(Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    public void HitStopTime(float newTimeScale, int restoreSpeed, float delay)
    {
        restoreTimeSpeed = restoreSpeed;
        Time.timeScale = newTimeScale;

        if(delay > 0)
        {
            StopCoroutine(StartTimeAgain(delay));
            StartCoroutine(StartTimeAgain(delay));
        }
        else
        {
            restoreTime = true;
        }
    }

    IEnumerator StartTimeAgain(float delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(delay);
    }

    public int Health
    {
        get{ return health;}

        set
        {
            if(health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if(onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

    float Mana
    {
        get { return mana;}

        set {
            if( mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = Mana;
            }

        }
    }

    void Heal()
    {
        if(Input.GetButton("Heal") && Health < maxHealth && Mana > 0 && !pState.jumping && !pState.dashing)
        {
            pState.healing = true;
            animator.SetBool("Healing",true);
            healTimer += Time.deltaTime;
            if(healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }

            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            animator.SetBool("Healing",false);
            pState.healing = false;
            healTimer = 0;
        }
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
