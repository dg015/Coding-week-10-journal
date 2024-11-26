using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;

    private readonly int isWalkingHash = Animator.StringToHash("Walking");
    private readonly int isIdleHash = Animator.StringToHash("Idle");
    private readonly int isDeadHash = Animator.StringToHash("Dead");
    private readonly int isGroundedHash = Animator.StringToHash("Grounded");

    void Update()
    {
        animator.SetBool(isWalkingHash, playerController.IsWalking());
        animator.SetBool(isGroundedHash, playerController.IsGrounded());

        switch (playerController.GetFacingDirection())
        {
            case PlayerController.FacingDirection.left:
                bodyRenderer.flipX = true;
                break;
            case PlayerController.FacingDirection.right:
                bodyRenderer.flipX = false;
                break;
        }
    }

    private void UpdateVisuals()
    {
        if (playerController.previousState != playerController.currentState)
        {
            switch (playerController.currentState)
            {
                case PlayerController.PlayerState.idle:
                    animator.CrossFade();
                    break;
                case PlayerController.PlayerState.walking:
                    break;
                case PlayerController.PlayerState.jumping:
                    break;
                case PlayerController.PlayerState.dead:
                    break;
                default:
                    break;
            }

        }
    }
}


