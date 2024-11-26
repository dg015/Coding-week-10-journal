using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using static PlayerController;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.RuleTile.TilingRuleOutput;
using static UnityEngine.UI.Image;

public class PlayerController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D body;
    [Header("Running")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float accelerationTime = 0.25f;
    [SerializeField] private float deacelerationTime = 0.15f;
    private float accelerationRate;
    private float decelerationRate;
    private Vector2 velocity;
    [SerializeField] private FacingDirection currentDirection;

    /*
     * Old variables
    [Header("Physics components")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Walking")]
    [SerializeField] private float speed;
    [SerializeField] private FacingDirection currentDirection;

    [Header("Raycast")]
    [SerializeField] private LayerMask layer;
    [SerializeField] private float raycastDistance = 2;



    [Header("Jump varaibles")]
    //jump
    [SerializeField] private float ApexTimeJump = 2;
    [SerializeField] private float ApexHeightJump = 2 ;
    [SerializeField] private float terminalSpeed;

    [Header("Coyote time")]
    [SerializeField] private bool isJumping;
    [SerializeField] private bool canJump;
    [SerializeField] private float CoyoteTimeMax = 2;
    [SerializeField] private float CoyoteTime;

    */

    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        body.gravityScale = 0;
        //rb = GetComponent<Rigidbody2D>();
        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationRate;
    }




    // Update is called once per frame
    void Update()
    {
        Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"),0);
        MovementUpdate(playerInput);
        body.velocity = velocity;
       
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x < 0)
        {
            currentDirection = FacingDirection.left;
        }
        else if(playerInput.x >0)
        {
            currentDirection = FacingDirection.right;
        }

        if (playerInput.x != 0)
        {
            velocity.x += accelerationRate * playerInput.x * Time.deltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        }
        else
        {
            if (velocity.x > 0)
            {
                velocity.x -= decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Max(velocity.x, 0);
            }
            else if(velocity.x < 0)
            {
                velocity.x += decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Min(velocity.x, 0);
            }
        }
    }


    public bool IsWalking()
    {
        if (body.velocity.x !=0)
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
        return true;
    }

    public FacingDirection GetFacingDirection()
    {
        return currentDirection;
    }

    /*
     *  private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x < 0)
        {
            currentDirection = FacingDirection.left;
        }
        else
        {
            currentDirection = FacingDirection.right;
        }
        float gravity = -2 * ApexHeightJump / (ApexTimeJump * ApexTimeJump);
        float jumpVelocity = 2 * ApexHeightJump / ApexTimeJump;


        rb.AddForce(playerInput * speed);
        float velocityX = rb.velocity.x;


        if (Input.GetKey(KeyCode.Space) && canJump == true) // is grounded and jumped
        {
            rb.gravityScale = 0;
            rb.velocity = new Vector2(velocityX, jumpVelocity);
            isJumping = true;
            canJump = false;
            
        }
        else if (!IsGrounded()) //free falling
        {
            rb.velocity = new Vector2(velocityX, rb.velocity.y + gravity * Time.deltaTime);
            rb.gravityScale = 0;
            TerminalVelocityCheckr();
            /*
             * Check if the person is jumping
             
            if (!isJumping)
            {
                timer();
                if (CoyoteTime <= CoyoteTimeMax ) // get withing the time frame
                {
                    canJump = true;
                }
            }
            else
{
    CoyoteTime = 0;
}

        }
        else if (IsGrounded()) //chillin
{
    CoyoteTime = 0;
    isJumping = false;
    canJump = true;
}


    }

    private void timer()
{
    CoyoteTime += 1 * Time.deltaTime;
    if (CoyoteTime >= CoyoteTimeMax)
    {
        CoyoteTime = 0;
    }
}


public bool IsWalking()
{
    if (rb.velocity.x != 0)
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
    Vector2 origin = new Vector2(transform.position.x, transform.position.y - 0.5f);

    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, raycastDistance, layer);
    Debug.DrawRay(origin, Vector2.down * raycastDistance, Color.red);

    if (hit.collider != null)
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
* */
}
