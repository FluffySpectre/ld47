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

    // Start is called before the first frame update
    void Awake()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        ProcessAnimation();
        ProcessMovement();
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

        transform.Translate(movement, Space.World);
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
        }
    }
}
