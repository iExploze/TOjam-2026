using UnityEngine;

[CreateAssetMenu(menuName = "Lab Rats/Effects/Exhaust Effect")]
public class ExhaustEffect : PlayerEffect
{
    [Header("Stat Multipliers")]
    [SerializeField] private float moveSpeedMultiplier = 0.75f;
    [SerializeField] private float accelerationMultiplier = 0.65f;
    [SerializeField] private float decelerationMultiplier = 0.7f;
    [SerializeField] private float airControlMultiplier = 0.65f;
    [SerializeField] private float rotationSpeedMultiplier = 0.85f;

    [Header("Cough Timing")]
    [SerializeField] private float firstCoughDelay = 0.75f;
    [SerializeField] private float coughInterval = 1.25f;
    [SerializeField] private float coughStunDuration = 0.12f;

    [Header("Tiny Push")]
    [SerializeField] private bool applyTinyPush = true;
    [SerializeField] private float pushForce = 1.8f;
    [SerializeField] private float upwardForce = 0.2f;

    private float coughTimer;

    public override void OnApply(PlayerController player)
    {
        ResetCoughTimer();
    }

    public override void ModifyStats(PlayerStats stats)
    {
        if (stats == null)
            return;

        stats.moveSpeed *= moveSpeedMultiplier;
        stats.acceleration *= accelerationMultiplier;
        stats.deceleration *= decelerationMultiplier;
        stats.airControl *= airControlMultiplier;
        stats.rotationSpeed *= rotationSpeedMultiplier;
    }

    public override void OnRoundStart(PlayerController player)
    {
        ResetCoughTimer();
    }

    public override void Tick(PlayerController player, float deltaTime)
    {
        if (player == null)
            return;

        coughTimer -= deltaTime;

        if (coughTimer > 0f)
            return;

        if (coughStunDuration > 0f)
            player.Stun(coughStunDuration);

        if (applyTinyPush)
        {
            Vector3 backwardDirection = -player.GetLastMoveDirection();
            backwardDirection.y = 0f;

            if (backwardDirection.sqrMagnitude < 0.0001f)
            {
                backwardDirection = -player.transform.forward;
                backwardDirection.y = 0f;
            }

            if (backwardDirection.sqrMagnitude > 0.0001f)
                backwardDirection.Normalize();

            Vector3 push = (backwardDirection * pushForce) + (Vector3.up * upwardForce);
            player.ApplyExternalForce(push, ForceMode.Impulse);
        }

        coughTimer = Mathf.Max(0.01f, coughInterval);
    }

    public override void OnRemove(PlayerController player)
    {
        coughTimer = 0f;
    }

    private void ResetCoughTimer()
    {
        coughTimer = Mathf.Max(0f, firstCoughDelay);
    }
}
