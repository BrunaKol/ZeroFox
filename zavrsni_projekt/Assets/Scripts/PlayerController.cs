using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //Start varijable
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;
    
    

    

    //enumerator varijable (FSM)
    private enum State { idle, running, jumping, falling, hurt, climb}
    private State state = State.idle;

    //Varijable za stepenice
    [HideInInspector] public bool canClimb = false;
    [HideInInspector] public bool bottomLadder = false;
    [HideInInspector] public bool topLadder = false;
    public Ladder ladder;
    private float naturalGravity;
    [SerializeField] float climbSpeed = 3f;
    
    //Inspector Varijable
    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpforce = 10f;
    [SerializeField] private int cherries = 0;
    [SerializeField] private TextMeshProUGUI cherryNumber;
    [SerializeField] private float hurtForce = 10f;
    [SerializeField] private AudioSource cherry;
    [SerializeField] private AudioSource footstep;
    [SerializeField] private int health;
    [SerializeField] private Text HealthAmount;


    // start se zove na poèetku da se postave osnovni parametri za igranje
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        HealthAmount.text = health.ToString();
        naturalGravity = rb.gravityScale;
    }

    //update se zove svaki frame
    private void Update()
    {
        if (state == State.climb)
        {
            Climb();
        }
        else if (state != State.hurt)
        {
            Movement();
        }
        AnimationState();
        anim.SetInteger("state", (int)state); //sets animation s obzirom na enumerator
    }


    //registrira nam dodirivanje s objektima koji prilikom kolizije nestaju te se brojaè ++1, gem ili cherry
    // tu nam trigger registrira i šalje koji se nalazi u inspectoru u boxcollideru
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Collectible")
        {
            cherry.Play();
            Destroy(collision.gameObject);
            cherries += 1;
            cherryNumber.text = cherries.ToString();
        }

        else if (collision.tag == "PowerUp")
        {
            Destroy(collision.gameObject);
            jumpforce += 8;
            GetComponent<SpriteRenderer>().color = Color.yellow;
            StartCoroutine(ResetPower());
        }
    }

    //registrira dodirivanje objekta koji æe ili uništiti nas ili mi njega odnosno enemy
    // ovdje nije aktivan on trigger jer ne možemo "proæi kroz neprijatelja"
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy") 
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            if ( state == State.falling) 
            {
                enemy.JumpedOn();
                Jump();
            }
            else
            {
                state = State.hurt;
                HandleHealth();

                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    //nalazi nam se s desna jer je njegova pozicija na x osi veæa od naše pa nas je ozljedio i vraæa nas u lijevo
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    //nalazi nam se s lijeva, jer mu je u ovom sluèaju pozicij ana x manja od naše, pa nas ozlijedi i odbije u desno
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }

        }
    }

    private void HandleHealth()
    {
        // smanjuje health i resetira level ako se potroši

        health -= 10;
        HealthAmount.text = health.ToString();

        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Movement()
    {
        float hDirection = Input.GetAxisRaw("Horizontal");
        float vDirection = Input.GetAxisRaw("Jump");

        //provjeravamo da li se može penjati i da li je pritisnuta tipka za dolje ili gore 
        if (canClimb && Mathf.Abs(Input.GetAxisRaw("Vertical")) > .1f)
        {
            state = State.climb;
            //zamrzne nam rotaciju odnosno y os i x os tj da smo pozicionirani na skalama da ne idemo dallje 
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            //postavlja igraèa na poziciju od stepenica
            transform.position = new Vector3(ladder.transform.position.x, rb.position.y);
            rb.gravityScale = 0f;
        }

        //moving left
        if (hDirection < 0)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            gameObject.GetComponent<SpriteRenderer>().flipX = true;
            //transform.localScale = new Vector2(-1,1);
        }
        //moving right
        else if (hDirection > 0)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            gameObject.GetComponent<SpriteRenderer>().flipX = false;
            //transform.localScale= new Vector2 (1,1);
        }
        //jumping
        if (vDirection > 0f && coll.IsTouchingLayers(ground)) 
        {
            Jump();
        }
    }

    private void Jump() 
    {
        float vDirection = Input.GetAxis("Jump");
        
        rb.velocity = new Vector2(rb.velocity.x, jumpforce);
        state = State.jumping;
    }

    private void AnimationState()
    {
        if (state == State.climb)
        {

        }
        else if (state == State.jumping)
        {
            if (rb.velocity.y < .1f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }

        }
        else if (state == State.hurt) 
        {
            if (Mathf.Abs(rb.velocity.x) < .1f)
            {
                state = State.idle;
            }
               
        }
        else if (Mathf.Abs(rb.velocity.x) > 2f)
        {
            //Moving, mathf.abs vraæa absolute number od x axise koji ako se mièemo lijevo ili desno je uvijek veæi 
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
        

    }

    private void Footstep()
    {
        if (state == State.running)
        {
            footstep.Play();
        }
    }

    private IEnumerator ResetPower()
    {
        yield return new WaitForSeconds(6);
        jumpforce = 12;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void Climb()
    {
        float vDirection = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            //vrati nam na normalnu sve osim y osi
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            canClimb = false;
            rb.gravityScale = naturalGravity;
            anim.speed = 1f;
            Jump();
            return;
        }

        //penjanje ako je stisnuto za gore
        if (vDirection > .1f && !topLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
            anim.speed = 1f;
        }

        //spuštanje
        else if(vDirection < -.1f && !bottomLadder)
        {
            rb.velocity = new Vector2(0f, vDirection * climbSpeed);
            anim.speed = 1f;
          
        }

        //mirovanje
        else
        {
            anim.speed = 0f;
            rb.velocity = Vector2.zero;
        }

    }





}
