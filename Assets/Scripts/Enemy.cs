using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField]float health;
    [SerializeField]float recoilLength;
    [SerializeField]float recoilFactor;
    [SerializeField]bool isRecoling = false;

    float recoilTimer;
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
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

    public void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        health -= damageDone;
        if(!isRecoling)
        {
            rb.AddForce(-hitForce * recoilLength * hitDirection);
        }
    }
}
