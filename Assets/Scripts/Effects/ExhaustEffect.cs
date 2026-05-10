using UnityEngine;

[CreateAssetMenu(menuName = "Lab Rats/Effects/Exhaust Effect")]
public class ExhaustEffect : PlayerEffect
{
    [Header("Hidden Stamina")]
    [SerializeField] private float minStaminaMoveMultiplier = 0.12f;
    [SerializeField] private float minStaminaAccelerationMultiplier = 0.2f;
    [SerializeField] private float minStaminaDecelerationMultiplier = 0.25f;
    [SerializeField] private float minStaminaRotationMultiplier = 0.35f;
    [SerializeField] private float staminaDrainPerSecondWhileMoving = 0.6f;
    [SerializeField] private float staminaRecoveryPerSecondWhileStill = 1.0f;
    [SerializeField] private float movingSpeedThreshold = 0.15f;

    private float staminaNormalized = 1f;

    public override void OnApply(PlayerController player)
    {
        ResetStamina(player);
    }

    public override void ModifyStats(PlayerStats stats)
    {
        if (stats == null)
            return;

        float staminaMoveScale = Mathf.Lerp(minStaminaMoveMultiplier, 1f, staminaNormalized);
        float staminaAccelerationScale = Mathf.Lerp(minStaminaAccelerationMultiplier, 1f, staminaNormalized);
        float staminaDecelerationScale = Mathf.Lerp(minStaminaDecelerationMultiplier, 1f, staminaNormalized);
        float staminaRotationScale = Mathf.Lerp(minStaminaRotationMultiplier, 1f, staminaNormalized);

        stats.moveSpeed *= staminaMoveScale;
        stats.acceleration *= staminaAccelerationScale;
        stats.deceleration *= staminaDecelerationScale;
        stats.rotationSpeed *= staminaRotationScale;
    }

    public override void OnRoundStart(PlayerController player)
    {
        ResetStamina(player);
    }

    public override void Tick(PlayerController player, float deltaTime)
    {
        if (player == null)
            return;

        float previousStamina = staminaNormalized;
        float horizontalSpeed = player.GetHorizontalSpeed();
        bool isMoving = horizontalSpeed > movingSpeedThreshold;

        if (isMoving)
            staminaNormalized -= staminaDrainPerSecondWhileMoving * deltaTime;
        else
            staminaNormalized += staminaRecoveryPerSecondWhileStill * deltaTime;

        staminaNormalized = Mathf.Clamp01(staminaNormalized);

        if (Mathf.Abs(staminaNormalized - previousStamina) > 0.0001f)
            player.RecalculateStats();
    }

    public override void OnRemove(PlayerController player)
    {
        staminaNormalized = 1f;

        if (player != null)
            player.RecalculateStats();
    }

    private void ResetStamina(PlayerController player)
    {
        staminaNormalized = 1f;

        if (player != null)
            player.RecalculateStats();
    }
}
