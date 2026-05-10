using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardOverlayUI : MonoBehaviour
{
    [SerializeField] private GameObject cardOverlay;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private CardChoiceSlotUI[] cardSlots;

    private void Awake()
    {
        Hide();
    }

    public void Show(PlayerId loserId, List<EffectCardDefinition> choices, int selectedIndex)
    {
        if (cardOverlay != null)
            cardOverlay.SetActive(true);

        string loserName = loserId == PlayerId.Player1 ? "Player 1" : "Player 2";

        if (titleText != null)
            titleText.text = $"{loserName} picks a card";

        UpdateSelection(choices, selectedIndex);
    }

    public void UpdateSelection(List<EffectCardDefinition> choices, int selectedIndex)
    {
        if (cardSlots == null)
            return;

        for (int i = 0; i < cardSlots.Length; i++)
        {
            EffectCardDefinition card = null;

            if (choices != null && i < choices.Count)
                card = choices[i];

            if (cardSlots[i] != null)
                cardSlots[i].SetCard(card, i == selectedIndex);
        }
    }

    public void Hide()
    {
        if (cardOverlay != null)
            cardOverlay.SetActive(false);
    }
}