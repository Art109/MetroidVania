using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField]protected float health;
    [SerializeField]protected float recoilLength;
    [SerializeField]protected float recoilFactor;
    [SerializeField]protected bool isRecoling = false;

    [SerializeField] protected PlayerController player;
    [SerializeField]protected float speed;
    [SerializeField]protected int damage;

    protected float recoilTimer;
    protected Rigidbody2D rb;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }

        if(isRecoling)
        {
            if(recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        health -= damageDone;
        if(!isRecoling)
        {
            rb.AddForce(-hitForce * recoilLength * hitDirection);
        }
    }

    protected void OnTriggerStay2D(Collider2D col)
    {
        if(col.CompareTag("Player") &&  !PlayerController.Instance.pState.invincible)
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }
}
