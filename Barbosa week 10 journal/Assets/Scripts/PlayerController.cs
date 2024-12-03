using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using static PlayerController;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.LightAnchor;
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
    [SerializeField] private float terminalSpeed;

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


    [Header("wall climb")]
    [SerializeField] private bool isClimbing;
    [SerializeField] private float climbSpeed;
    [SerializeField] private Vector2 boxSize;
    public LayerMask wallCheckerLayerMask;

    [Header("RocketJump")]
    [SerializeField] private Vector2 mouseLocation;
    [SerializeField] private float throwForce;
    [SerializeField] private float RocketCooldownTimer;
    [SerializeField] private float RocketCooldownTimerMax;
    private Vector2 angle;
    
    //directions in which the player can face
    public enum FacingDirection
    {
        left, right
    }

    //player states 
    public enum PlayerState
    { 
        idle, walking, jumping, dead, dahsing, climbing
    }

    // Start is called before the first frame update
    void Start()
    {
        //set  gravity to 0
        body.gravityScale = 0;
        //use formula to get the speed ( aceleration , decelaration, graivty and jump speed)
        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationTime;
        gravity = -2 * apexHeight / (apexTime * apexTime);
        InitialJumpSpeed = 2 * apexHeight / apexTime;
    }



    private void FixedUpdate()
    {
        if (IsDashing)
        {
            //if youre pressed the inputs then dash
            applyDash();
        }
        RocketJump();

        //get the player X and Y input
        Vector2 playerInput = new Vector2();
        Vector2 playerInputY = new Vector2();

        //get the players horizontal and vertical input
        playerInput.x = Input.GetAxisRaw("Horizontal");
        playerInputY.y = Input.GetAxisRaw("Vertical");

        climbWall(playerInputY);

        if (!IsDashing)
        {
            MovementUpdate(playerInput);
            jumpUpdate();
            //Clamps the player speed when they're not dashing to not keep the momentum
            velocity.x = Mathf.Clamp(velocity.x,-maxSpeed, maxSpeed);
        }

        body.velocity = velocity;

        if (isClimbing) // if climbing set velocity 0 so they dont fall down from wall
        {
            velocity.y = 0;
        }

        else if (!isGrounded)// in air
        {
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, -terminalSpeed);
        }

        else
        {
            velocity.y = 0;
        }

    }

    // Update is called once per frame
    void Update()
    {
        //set previous state as the current one
        previousState = currentState;
        if (!canDash)
        {
            //if cant dash go on cooldown
            DashCooldown();
        }
        if (canDash)
        {
            //if can dash get input to check if youre going to
            GetDashInput();
        }
        checkForGround();
        getMouseLocation();
        DetectWall();
        if (isDead)
        {
            currentState = PlayerState.dead;
        }

        switch(currentState)
        {
            case PlayerState.dead:
                break;


                //if the playyer is still then enter idle
            case PlayerState.idle:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x > 0) currentState = PlayerState.walking;
                else if (velocity.x < 0) currentState = PlayerState.walking;
                else if (IsDashing) currentState = PlayerState.dahsing;
                else if (isClimbing) currentState = PlayerState.climbing; 
                break;


                //if the player is moving on the ground then enter walking
            case PlayerState.walking:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x == 0) currentState = PlayerState.idle;
                else if (IsDashing) currentState = PlayerState.dahsing;
                else if (isClimbing) currentState = PlayerState.climbing;
                break;

                
                //if the player is jumping or falling enter jumping
            case PlayerState.jumping:
                if(isGrounded)
                {
                    if (velocity.x != 0) currentState = PlayerState.walking;
                    else if (IsDashing) currentState = PlayerState.dahsing;
                    else currentState = PlayerState.idle;
                }
                else if (isClimbing) currentState = PlayerState.climbing;
                break;


                //if the player is dashing on the ground enter dashing
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
                else if (isClimbing) currentState = PlayerState.climbing;
                break;


                //if the player is climbing wall enter climbing
            case PlayerState.climbing:
                if (!isClimbing && velocity.x != 0 && isGrounded)
                {
                    currentState = PlayerState.walking;
                }
                else if (!isClimbing && velocity.x != 0 && !isGrounded)
                {
                    currentState = PlayerState.jumping;
                }
                else if (!isClimbing && velocity.x == 0)
                {
                    currentState = PlayerState.idle;
                }
                break;
        }

    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x < 0)
        {
            //if the input is negative theyre facing the left
            currentDirection = FacingDirection.left;
        }
        else if(playerInput.x >0)
        {
            //if the input is positive theyre facing the right
            currentDirection = FacingDirection.right;
        }

        if (playerInput.x != 0)
        {
            //acelerating and clamping the player speed 
            velocity.x += accelerationRate * playerInput.x * Time.deltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        }
        else
        {
            if (velocity.x > 0)
            {
                //decelerating on the right and limiting the max speed
                velocity.x -= decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Max(velocity.x, 0);
            }
            else if(velocity.x < 0)
            {
                //decelerating on the left and limiting the max speed
                velocity.x += decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Min(velocity.x, 0);
            }
        }
    }


    private void jumpUpdate()
    {
        if ( isGrounded && Input.GetButton("Jump"))
        {
            //if the player is grounded and presses thekey to jump apply initial jump speed and set them as not grounded anymore
            velocity.y = InitialJumpSpeed;
            isGrounded = false;
        }
    }

    private void checkForGround()
    {
        //check if the player overlap box touches the ground with the layer ground then return true if so
        isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * groundCheckOffset,groundCheckSize,0,groundCheckLayerMask);
    }

    private void OnDrawGizmos()
    {
        //gizmos for easier readability
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset,groundCheckSize);
        Gizmos.DrawWireCube(transform.position, boxSize);
    }

    public bool IsWalking()
    {
        //if the player velocity is not 0 then theyre walking
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
        //if the player is grounded return true
        return isGrounded;
    }

    public FacingDirection GetFacingDirection()
    {
        //return the direction the player is currently facing
        return currentDirection;
    }

    //Dash
    private void GetDashInput()
    {
        //get input
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            
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
        
        //start timer 
        cooldown += Time.deltaTime;
        //if cooldown ends allow player to dash again
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
        //get a overlapbox to check if the player is touching the wall 
        if(Physics2D.OverlapBox(transform.position, boxSize,0,wallCheckerLayerMask))
        {
            //if the player is presses any of these keys when near a wall set climbing as true 
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
    private void climbWall(Vector2 playerInputY)
    {
       if(isClimbing)
        {
            //aplpy speed on the Y so the player feels like they're climbing up or down
            velocity.y += climbSpeed * playerInputY.y * Time.deltaTime;
            Debug.Log(velocity.y);

        }

    }

    // RocketJump
    private void getMouseLocation()
    {
        //get the mouse location and and lock it to the screen
        mouseLocation = Input.mousePosition;
        mouseLocation = Camera.main.ScreenToWorldPoint(mouseLocation);
        Debug.DrawLine(transform.position, mouseLocation);

        //get the new direction
        Vector2 directionMouse =  new Vector2(mouseLocation.x - transform.position.x, mouseLocation.y - transform.position.y);

        //Invert it os the force is applied in the oposite direciton, making it feel like an action recoil
        angle = - directionMouse.normalized;
    }
    private void RocketJump()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //get a ray for debuging 
            Debug.DrawRay(transform.position, angle * throwForce, Color.blue, 1f);

            //add force
            body.AddForce((10*throwForce) * angle, ForceMode2D.Force);

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

