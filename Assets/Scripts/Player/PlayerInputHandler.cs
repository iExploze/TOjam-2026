using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private Key moveUp = Key.W;
    [SerializeField] private Key moveDown = Key.S;
    [SerializeField] private Key moveLeft = Key.A;
    [SerializeField] private Key moveRight = Key.D;
    [SerializeField] private Key jump = Key.Space;

    public int PlayerIndex => playerIndex;
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }

    private Keyboard keyboard;

    private void Awake()
    {
        ApplyPreset();
    }

    private void OnEnable()
    {
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        keyboard ??= Keyboard.current;

        if (keyboard == null)
        {
            MoveInput = Vector2.zero;
            JumpPressed = false;
            return;
        }

        ReadMovement();
        ReadJump();
    }

    private void ApplyPreset()
    {
        if (controlPreset == ControlPreset.PlayerOneWASD)
        {
            playerIndex = 0;

            moveUp = Key.W;
            moveDown = Key.S;
            moveLeft = Key.A;
            moveRight = Key.D;
            jump = Key.Space;
        }
        else if (controlPreset == ControlPreset.PlayerTwoArrows)
        {
            playerIndex = 1;

            moveUp = Key.UpArrow;
            moveDown = Key.DownArrow;
            moveLeft = Key.LeftArrow;
            moveRight = Key.RightArrow;
            jump = Key.RightCtrl;
        }
    }

    private void ReadMovement()
    {
        float x = 0f;
        float y = 0f;

        if (IsPressed(moveLeft))
            x -= 1f;

        if (IsPressed(moveRight))
            x += 1f;

        if (IsPressed(moveDown))
            y -= 1f;

        if (IsPressed(moveUp))
            y += 1f;

        MoveInput = new Vector2(x, y);

        if (MoveInput.sqrMagnitude > 1f)
            MoveInput = MoveInput.normalized;
    }

    private void ReadJump()
    {
        if (WasPressedThisFrame(jump))
            JumpPressed = true;
    }

    private bool IsPressed(Key key)
    {
        if (key == Key.None || keyboard == null)
            return false;

        return keyboard[key].isPressed;
    }

    private bool WasPressedThisFrame(Key key)
    {
        if (key == Key.None || keyboard == null)
            return false;

        return keyboard[key].wasPressedThisFrame;
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ApplyPreset();
        }
    }
#endif
}