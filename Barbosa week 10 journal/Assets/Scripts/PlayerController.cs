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

    [Header("Physics")]
    [SerializeField] private Rigidbody2D body;
    [Header("Horizontal")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float accelerationTime = 0.25f;
    [SerializeField] private float decelerationTime = 0.15f;

    [Header("Verital")]
    public float apexHeight = 3f;
    public float apexTime = 5;
    private float accelerationRate;
    private float decelerationRate;


    private Vector2 velocity;
    [SerializeField] private FacingDirection currentDirection;
    private float gravity;
    private float InitialJumpSpeed;

    [Header("Ground Check")]
    public float groundCheckOffset = 0.5f;
    public Vector2 groundCheckSize = new Vector2(0.4f, 0.1f);
    public LayerMask groundCheckLayerMask;

    private bool isGrounded;
    private bool isDead = false;

    public PlayerState currentState = PlayerState.idle;
    public PlayerState previousState = PlayerState.idle;

    [Header("Dash")]
    //physics and dash 
    [SerializeField] private bool IsDashing;
    [SerializeField] private float dashMultiplier;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashTimeMax;
    //dash cooldown
    [SerializeField] private float cooldown;
    [SerializeField] private float cooldownMaxTime;
    [SerializeField] private bool canDash = true;


    [Header("Dash")]
    [SerializeField] private bool isClimbing;
    [SerializeField] private float climbSpeed;
    [SerializeField] private Vector2 boxSize;
    public LayerMask wallCheckerLayerMask;

    public enum FacingDirection
    {
        left, right
    }

    public enum PlayerState
    { 
        idle, walking, jumping, dead, dahsing, climbing
    }

    // Start is called before the first frame update
    void Start()
    {
        body.gravityScale = 0;
        //rb = GetComponent<Rigidbody2D>();
        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationTime;
        gravity = -2 * apexHeight / (apexTime * apexTime);
        InitialJumpSpeed = 2 * apexHeight / apexTime;
    }




    // Update is called once per frame
    void Update()
    {
        previousState = currentState;
        if (!canDash)
        {
            DashCooldown();
        }
        if (canDash)
        {
            GetDashInput();
        }
        if(IsDashing)
        {
            applyDash();
        }


        checkForGround();
        DetectWall();
        climbWall();
        Vector2 playerInput = new Vector2();
        playerInput.x = Input.GetAxisRaw("Horizontal");

        if(isDead)
        {
            currentState = PlayerState.dead;
        }

        switch(currentState)
        {
            case PlayerState.dead:
                break;
            case PlayerState.idle:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x > 0) currentState = PlayerState.walking;
                else if (IsDashing) currentState = PlayerState.dahsing;
                break;
            case PlayerState.walking:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x == 0) currentState = PlayerState.idle;
                else if (IsDashing) currentState = PlayerState.dahsing;
                break;
            case PlayerState.jumping:
                if(isGrounded)
                {
                    if (velocity.x != 0) currentState = PlayerState.walking;
                    else if (IsDashing) currentState = PlayerState.dahsing;
                    else currentState = PlayerState.idle;
                    
                }
                break;
            case PlayerState.dahsing:
                if(!IsDashing && velocity.x !=0 && isGrounded) 
                { 
                    currentState = PlayerState.walking;
                }
                else if (!IsDashing && velocity.x != 0 && !isGrounded)
                {
                    currentState = PlayerState.jumping;
                }
                else if (!IsDashing &&velocity.x == 0)
                {
                    currentState = PlayerState.idle;
                }
                break;
        }

        if(!IsDashing)
        {
            MovementUpdate(playerInput);
            jumpUpdate();
        }

        body.velocity = velocity;
        if (!isClimbing)
        {
            if (!isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }
        else
        {
            velocity.y = 0;
        }

       
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


    private void jumpUpdate()
    {
        if ( isGrounded && Input.GetButton("Jump"))
        {
            velocity.y = InitialJumpSpeed;
            isGrounded = false;
        }
    }

    private void checkForGround()
    {
        isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * groundCheckOffset,groundCheckSize,0,groundCheckLayerMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset,groundCheckSize);
        Gizmos.DrawWireCube(transform.position, boxSize);
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
        return isGrounded;
    }

    public FacingDirection GetFacingDirection()
    {
        return currentDirection;
    }

    //Dash
    private void GetDashInput()
    {
        //get input
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("got into dash timer");
            IsDashing = true;
        }
    }

    private void applyDash()
    {
        if(IsDashing)//check if dashing
        {
            dashTime += Time.deltaTime; // increase timer
            if (currentDirection == FacingDirection.right)
            {
                if (dashTime < dashTimeMax) // check if timer is done
                {
                    velocity.x += accelerationRate * dashMultiplier * Time.deltaTime;
                    Debug.Log("right");
                    canDash = false;
                }
                else // if timer is done set dashing off
                {
                    dashTime = 0;
                    IsDashing = false;
                }
            }
             else if (currentDirection == FacingDirection.left)
             {
                if (dashTime < dashTimeMax)// check if timer is done
                {
                    velocity.x += accelerationRate * (-1 * dashMultiplier) * Time.deltaTime;
                    Debug.Log("left");
                    canDash = false;

                }
                else // if timer is done set dashing off
                {
                    dashTime = 0;
                    IsDashing = false;
                }
             }
        }
    }

    private void DashCooldown()
    {
        Debug.Log("fell here");
        cooldown += Time.deltaTime;
        if (cooldown >= dashTimeMax && canDash == false)
        {
            canDash = true;
            cooldown = 0;
        }
        else
        {

            canDash = false;
        }
    }

    //climb
    private void DetectWall()
    {
        if(Physics2D.OverlapBox(transform.position, boxSize,0,wallCheckerLayerMask))
        {
            if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                isClimbing = true;

            }
        }
        else
        {
            isClimbing = false;
        }
    }
    private void climbWall(Vector2 playerInput)
    {
       if(isClimbing)
        {
            velocity.y += climbSpeed * playerInput.y * Time.deltaTime;


        }

    }
      
}



    /*
     * OLD CODE
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

