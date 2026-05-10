using UnityEngine;

public class DeathZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        //Debug.Log(player + "died");
        RoundManager.Instance.PlayerDied(player);
    }
}