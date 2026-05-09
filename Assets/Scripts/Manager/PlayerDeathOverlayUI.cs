using TMPro;
using UnityEngine;

public class PlayerDeathOverlayUI : MonoBehaviour
{
    [Header("P1 Overlay")]
    [SerializeField] private GameObject p1DeathOverlay;
    [SerializeField] private TMP_Text p1RespawnText;

    [Header("P2 Overlay")]
    [SerializeField] private GameObject p2DeathOverlay;
    [SerializeField] private TMP_Text p2RespawnText;

    public void ShowRespawn(PlayerId playerId, int secondsLeft)
    {
        GameObject overlay = GetOverlay(playerId);
        TMP_Text text = GetText(playerId);

        if (overlay != null)
            overlay.SetActive(true);

        if (text != null)
            text.text = $"Respawning in {secondsLeft}...";
    }

    public void HideRespawn(PlayerId playerId)
    {
        GameObject overlay = GetOverlay(playerId);
        TMP_Text text = GetText(playerId);

        if (text != null)
            text.text = "";

        if (overlay != null)
            overlay.SetActive(false);
    }

    public void HideAll()
    {
        HideRespawn(PlayerId.Player1);
        HideRespawn(PlayerId.Player2);
    }

    private GameObject GetOverlay(PlayerId playerId)
    {
        return playerId == PlayerId.Player1 ? p1DeathOverlay : p2DeathOverlay;
    }

    private TMP_Text GetText(PlayerId playerId)
    {
        return playerId == PlayerId.Player1 ? p1RespawnText : p2RespawnText;
    }
}