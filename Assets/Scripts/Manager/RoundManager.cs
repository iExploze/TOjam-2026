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

    [Header("UI")]
    [SerializeField] private RoundOverOverlayUI roundOverOverlayUI;
    [SerializeField] private PlayerDeathOverlayUI deathOverlayUI;
    [SerializeField] private CardOverlayUI cardOverlayUI;

    [Header("Effect Cards")]
    [SerializeField] private List<EffectCardDefinition> hazardPool = new List<EffectCardDefinition>();

    private bool choosingCard;
    private readonly List<EffectCardDefinition> currentChoices = new List<EffectCardDefinition>();
    private int selectedChoiceIndex;
    private EffectCardDefinition selectedCard;
    private PlayerId cardChooserId;

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
        ClearTemporaryUI();

        StartRound();
    }

    private void Update()
    {
        if (choosingCard)
            HandleCardSelectionInput();
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

        if (p1Score >= pointsToWin || p2Score >= pointsToWin)
        {
            matchOver = true;

            if (roundOverOverlayUI != null)
                roundOverOverlayUI.ShowMatchOver(winner.PlayerId);

            yield return new WaitForSeconds(matchEndPause);

            ResetMatch();
            yield break;
        }

        // 1. Show round over screen
        if (roundOverOverlayUI != null)
            roundOverOverlayUI.ShowRoundOver(winner.PlayerId);

        yield return new WaitForSeconds(roundOverScreenTime);

        if (roundOverOverlayUI != null)
            roundOverOverlayUI.Hide();

        // 2. Show card selection screen
        if (selectedCard != null && selectedCard.effectAsset != null)
            winner.Effects.AddEffect(selectedCard.effectAsset);

        // 3. Hide card screen after selection
        if (cardOverlayUI != null)
            cardOverlayUI.Hide();

        // 4. Apply selected card effect to the winner
        ClearAllPlayerEffects();

        if (selectedCard != null && selectedCard.effectAsset != null)
            winner.Effects.AddEffect(selectedCard.effectAsset);

        // 5. Reset and start next round
        ResetBothPlayersForRound();

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

        int choicesToPick = Mathf.Min(3, available.Count);

        for (int i = 0; i < choicesToPick; i++)
        {
            int randomIndex = Random.Range(0, available.Count);

            currentChoices.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }
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
    }
}