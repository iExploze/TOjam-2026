using UnityEngine;

public enum PlayerState
{
    Normal,
    Stunned,
    Finished,
    Disabled
}

[System.Serializable]
public class PlayerStats
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 45f;
    public float deceleration = 55f;
    public float airControl = 0.55f;

    [Header("Jumping")]
    public float jumpForce = 8f;
    public float extraGravity = 18f;

    [Header("Rotation")]
    public float rotationSpeed = 14f;

    public PlayerStats Clone()
    {
        return new PlayerStats
        {
            moveSpeed = moveSpeed,
            acceleration = acceleration,
            deceleration = deceleration,
            airControl = airControl,
            jumpForce = jumpForce,
            extraGravity = extraGravity,
            rotationSpeed = rotationSpeed
        };
    }
}

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerEffectController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerEffectController effects;

    [Header("Child References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform groundCheck;

    [Header("Stats")]
    [SerializeField] private PlayerStats baseStats = new PlayerStats();

    public PlayerStats CurrentStats { get; private set; }
    public PlayerState State { get; private set; } = PlayerState.Normal;

    public int PlayerIndex => input.PlayerIndex;
    public Transform CameraTarget => cameraTarget;
    public Transform VisualRoot => visualRoot;
    public Transform GroundCheck => groundCheck;

    public PlayerId PlayerId => input.PlayerIndex == 0 ? PlayerId.Player1 : PlayerId.Player2;
    public PlayerEffectController Effects => effects;

    private void Awake()
    {
        input ??= GetComponent<PlayerInputHandler>();
        motor ??= GetComponent<PlayerMotor>();
        effects ??= GetComponent<PlayerEffectController>();

        if (visualRoot == null)
            visualRoot = transform.Find("VisualRoot");

        if (cameraTarget == null)
            cameraTarget = transform.Find("CameraTarget");

        if (groundCheck == null)
            groundCheck = transform.Find("GroundCheck");

        motor.Initialize(this, groundCheck);
        effects.Initialize(this);

        RecalculateStats();
        SetState(PlayerState.Normal);
    }

    private void Update()
    {
        if (State != PlayerState.Normal)
            return;

        if (input.JumpPressed)
        {
            motor.TryJump(CurrentStats);
            input.ConsumeJump();
        }
    }

    private void FixedUpdate()
    {
        if (State != PlayerState.Normal)
            return;

        effects.TickEffects(Time.fixedDeltaTime);

        if (State != PlayerState.Normal)
            return;

        motor.Move(input.MoveInput, CurrentStats);
        motor.ApplyExtraGravity(CurrentStats);
    }

    public void RecalculateStats()
    {
        CurrentStats = baseStats.Clone();
        effects.ModifyStats(CurrentStats);
    }

    public void ApplyExternalForce(Vector3 force, ForceMode forceMode = ForceMode.Impulse)
    {
        if (motor != null)
            motor.AddExternalForce(force, forceMode);
    }

    public Vector3 GetLastMoveDirection()
    {
        if (motor != null && motor.LastMoveDirection.sqrMagnitude > 0.01f)
            return motor.LastMoveDirection.normalized;

        return transform.forward;
    }

    public void SetState(PlayerState newState)
    {
        State = newState;

        bool canMove = State == PlayerState.Normal;

        if (input != null)
            input.SetInputEnabled(canMove);

        if (!canMove && motor != null)
            motor.StopHorizontalMovement();
    }

    public void ResetForRound(Vector3 position, Quaternion rotation)
    {
        if (motor != null)
            motor.TeleportTo(position, rotation);
        else
            transform.SetPositionAndRotation(position, rotation);

        if (input != null)
            input.ResetInput();

        RecalculateStats();
        SetState(PlayerState.Normal);
    }

    public void FinishRound()
    {
        effects.NotifyRoundEnd();

        SetState(PlayerState.Finished);

        motor.ResetVelocity();
        input.ResetInput();
    }

    public void RespawnAt(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (motor != null)
            motor.TeleportTo(spawnPosition, spawnRotation);
        else
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        if (input != null)
            input.ResetInput();

        RecalculateStats();
        SetState(PlayerState.Normal);
    }

    public void Stun(float duration)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(StunRoutine(duration));
    }

    private System.Collections.IEnumerator StunRoutine(float duration)
    {
        SetState(PlayerState.Stunned);

        yield return new WaitForSeconds(duration);

        if (State == PlayerState.Stunned)
            SetState(PlayerState.Normal);
    }
}
