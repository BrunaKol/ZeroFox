using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : Enemy
{
    private Collider2D coll;
   

    private bool facingLeft = true;

    [SerializeField] private LayerMask ground;


    [SerializeField] private float leftCap;
    [SerializeField] private float rightCap;

    [SerializeField] private float jumpLenght = 10f;
    [SerializeField] private float jumpHeight = 15f;

    protected override void Start()
    {
        base.Start();
        coll = GetComponent<Collider2D>();
    }

    private void Update()
    {
        //transition from jump to fall
        //transition from fall to idle

        if (anim.GetBool("Jumping"))
        {
            if (rb.velocity.y < .1)
            {
                anim.SetBool("Falling", true);
                anim.SetBool("Jumping", false);
            }
        }

        if (anim.GetBool("Falling") && coll.IsTouchingLayers(ground))
        {
            anim.SetBool("Falling", false);
            anim.SetBool("Jumping", false);
        }
    }

    private void Move()
    {
        if (facingLeft)
        {
            //test to see if we are beyond left cap 
            if (transform.position.x > leftCap)
            {
                //da je okrenut na pravu stranu provjera i ako nije okrenut ga 
                if (transform.localScale.x != 1)
                {
                    transform.localScale = new Vector3(1, 1);
                }
                //test to see if the i am on the ground if so jump
                if (coll.IsTouchingLayers(ground))
                {
                    rb.velocity = new Vector2(-jumpLenght, jumpHeight);
                    anim.SetBool("Jumping", true);
                }
            }
            else
            {
                facingLeft = false;
            }
            //if it is not, we are going to face right
        }
        else
        {
            if (transform.position.x < rightCap)
            {
                //da je okrenut na pravu stranu provjera i ako nije okrenut ga 
                if (transform.localScale.x != -1)
                {
                    transform.localScale = new Vector3(-1, 1);
                }
                //test to see if the i am on the ground if so jump
                if (coll.IsTouchingLayers(ground))
                {
                    rb.velocity = new Vector2(jumpLenght, jumpHeight);
                    anim.SetBool("Jumping", true);
                }
            }
            else
            {
                facingLeft = true;
            }
            //if it is not, we are going to face right
        }
    }


}
