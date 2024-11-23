using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Momevement Settings")]
    [SerializeField] float walkSpeed = 1f;
    

    [Header("Ground Check Settings")]
    [SerializeField]float jumpForce = 45;
    [SerializeField]Transform grounCheckPoint;
    [SerializeField]float groundCheckY = 0.2f;
    [SerializeField]float groundCheckX = 0.5f;
    [SerializeField]LayerMask groundLayer;

    Rigidbody2D rb;
    Animator animator;
    float xAxis;


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
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        Move();
        Jump();
        Flip();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    void Flip(){
        if(xAxis < 0)
            transform.localScale = new Vector2(-1 , transform.localScale.y);
        else if(xAxis > 0)
            transform.localScale = new Vector2(1 , transform.localScale.y);
    }

    void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis , rb.velocity.y);
        animator.SetBool("Walking", rb.velocity.x != 0 && Grounded() );
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
        if(Input.GetButtonUp("Jump") && rb.velocity.y >0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        if(Input.GetButtonDown("Jump") && Grounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        }

        animator.SetBool("Jumping", !Grounded());
    }
}
