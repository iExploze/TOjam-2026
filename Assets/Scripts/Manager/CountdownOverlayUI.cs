using TMPro;
using UnityEngine;

public class CountdownOverlayUI : MonoBehaviour
{
    [SerializeField] private GameObject countdownOverlay;
    [SerializeField] private TMP_Text countdownText;

    private void Awake()
    {
        Hide();
    }

    public void ShowNumber(int number)
    {
        if (countdownOverlay != null)
            countdownOverlay.SetActive(true);

        if (countdownText != null)
            countdownText.text = number.ToString();
    }

    public void ShowGo()
    {
        if (countdownOverlay != null)
            countdownOverlay.SetActive(true);

        if (countdownText != null)
            countdownText.text = "GO!!!";
    }

    public void Hide()
    {
        if (countdownOverlay != null)
            countdownOverlay.SetActive(false);
    }
}