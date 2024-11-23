using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Physics components")]
    [SerializeField] private Rigidbody2D rb;
    [Header("Walking")]
    [SerializeField] private float speed;
    [SerializeField] private FacingDirection currentDirection;
    [Header("Raycast")]
    [SerializeField] private LayerMask layer;
    [SerializeField] private float jumpHeight = 5;
    [SerializeField] private int raycastDistance = 2;
    [Space]
    [Space]
    [Header("Jump varaibles")]
    //jump
    [SerializeField] private float ApexTimeJump = 2;
    [SerializeField] private float ApexHeightJump = 2 ;
    [SerializeField] private float terminalSpeed;
    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }


    private void TerminalVelocityCheckr()
    {
        // create new variable and attach it to speed
        Vector2 playerVelocity = rb.velocity;
        //clamp new variable to terminal speed, its negative because its falling
        playerVelocity.y = Mathf.Clamp(rb.velocity.y, -1 *terminalSpeed, 1000);

        //use variable as the velocity of the player
        rb.velocity = playerVelocity;

    }



    // Update is called once per frame
    void Update()
    {

        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.
        Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"),0);
        MovementUpdate(playerInput);
        
    }

    private void MovementUpdate(Vector2 playerInput)
    {

        float gravity = -2 * ApexHeightJump / (ApexTimeJump * ApexTimeJump);
        float jumpVelocity = 2 * ApexHeightJump / ApexTimeJump;


        rb.AddForce(playerInput * speed);
        float velocityX = rb.velocity.x;


        if (IsGrounded() && Input.GetKey(KeyCode.Space) )
        {
            rb.gravityScale = 0;
            //rb.AddForce(new Vector2(0, jumpHeight),ForceMode2D.Impulse); old jump
            //new jump bellow

            rb.velocity = new Vector2(velocityX, jumpVelocity);

            
        }
        else if (!IsGrounded())
        {
            rb.velocity = new Vector2(velocityX, rb.velocity.y + gravity * Time.deltaTime);
            rb.gravityScale = 0;
            TerminalVelocityCheckr();
            
        }
        else
        {
            rb.gravityScale = 1;
        }

    }

    public bool IsWalking()
    {
        if (Input.GetAxis("Horizontal") > 0.1 || Input.GetAxis("Horizontal") < -0.1)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public bool IsGrounded()
    {
        
        RaycastHit hit;
        Debug.DrawLine(transform.position, transform.position + Vector3.down * raycastDistance, Color.white);
        //Physics.Raycast(transform.position, transform.position + Vector3.down, out hit, raycastDistance, layer);
        if (Physics2D.Raycast(transform.position, transform.position + Vector3.down, raycastDistance, layer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public FacingDirection GetFacingDirection()
    {
        if (Input.GetAxis("Horizontal") > 0.1)
        {

            currentDirection = FacingDirection.right;

            return currentDirection;
            
        }
        if (Input.GetAxis("Horizontal") < -0.1)
        {

            currentDirection = FacingDirection.left;
            return currentDirection;
        }
        else
            return currentDirection;
    }
}
