using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpImpulse = 10f;

    [Header("Better Jump Feel")]
    public float fallMultiplier = 2.5f;      // rychlejší pád
    public float lowJumpMultiplier = 2.0f;   // když pustíš skok brzy, začne “víc padat”
    public float jumpCutMultiplier = 0.5f;   // okamžité zkrácení skoku při puštění (0.4–0.7)

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("Auto-hop")]
    public bool holdToAutoHop = true;

    Rigidbody2D rb;

    float moveX;

    bool jumpHeld;
    bool jumpPressedThisFrame;
    bool jumpReleasedThisFrame;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // --- MOVE INPUT ---
        moveX = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
        }
        if (Gamepad.current != null)
        {
            float stick = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stick) > 0.1f) moveX = Mathf.Clamp(stick, -1f, 1f);
        }

        // --- JUMP INPUT ---
        jumpPressedThisFrame = false;
        jumpReleasedThisFrame = false;

        bool heldNow = false;

        if (Keyboard.current != null)
        {
            heldNow |= Keyboard.current.spaceKey.isPressed;
            if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressedThisFrame = true;
            if (Keyboard.current.spaceKey.wasReleasedThisFrame) jumpReleasedThisFrame = true;
        }

        if (Gamepad.current != null)
        {
            heldNow |= Gamepad.current.buttonSouth.isPressed;
            if (Gamepad.current.buttonSouth.wasPressedThisFrame) jumpPressedThisFrame = true;
            if (Gamepad.current.buttonSouth.wasReleasedThisFrame) jumpReleasedThisFrame = true;
        }

        jumpHeld = heldNow;
    }

    void FixedUpdate()
    {
        // Move
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        bool grounded = IsGrounded();

        // Start jump:
        // - normálně na stisk
        // - auto-hop: když držíš a jsi na zemi, skočí (ale jen když nejsi už ve vzduchu)
        if ((jumpPressedThisFrame || (holdToAutoHop && jumpHeld)) && grounded)
        {
            StartJump();
        }

        // Variable jump height (plynule):
        // Když PUSTÍŠ skok během stoupání, okamžitě zkrať vzestup.
        if (jumpReleasedThisFrame && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // Better jump feel:
        // - když padáš, přidej gravitaci
        // - když stoupáš, ale nedržíš skok, přidej gravitaci (nižší skok)
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    void StartJump()
    {
        // konzistentní skok
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
