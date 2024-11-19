using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D colider;
    [SerializeField] private float speed;
    [SerializeField] private FacingDirection currentDirection;
    [SerializeField] private LayerMask layer;
    [SerializeField] private float jumpHeight = 5;
    [SerializeField] private List<ContactPoint2D> contacts = new List<ContactPoint2D>();
    [SerializeField] private int raycastDistance = 2;
    [Space]
    [Space]
    //jump
    [SerializeField] private float ApexTimeJump = 2;
    [SerializeField] private float ApexHeightJump = 2 ;


    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        colider = GetComponent<BoxCollider2D>();    
    }

    // Update is called once per frame
    void Update()
    {

        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.
        //Debug.Log(Input.GetAxis("Horizontal"));
        Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"),0);
        MovementUpdate(playerInput);       
    }

    private void MovementUpdate(Vector2 playerInput)
    {

        float gravity = -2 * ApexHeightJump / (ApexTimeJump * ApexTimeJump);


        rb.AddForce(playerInput * speed);
        float velocityX = rb.velocity.x;



        if (IsGrounded() && Input.GetKey(KeyCode.Space) )
        {
            rb.gravityScale = 0;
            //rb.AddForce(new Vector2(0, jumpHeight),ForceMode2D.Impulse); old jump
            //new jump bellow

            float jumpVelocity = 2 * ApexHeightJump / ApexTimeJump;
            rb.velocity = new Vector2(velocityX, gravity);

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
            return false;
        
    }
    public bool IsGrounded()
    {
        
        RaycastHit hit;
        Debug.DrawLine(transform.position, transform.position + Vector3.down * raycastDistance, Color.white);
        Physics.Raycast(transform.position, transform.position + Vector3.down, out hit, raycastDistance, layer);
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
