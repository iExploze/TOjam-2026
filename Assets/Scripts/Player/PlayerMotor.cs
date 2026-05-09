using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform groundCheck;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.22f;

    public bool IsGrounded { get; private set; }
    public Vector3 LastMoveDirection { get; private set; }

    private PlayerController player;

    public void Initialize(PlayerController owner, Transform groundCheckTransform)
    {
        player = owner;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (groundCheck == null)
            groundCheck = groundCheckTransform;

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector2 input, PlayerStats stats)
    {
        CheckGrounded();

        Vector3 moveDirection = GetMoveDirection(input);

        if (moveDirection.sqrMagnitude > 0.01f)
            LastMoveDirection = moveDirection;

        Vector3 currentHorizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        Vector3 targetVelocity = moveDirection * stats.moveSpeed;

        float controlMultiplier = IsGrounded ? 1f : stats.airControl;

        float accelRate = moveDirection.sqrMagnitude > 0.01f
            ? stats.acceleration
            : stats.deceleration;

        Vector3 newHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            accelRate * controlMultiplier * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(
            newHorizontalVelocity.x,
            rb.linearVelocity.y,
            newHorizontalVelocity.z
        );

        RotateTowardMovement(moveDirection, stats);
    }

    public void TryJump(PlayerStats stats)
    {
        CheckGrounded();

        if (!IsGrounded)
            return;

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        rb.AddForce(Vector3.up * stats.jumpForce, ForceMode.Impulse);
    }

    public void ApplyExtraGravity(PlayerStats stats)
    {
        if (IsGrounded)
            return;

        rb.AddForce(Vector3.down * stats.extraGravity, ForceMode.Acceleration);
    }

    public void AddExternalForce(Vector3 force, ForceMode forceMode = ForceMode.Impulse)
    {
        rb.AddForce(force, forceMode);
    }

    public void ResetVelocity()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void StopHorizontalMovement()
    {
        rb.linearVelocity = new Vector3(
            0f,
            rb.linearVelocity.y,
            0f
        );
    }

    private Vector3 GetMoveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
            return Vector3.zero;


            return new Vector3(input.x, 0f, input.y).normalized;
        
    }

    private void RotateTowardMovement(Vector3 moveDirection, PlayerStats stats)
    {
        if (moveDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            stats.rotationSpeed * Time.fixedDeltaTime
        );
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            IsGrounded = false;
            //Debug.LogWarning($"{name}: GroundCheck is missing.");
            return;
        }

        IsGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        //Debug.Log($"{name} grounded: {IsGrounded}");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}