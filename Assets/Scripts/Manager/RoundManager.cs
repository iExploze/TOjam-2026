using System.Collections;
using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Players")]
    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;

    [Header("Spawns")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("Match Settings")]
    [SerializeField] private int pointsToWin = 10;
    [SerializeField] private int deathRespawnSeconds = 2;
    [SerializeField] private float roundOverScreenTime = 3f;
    [SerializeField] private float matchEndPause = 3f;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private RoundOverOverlayUI roundOverOverlayUI;
    [SerializeField] private PlayerDeathOverlayUI deathOverlayUI;

    private int p1Score;
    private int p2Score;

    private bool roundLocked;
    private bool matchOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateScoreUI();
        ClearTemporaryUI();

        StartRound();
    }

    public void PlayerFinished(PlayerController finishingPlayer)
    {
        if (finishingPlayer == null)
            return;

        if (roundLocked || matchOver)
            return;

        StartCoroutine(RoundWinRoutine(finishingPlayer));
    }

    public void PlayerDied(PlayerController deadPlayer)
    {
        if (deadPlayer == null)
            return;

        if (matchOver || roundLocked)
            return;

        if (deadPlayer.State != PlayerState.Normal)
            return;

        StartCoroutine(PlayerDeathRoutine(deadPlayer));
    }

    private IEnumerator RoundWinRoutine(PlayerController winner)
    {
        roundLocked = true;

        PlayerController loser = GetOtherPlayer(winner);

        winner.FinishRound();

        if (loser != null)
            loser.FinishRound();

        if (winner.PlayerId == PlayerId.Player1)
            p1Score++;
        else
            p2Score++;

        UpdateScoreUI();

        if (p1Score >= pointsToWin || p2Score >= pointsToWin)
        {
            matchOver = true;

            if (roundOverOverlayUI != null)
                roundOverOverlayUI.ShowMatchOver(winner.PlayerId);

            SetMessage($"{GetPlayerName(winner)} wins the experiment!");

            yield return new WaitForSeconds(matchEndPause);

            ResetMatch();
            yield break;
        }

        if (roundOverOverlayUI != null)
            roundOverOverlayUI.ShowRoundOver(winner.PlayerId);

        SetMessage($"{GetPlayerName(winner)} wins the round!");

        yield return new WaitForSeconds(roundOverScreenTime);

        if (roundOverOverlayUI != null)
            roundOverOverlayUI.Hide();

        ResetBothPlayersForRound();

        SetMessage("");

        roundLocked = false;
    }

    private IEnumerator PlayerDeathRoutine(PlayerController deadPlayer)
    {
        deadPlayer.SetState(PlayerState.Disabled);

        for (int i = deathRespawnSeconds; i > 0; i--)
        {
            if (deathOverlayUI != null)
                deathOverlayUI.ShowRespawn(deadPlayer.PlayerId, i);

            yield return new WaitForSeconds(1f);
        }

        Transform spawn = GetSpawn(deadPlayer);

        if (spawn != null)
            deadPlayer.RespawnAt(spawn.position, spawn.rotation);

        if (deathOverlayUI != null)
            deathOverlayUI.HideRespawn(deadPlayer.PlayerId);
    }

    private void StartRound()
    {
        roundLocked = false;
        matchOver = false;

        ResetBothPlayersForRound();
        ClearTemporaryUI();
    }

    private void ResetBothPlayersForRound()
    {
        if (deathOverlayUI != null)
            deathOverlayUI.HideAll();

        if (player1 != null && player1Spawn != null)
            player1.ResetForRound(player1Spawn.position, player1Spawn.rotation);

        if (player2 != null && player2Spawn != null)
            player2.ResetForRound(player2Spawn.position, player2Spawn.rotation);
    }

    private void ResetMatch()
    {
        p1Score = 0;
        p2Score = 0;

        matchOver = false;
        roundLocked = false;

        UpdateScoreUI();
        ClearTemporaryUI();
        ResetBothPlayersForRound();
    }

    private PlayerController GetOtherPlayer(PlayerController player)
    {
        if (player == player1)
            return player2;

        if (player == player2)
            return player1;

        return null;
    }

    private Transform GetSpawn(PlayerController player)
    {
        if (player == player1)
            return player1Spawn;

        if (player == player2)
            return player2Spawn;

        return null;
    }

    private string GetPlayerName(PlayerController player)
    {
        if (player == null)
            return "Unknown Player";

        return player.PlayerId == PlayerId.Player1 ? "Player 1" : "Player 2";
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"P1: {p1Score}     P2: {p2Score}";
    }

    private void SetMessage(string text)
    {
        if (messageText != null)
            messageText.text = text;
    }

    private void ClearTemporaryUI()
    {
        SetMessage("");

        if (roundOverOverlayUI != null)
            roundOverOverlayUI.Hide();

        if (deathOverlayUI != null)
            deathOverlayUI.HideAll();
    }
}