using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public enum ControlPreset
    {
        PlayerOneWASD,
        PlayerTwoArrows,
        Custom
    }

    [Header("Player")]
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private ControlPreset controlPreset = ControlPreset.PlayerOneWASD;

    [Header("Custom Keys")]
    [SerializeField] private KeyCode moveUp = KeyCode.W;
    [SerializeField] private KeyCode moveDown = KeyCode.S;
    [SerializeField] private KeyCode moveLeft = KeyCode.A;
    [SerializeField] private KeyCode moveRight = KeyCode.D;
    [SerializeField] private KeyCode jump = KeyCode.Space;

    public int PlayerIndex => playerIndex;
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }

    private void Awake()
    {
        ApplyPreset();
    }

    private void Update()
    {
        ReadMovement();
        ReadJump();
    }

    private void ApplyPreset()
    {
        if (controlPreset == ControlPreset.PlayerOneWASD)
        {
            playerIndex = 0;

            moveUp = KeyCode.W;
            moveDown = KeyCode.S;
            moveLeft = KeyCode.A;
            moveRight = KeyCode.D;
            jump = KeyCode.Space;
        }
        else if (controlPreset == ControlPreset.PlayerTwoArrows)
        {
            playerIndex = 1;

            moveUp = KeyCode.UpArrow;
            moveDown = KeyCode.DownArrow;
            moveLeft = KeyCode.LeftArrow;
            moveRight = KeyCode.RightArrow;
            jump = KeyCode.RightShift;
        }
    }

    private void ReadMovement()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(moveLeft))
            x -= 1f;

        if (Input.GetKey(moveRight))
            x += 1f;

        if (Input.GetKey(moveDown))
            y -= 1f;

        if (Input.GetKey(moveUp))
            y += 1f;

        MoveInput = new Vector2(x, y);

        if (MoveInput.sqrMagnitude > 1f)
            MoveInput = MoveInput.normalized;
    }

    private void ReadJump()
    {
        if (Input.GetKeyDown(jump))
            JumpPressed = true;
    }

    public void ConsumeJump()
    {
        JumpPressed = false;
    }

    public void ResetInput()
    {
        MoveInput = Vector2.zero;
        JumpPressed = false;
    }
}