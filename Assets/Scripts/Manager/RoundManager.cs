using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class EffectCardDefinition
{
    public EffectType effectType;
    public PlayerEffect effectAsset;
}

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
    [SerializeField] private int roundCountdownSeconds = 3;
    [SerializeField] private int deathRespawnSeconds = 2;
    [SerializeField] private float postRoundPause = 0.75f;
    [SerializeField] private float matchEndPause = 3f;

    [Header("Effect Cards")]
    [SerializeField] private List<EffectCardDefinition> hazardPool = new List<EffectCardDefinition>();

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text effectChoiceText;

    private int p1Score;
    private int p2Score;

    private bool roundLocked;
    private bool matchOver;
    private bool choosingEffect;

    private readonly List<EffectCardDefinition> currentChoices = new List<EffectCardDefinition>();

    private int selectedChoiceIndex;
    private EffectCardDefinition selectedEffectCard;

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

        StartCoroutine(StartRoundRoutine());
    }

    private void Update()
    {
        if (choosingEffect)
            HandleEffectSelectionInput();
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

        if (roundLocked || matchOver)
            return;

        StartCoroutine(PlayerDeathRoutine(deadPlayer));
        Debug.Log("triggered respawn for " + deadPlayer);
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

        SetMessage($"{GetPlayerName(winner)} wins the round!");

        yield return new WaitForSeconds(postRoundPause);

        if (p1Score >= pointsToWin || p2Score >= pointsToWin)
        {
            matchOver = true;

            SetMessage($"{GetPlayerName(winner)} wins the experiment!");
            SetEffectChoice("");

            yield return new WaitForSeconds(matchEndPause);

            ResetMatch();
            yield break;
        }

        yield return EffectChoiceRoutine(loser, winner);

        PlayerEffect chosenEffect = selectedEffectCard != null
            ? selectedEffectCard.effectAsset
            : null;

        string chosenEffectName = chosenEffect != null
            ? chosenEffect.effectName
            : "No Effect";

        SetMessage($"{GetPlayerName(loser)} chose {chosenEffectName} for {GetPlayerName(winner)}!");

        yield return new WaitForSeconds(1f);

        yield return ResetRoundWithCountdownRoutine(winner, chosenEffect);
    }

    private IEnumerator EffectChoiceRoutine(PlayerController chooser, PlayerController target)
    {
        PickRandomEffectChoices();

        selectedChoiceIndex = 0;

        if (currentChoices.Count > 0)
            selectedEffectCard = currentChoices[selectedChoiceIndex];
        else
            selectedEffectCard = null;

        choosingEffect = true;

        SetMessage($"{GetPlayerName(chooser)}, choose a curse for {GetPlayerName(target)}");
        RefreshEffectChoiceUI();

        while (choosingEffect)
            yield return null;

        SetEffectChoice("");
    }

    private IEnumerator ResetRoundWithCountdownRoutine(PlayerController cursedPlayer, PlayerEffect curseEffect)
    {
        ClearAllPlayerEffects();

        if (cursedPlayer != null && curseEffect != null)
            cursedPlayer.Effects.AddEffect(curseEffect);

        ResetBothPlayersForRound();

        SetMessage("Next test begins...");
        SetEffectChoice("");

        for (int i = roundCountdownSeconds; i > 0; i--)
        {
            SetCountdown(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        SetCountdown("");
        SetMessage("GO!");

        roundLocked = false;

        yield return new WaitForSeconds(1f);

        SetMessage("");
    }

    private IEnumerator StartRoundRoutine()
    {
        roundLocked = true;

        ClearAllPlayerEffects();
        ResetBothPlayersForRound();

        SetMessage("Get ready!");

        for (int i = roundCountdownSeconds; i > 0; i--)
        {
            SetCountdown(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        SetCountdown("");
        SetMessage("GO!");

        roundLocked = false;

        yield return new WaitForSeconds(1f);

        SetMessage("");
    }

    private IEnumerator PlayerDeathRoutine(PlayerController deadPlayer)
    {
        deadPlayer.SetState(PlayerState.Disabled);

        SetMessage($"{GetPlayerName(deadPlayer)} got lab-tested...");

        for (int i = deathRespawnSeconds; i > 0; i--)
        {
            SetCountdown($"{GetPlayerName(deadPlayer)} respawns in {i}");
            yield return new WaitForSeconds(1f);
        }

        SetCountdown("");

        Transform spawn = GetSpawn(deadPlayer);

        if (spawn != null)
            deadPlayer.RespawnAt(spawn.position, spawn.rotation);

        SetMessage("");
    }

    private void HandleEffectSelectionInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        bool leftPressed =
            keyboard.aKey.wasPressedThisFrame ||
            keyboard.leftArrowKey.wasPressedThisFrame;

        bool rightPressed =
            keyboard.dKey.wasPressedThisFrame ||
            keyboard.rightArrowKey.wasPressedThisFrame;

        bool confirmPressed =
            keyboard.spaceKey.wasPressedThisFrame ||
            keyboard.leftCtrlKey.wasPressedThisFrame ||
            keyboard.rightCtrlKey.wasPressedThisFrame;

        if (currentChoices.Count == 0)
        {
            if (confirmPressed)
                choosingEffect = false;

            return;
        }

        if (leftPressed)
        {
            selectedChoiceIndex--;

            if (selectedChoiceIndex < 0)
                selectedChoiceIndex = currentChoices.Count - 1;

            selectedEffectCard = currentChoices[selectedChoiceIndex];
            RefreshEffectChoiceUI();
        }

        if (rightPressed)
        {
            selectedChoiceIndex++;

            if (selectedChoiceIndex >= currentChoices.Count)
                selectedChoiceIndex = 0;

            selectedEffectCard = currentChoices[selectedChoiceIndex];
            RefreshEffectChoiceUI();
        }

        if (confirmPressed)
        {
            selectedEffectCard = currentChoices[selectedChoiceIndex];
            choosingEffect = false;
        }
    }

    private void PickRandomEffectChoices()
    {
        currentChoices.Clear();

        List<EffectCardDefinition> available = new List<EffectCardDefinition>();

        foreach (EffectCardDefinition card in hazardPool)
        {
            if (card != null && card.effectAsset != null)
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

    private void RefreshEffectChoiceUI()
    {
        if (effectChoiceText == null)
            return;

        if (currentChoices.Count == 0)
        {
            effectChoiceText.text = "No effects available. Press Space or Ctrl to continue.";
            return;
        }

        string text = "A/D or ←/→ to choose    Space/Ctrl to confirm\n\n";

        for (int i = 0; i < currentChoices.Count; i++)
        {
            string effectName = currentChoices[i].effectAsset.effectName;

            if (i == selectedChoiceIndex)
                text += $"<b>[ {effectName} ]</b>   ";
            else
                text += $"{effectName}   ";
        }

        effectChoiceText.text = text;
    }

    private void ResetBothPlayersForRound()
    {
        if (player1 != null && player1Spawn != null)
            player1.ResetForRound(player1Spawn.position, player1Spawn.rotation);

        if (player2 != null && player2Spawn != null)
            player2.ResetForRound(player2Spawn.position, player2Spawn.rotation);
    }

    private void ClearAllPlayerEffects()
    {
        if (player1 != null)
            player1.Effects.ClearEffects();

        if (player2 != null)
            player2.Effects.ClearEffects();
    }

    private void ResetMatch()
    {
        p1Score = 0;
        p2Score = 0;

        matchOver = false;

        UpdateScoreUI();
        ClearTemporaryUI();

        StartCoroutine(StartRoundRoutine());
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

    private void SetCountdown(string text)
    {
        if (countdownText != null)
            countdownText.text = text;
    }

    private void SetEffectChoice(string text)
    {
        if (effectChoiceText != null)
            effectChoiceText.text = text;
    }

    private void ClearTemporaryUI()
    {
        SetMessage("");
        SetCountdown("");
        SetEffectChoice("");
    }
}