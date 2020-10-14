using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCommand
{
    public int InputX;
    public int InputY;
    public bool InputDash;
}

public class BotController : MonoBehaviour
{
    public ParticleSystem dust;
    
    public float runSpeed = 2f;
    public float dashMultiplicator = 5f;

    public bool active = false;
    public InputCommand command;
    public float duration = 0f;
    public int inputIndex = 0;
    public float dashTime = 0f;
    
    public float movementX, movementY;

    public RuntimeAnimatorController playerController;
    public RuntimeAnimatorController botController;

    public bool IsDashing { get => dashTime > 0; }

    private SpriteRenderer playerSprite;
    private Animator animator;
    private Rigidbody2D rigid;

    // Start is called before the first frame update
    void Awake()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ProcessAnimation();
    }

    void FixedUpdate()
    {
        ProcessMovement();
    }

    public IEnumerator MoveToSpawn(Vector3 pos) 
    {
        // disable physics
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        playerSprite.flipX = false;
        
        float distToSpawn = 1f;
        float animStep = 0f;
        while (distToSpawn > 0.1f)
        {
            animStep += 2f * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, pos, animStep);

            distToSpawn = Vector3.Distance(pos, transform.position);

            yield return null;
        }

        transform.position = pos;

        // enable physics again
        GetComponent<Collider2D>().enabled = true;
        GetComponent<Rigidbody2D>().simulated = true;
    }

    public void ResetBot()
    {
        active = false;

        ChangeAppearanceToBot();

        // hide offscreen
        transform.position = new Vector3(0, 1000);

        // clear the inputs memory too
        inputIndex = 0;
        command = new InputCommand();
        duration = 0;

        dashTime = 0;
    }

    public void ChangeAppearanceToPlayer()
    {
        animator.runtimeAnimatorController = playerController as RuntimeAnimatorController;
    }

    public void ChangeAppearanceToBot()
    {
        animator.runtimeAnimatorController = botController as RuntimeAnimatorController;
    }

    public void MoveX(float x) 
    {
        movementX = x;
    }

    public void MoveY(float y) 
    {
        movementY = y;
    }
    
    void ProcessMovement()
    {   
        Vector3 movement = new Vector3(movementX, movementY) * runSpeed;
        
        if (IsDashing) movement *= dashMultiplicator;

        movement *= Time.deltaTime;

        rigid.MovePosition(transform.position + movement * 50f * Time.fixedDeltaTime);

        movementX = 0;
        movementY = 0;
    }

    void ProcessAnimation()
    {
        if (movementX < 0)
        {
            playerSprite.flipX = true;
        }
        else if (movementX > 0)
        {
            playerSprite.flipX = false;
        }

        if (movementX == 0f && movementY == 0f)
        {
            animator.SetBool("Moving", false);
        }
        else 
        {
            animator.SetBool("Moving", true);
            CreateDust();
        }
    }

    void CreateDust()
    {
        dust.Play();
    }
}
