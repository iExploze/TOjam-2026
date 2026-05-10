using TMPro;
using UnityEngine;

public class ScoreMeterUI : MonoBehaviour
{
    [SerializeField] private GameObject scoreMeterRoot;
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        Show();
        SetScore(0, 0);
    }

    public void SetScore(int p1Score, int p2Score)
    {
        if (scoreText != null)
            scoreText.text = $"{p1Score}:{p2Score}";

        scoreText.transform.localScale = Vector3.one * 1.15f;
        CancelInvoke(nameof(ResetScale));
        Invoke(nameof(ResetScale), 0.12f);
    }

    public void Show()
    {
        if (scoreMeterRoot != null)
            scoreMeterRoot.SetActive(true);
    }

    public void Hide()
    {
        if (scoreMeterRoot != null)
            scoreMeterRoot.SetActive(false);
    }

    private void ResetScale()
    {
        if (scoreText != null)
            scoreText.transform.localScale = Vector3.one;
    }
}