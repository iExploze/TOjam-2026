using UnityEngine;

[CreateAssetMenu(menuName = "Lab Rats/Effects/Stat Modifier Effect")]
public class StatModifierEffect : PlayerEffect
{
    [Header("Stat Multipliers")]
    [SerializeField] private float moveSpeedMultiplier = 1f;
    [SerializeField] private float accelerationMultiplier = 1f;
    [SerializeField] private float decelerationMultiplier = 1f;
    [SerializeField] private float airControlMultiplier = 1f;
    [SerializeField] private float jumpForceMultiplier = 1f;
    [SerializeField] private float extraGravityMultiplier = 1f;
    [SerializeField] private float rotationSpeedMultiplier = 1f;

    public override void ModifyStats(PlayerStats stats)
    {
        stats.moveSpeed *= moveSpeedMultiplier;
        stats.acceleration *= accelerationMultiplier;
        stats.deceleration *= decelerationMultiplier;
        stats.airControl *= airControlMultiplier;
        stats.jumpForce *= jumpForceMultiplier;
        stats.extraGravity *= extraGravityMultiplier;
        stats.rotationSpeed *= rotationSpeedMultiplier;
    }
}