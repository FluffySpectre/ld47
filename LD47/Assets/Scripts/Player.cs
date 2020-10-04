using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float runSpeed = 2f;
    public float dashMultiplicator = 5f;
    
    private bool kicking;
    private Vector3 movement = Vector3.zero;

    public bool IsKicking { get => kicking; }

    private SpriteRenderer playerSprite;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.PlayerControllable) {
            movement = Vector3.zero;
            animator.SetBool("Moving", false);
            return;
        }
    
        ProcessInput();
        ProcessMovement();
        ProcessAnimation();
    }

    void ProcessInput()
    {   
        float x = Input.GetAxisRaw("Horizontal") * runSpeed * Time.deltaTime;
        float y = Input.GetAxisRaw("Vertical") * runSpeed * Time.deltaTime;
        movement = new Vector3(x, y, 0);

        // slow down diagonal movement
        if (x != 0f && y != 0f)
        {
            movement *= 0.75f;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            kicking = true;
        }
        else
        {
            kicking = false;
        }
    }

    void ProcessMovement()
    {   
        if (kicking)
        {
            // TODO: do dash
            movement *= dashMultiplicator;
        }
        
        transform.Translate(movement, Space.World);
    }

    void ProcessAnimation()
    {
        if (movement.x < 0)
        {
            playerSprite.flipX = true;
        }
        else if (movement.x > 0)
        {
            playerSprite.flipX = false;
        }

        if (movement.sqrMagnitude != 0f)
        {
            animator.SetBool("Moving", true);
        }
        else 
        {
            animator.SetBool("Moving", false);
        }
    }
}
