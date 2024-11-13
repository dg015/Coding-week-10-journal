using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D colider;
    [SerializeField] private float speed;
    [SerializeField] private FacingDirection currentDirection;
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
        Debug.Log(Input.GetAxis("Horizontal"));
        Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"),0);
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        rb.AddForce(playerInput * speed);
    }

    public bool IsWalking()
    {
        return false;
    }
    public bool IsGrounded()
    {
        return false;
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
