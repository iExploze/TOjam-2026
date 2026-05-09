using UnityEngine;

public class CameraFollowProxy : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget;

    [Header("Horizontal Follow")]
    [SerializeField] private float horizontalSmoothTime = 0.08f;

    [Header("Vertical Jump Follow")]
    [SerializeField] private bool softenJumpFollow = true;

    [Tooltip("0 = ignore jump height, 1 = fully follow jump height.")]
    [Range(0f, 1f)]
    [SerializeField] private float jumpFollowAmount = 0.25f;

    [SerializeField] private float verticalSmoothTime = 0.25f;

    [Header("Base Height")]
    [SerializeField] private bool lockBaseHeightOnStart = true;
    [SerializeField] private float baseY = 0f;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private void Start()
    {
        if (playerTarget == null)
            return;

        if (lockBaseHeightOnStart)
            baseY = playerTarget.position.y;

        transform.position = GetDesiredPosition();
    }

    private void LateUpdate()
    {
        if (playerTarget == null)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 desiredPosition = GetDesiredPosition();

        Vector3 horizontalCurrent = new Vector3(
            currentPosition.x,
            0f,
            currentPosition.z
        );

        Vector3 horizontalDesired = new Vector3(
            desiredPosition.x,
            0f,
            desiredPosition.z
        );

        Vector3 smoothedHorizontal = Vector3.SmoothDamp(
            horizontalCurrent,
            horizontalDesired,
            ref horizontalVelocity,
            horizontalSmoothTime
        );

        float smoothedY = Mathf.SmoothDamp(
            currentPosition.y,
            desiredPosition.y,
            ref verticalVelocity,
            verticalSmoothTime
        );

        transform.position = new Vector3(
            smoothedHorizontal.x,
            smoothedY,
            smoothedHorizontal.z
        );
    }

    private Vector3 GetDesiredPosition()
    {
        float targetY = playerTarget.position.y;

        float softenedY = softenJumpFollow
            ? Mathf.Lerp(baseY, targetY, jumpFollowAmount)
            : targetY;

        return new Vector3(
            playerTarget.position.x,
            softenedY,
            playerTarget.position.z
        );
    }

    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;

        if (playerTarget == null)
            return;

        if (lockBaseHeightOnStart)
            baseY = playerTarget.position.y;

        transform.position = GetDesiredPosition();
    }
}