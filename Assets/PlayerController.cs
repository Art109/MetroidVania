using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Momevement Settings")]
    Rigidbody2D rb;
    [SerializeField] float walkSpeed = 1f;
    float xAxis;

    [Header("Ground Check Settings")]
    [SerializeField]float jumpForce = 45;
    [SerializeField]Transform grounCheckPoint;
    [SerializeField]float groundCheckY = 0.2f;
    [SerializeField]float groundCheckX = 0.5f;
    [SerializeField]LayerMask groundLayer;

    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        Move();
        Jump();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis , rb.velocity.y);
    }

    //Pesquisar sobre Raycast 
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
    }
}
