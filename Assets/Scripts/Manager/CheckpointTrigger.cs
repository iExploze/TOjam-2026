using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Checkpoint")]
    [SerializeField] private Transform respawnPointOverride;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        Transform respawnPoint = respawnPointOverride != null ? respawnPointOverride : transform;

        if (RoundManager.Instance != null)
            RoundManager.Instance.SetCheckpoint(player, respawnPoint.position, respawnPoint.rotation);
    }
}
