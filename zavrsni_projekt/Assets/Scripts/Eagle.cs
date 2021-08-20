using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eagle : Enemy
{
    private Rigidbody2D eagle;
    private Collider2D coll; 

    protected override void Start()
    {
        base.Start();
        coll = GetComponent<Collider2D>();
        eagle = GetComponent<Rigidbody2D>();
    }
}
