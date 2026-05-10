using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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
    [SerializeField] private int roundStartCountdownSeconds = 3;
    [SerializeField] private float goScreenTime = 0.5f;

    [Header("UI")]
    [SerializeField] private RoundOverOverlayUI roundOverOverlayUI;
    [SerializeField] private PlayerDeathOverlayUI deathOverlayUI;
    [SerializeField] private CardOverlayUI cardOverlayUI;
    [SerializeField] private CountdownOverlayUI countdownOverlayUI;
    [SerializeField] private ScoreMeterUI scoreMeterUI;

    [Header("Effect Cards")]
    [SerializeField] private List<EffectCardDefinition> hazardPool = new List<EffectCardDefinition>();

    [Header("Debug Card Selection")]
    [SerializeField] private bool debugForceCardInChoices;
    [SerializeField] private int debugForcedHazardPoolIndex;

    private bool choosingCard;
    private readonly List<EffectCardDefinition> currentChoices = new List<EffectCardDefinition>();
    private int selectedChoiceIndex;
    private EffectCardDefinition selectedCard;
    private PlayerId cardChooserId;

    private int p1Score;
    private int p2Score;

    private bool roundLocked;
    private bool matchOver;

    private Vector3 player1RespawnPosition;
    private Quaternion player1RespawnRotation;
    private bool player1HasRespawnPoint;

    private Vector3 player2RespawnPosition;
    private Quaternion player2RespawnRotation;
    private bool player2HasRespawnPoint;

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
        UpdateScoreMeter();
        ClearTemporaryUI();

        StartRound();
    }

    private void Update()
    {
        if (choosingCard)
            HandleCardSelectionInput();
    }

    private void UpdateScoreMeter()
    {
        if (scoreMeterUI != null)
            scoreMeterUI.SetScore(p1Score, p2Score);
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

    public void SetCheckpoint(PlayerController player, Vector3 position, Quaternion rotation)
    {
        if (player == null)
            return;

        if (player == player1)
        {
            player1RespawnPosition = position;
            player1RespawnRotation = rotation;
            player1HasRespawnPoint = true;
            return;
        }

        if (player == player2)
        {
            player2RespawnPosition = position;
            player2RespawnRotation = rotation;
            player2HasRespawnPoint = true;
        }
    }

    private void DisablePlayerMovement()
    {
        if (player1 != null)
            player1.FinishRound();

        if (player2 != null)
            player2.FinishRound();
    }

    private void EnablePlayerMovement()
    {
        if (player1 != null)
        {
            player1.Effects.NotifyRoundStart();
            player1.SetState(PlayerState.Normal);
        }

        if (player2 != null)
        {
            player2.Effects.NotifyRoundStart();
            player2.SetState(PlayerState.Normal);
        }
    }

    private IEnumerator RoundWinRoutine(PlayerController winner)
    {
        roundLocked = true;

        PlayerController loser = GetOtherPlayer(winner);

        DisablePlayerMovement();

        if (winner.PlayerId == PlayerId.Player1)
            p1Score++;
        else
            p2Score++;

        UpdateScoreMeter();

        if (p1Score >= pointsToWin || p2Score >= pointsToWin)
        {
            matchOver = true;
            roundLocked = true;
            choosingCard = false;

            DisablePlayerMovement();

            if (cardOverlayUI != null)
                cardOverlayUI.Hide();

            if (countdownOverlayUI != null)
                countdownOverlayUI.Hide();

            if (deathOverlayUI != null)
                deathOverlayUI.HideAll();

            if (roundOverOverlayUI != null)
                roundOverOverlayUI.ShowMatchOver(winner.PlayerId);

            Time.timeScale = 0f;

            yield break;
        }

        // 1. Winner screen
        if (scoreMeterUI != null)
            scoreMeterUI.Hide();

        if (roundOverOverlayUI != null)
            roundOverOverlayUI.ShowRoundOver(winner.PlayerId);

        yield return new WaitForSeconds(roundOverScreenTime);

        if (roundOverOverlayUI != null)
            roundOverOverlayUI.Hide();

        // 2. Card selection screen
        yield return CardChoiceRoutine(loser, winner);

        if (cardOverlayUI != null)
            cardOverlayUI.Hide();

        // 3. Reset players, freeze them, countdown, then unlock movement
        yield return StartNextRoundWithCountdownRoutine(winner);
    }

    private IEnumerator StartNextRoundWithCountdownRoutine(PlayerController cursedWinner)
    {
        if (roundOverOverlayUI != null)
            roundOverOverlayUI.Hide();

        if (cardOverlayUI != null)
            cardOverlayUI.Hide();

        if (countdownOverlayUI != null)
            countdownOverlayUI.Hide();

        ClearAllPlayerEffects();

        // Reset positions.
        ResetBothPlayersForRound();

        // Wait one physics step so Rigidbody teleport actually settles.
        yield return new WaitForFixedUpdate();

        // Freeze players during countdown.
        DisablePlayerMovement();

        // Apply selected curse to the previous winner.
        if (selectedCard != null && selectedCard.effectAsset != null && cursedWinner != null)
            cursedWinner.Effects.AddEffect(selectedCard.effectAsset);

        for (int i = roundStartCountdownSeconds; i > 0; i--)
        {
            if (countdownOverlayUI != null)
                countdownOverlayUI.ShowNumber(i);

            yield return new WaitForSeconds(1f);
        }

        if (countdownOverlayUI != null)
            countdownOverlayUI.ShowGo();

        yield return new WaitForSeconds(goScreenTime);

        if (countdownOverlayUI != null)
            countdownOverlayUI.Hide();

        EnablePlayerMovement();

        if (scoreMeterUI != null)
            scoreMeterUI.Show();

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

        if (TryGetRespawnPoint(deadPlayer, out Vector3 respawnPosition, out Quaternion respawnRotation))
            deadPlayer.RespawnAt(respawnPosition, respawnRotation);

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

        ResetRespawnPointsToSpawns();

        if (player1 != null && player1HasRespawnPoint)
            player1.ResetForRound(player1RespawnPosition, player1RespawnRotation);

        if (player2 != null && player2HasRespawnPoint)
            player2.ResetForRound(player2RespawnPosition, player2RespawnRotation);
    }

    private void ResetMatch()
    {
        p1Score = 0;
        p2Score = 0;

        UpdateScoreMeter();

        matchOver = false;
        roundLocked = false;

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

    private void ResetRespawnPointsToSpawns()
    {
        if (player1Spawn != null)
        {
            player1RespawnPosition = player1Spawn.position;
            player1RespawnRotation = player1Spawn.rotation;
            player1HasRespawnPoint = true;
        }
        else
        {
            player1HasRespawnPoint = false;
        }

        if (player2Spawn != null)
        {
            player2RespawnPosition = player2Spawn.position;
            player2RespawnRotation = player2Spawn.rotation;
            player2HasRespawnPoint = true;
        }
        else
        {
            player2HasRespawnPoint = false;
        }
    }

    private bool TryGetRespawnPoint(PlayerController player, out Vector3 position, out Quaternion rotation)
    {
        if (player == player1 && player1HasRespawnPoint)
        {
            position = player1RespawnPosition;
            rotation = player1RespawnRotation;
            return true;
        }

        if (player == player2 && player2HasRespawnPoint)
        {
            position = player2RespawnPosition;
            rotation = player2RespawnRotation;
            return true;
        }

        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }

    private string GetPlayerName(PlayerController player)
    {
        if (player == null)
            return "Unknown Player";

        return player.PlayerId == PlayerId.Player1 ? "Player 1" : "Player 2";
    }

    private IEnumerator CardChoiceRoutine(PlayerController chooser, PlayerController target)
    {
        PickRandomCardChoices();

        selectedChoiceIndex = 0;
        selectedCard = currentChoices.Count > 0 ? currentChoices[0] : null;

        cardChooserId = chooser.PlayerId;
        choosingCard = true;

        if (cardOverlayUI != null)
            cardOverlayUI.Show(chooser.PlayerId, currentChoices, selectedChoiceIndex);

        while (choosingCard)
            yield return null;
    }

    private void HandleCardSelectionInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        bool leftPressed = false;
        bool rightPressed = false;
        bool confirmPressed = false;

        if (cardChooserId == PlayerId.Player1)
        {
            // Player 1 card controls
            leftPressed = keyboard.aKey.wasPressedThisFrame;
            rightPressed = keyboard.dKey.wasPressedThisFrame;
            confirmPressed = keyboard.spaceKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame;
        }
        else if (cardChooserId == PlayerId.Player2)
        {
            // Player 2 card controls
            leftPressed = keyboard.leftArrowKey.wasPressedThisFrame;
            rightPressed = keyboard.rightArrowKey.wasPressedThisFrame;
            confirmPressed = keyboard.rightCtrlKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame;
        }

        if (currentChoices.Count == 0)
        {
            if (confirmPressed)
                choosingCard = false;

            return;
        }

        if (leftPressed)
        {
            selectedChoiceIndex--;

            if (selectedChoiceIndex < 0)
                selectedChoiceIndex = currentChoices.Count - 1;

            selectedCard = currentChoices[selectedChoiceIndex];

            if (cardOverlayUI != null)
                cardOverlayUI.UpdateSelection(currentChoices, selectedChoiceIndex);
        }

        if (rightPressed)
        {
            selectedChoiceIndex++;

            if (selectedChoiceIndex >= currentChoices.Count)
                selectedChoiceIndex = 0;

            selectedCard = currentChoices[selectedChoiceIndex];

            if (cardOverlayUI != null)
                cardOverlayUI.UpdateSelection(currentChoices, selectedChoiceIndex);
        }

        if (confirmPressed)
        {
            selectedCard = currentChoices[selectedChoiceIndex];
            choosingCard = false;
        }
    }

    private void PickRandomCardChoices()
    {
        currentChoices.Clear();

        List<EffectCardDefinition> available = new List<EffectCardDefinition>();

        foreach (EffectCardDefinition card in hazardPool)
        {
            if (card != null)
                available.Add(card);
        }

        if (debugForceCardInChoices && TryGetDebugForcedCardFromPool(out EffectCardDefinition debugForcedCard))
        {
            currentChoices.Add(debugForcedCard);
            available.RemoveAll(card => card == debugForcedCard);
        }

        int choicesToPick = Mathf.Min(3, currentChoices.Count + available.Count);

        while (currentChoices.Count < choicesToPick && available.Count > 0)
        {
            int randomIndex = Random.Range(0, available.Count);

            currentChoices.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }
    }

    private bool TryGetDebugForcedCardFromPool(out EffectCardDefinition forcedCard)
    {
        forcedCard = null;

        if (hazardPool == null || hazardPool.Count == 0)
            return false;

        if (debugForcedHazardPoolIndex < 0 || debugForcedHazardPoolIndex >= hazardPool.Count)
            return false;

        forcedCard = hazardPool[debugForcedHazardPoolIndex];
        return forcedCard != null;
    }

    private void ClearAllPlayerEffects()
    {
        if (player1 != null)
            player1.Effects.ClearEffects();

        if (player2 != null)
            player2.Effects.ClearEffects();
    }

    private void ClearTemporaryUI()
    {
        if (roundOverOverlayUI != null)
            roundOverOverlayUI.Hide();

        if (deathOverlayUI != null)
            deathOverlayUI.HideAll();

        if (cardOverlayUI != null)
            cardOverlayUI.Hide();

        if (countdownOverlayUI != null)
            countdownOverlayUI.Hide();

        if (scoreMeterUI != null)
            scoreMeterUI.Show();
    }
}
