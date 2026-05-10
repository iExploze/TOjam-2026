using TMPro;
using UnityEngine;

public class RoundOverOverlayUI : MonoBehaviour
{
    [SerializeField] private GameObject roundOverOverlay;
    [SerializeField] private TMP_Text winningText;

    private void Awake()
    {
        Hide();
    }

    public void ShowRoundOver(PlayerId winnerId)
    {
        if (roundOverOverlay != null)
            roundOverOverlay.SetActive(true);

        string winnerName = winnerId == PlayerId.Player1 ? "Player 1" : "Player 2";

        if (winningText != null)
            winningText.text = $"{winnerName}'s rat won!";
    }

    public void ShowMatchOver(PlayerId winnerId)
    {
        if (roundOverOverlay != null)
            roundOverOverlay.SetActive(true);

        string winnerName = winnerId == PlayerId.Player1 ? "Player 1" : "Player 2";

        if (winningText != null)
            winningText.text = $"{winnerName} won the experiment!";
    }

    public void Hide()
    {
        if (roundOverOverlay != null)
            roundOverOverlay.SetActive(false);
    }
}