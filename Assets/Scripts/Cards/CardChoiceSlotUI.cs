using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardChoiceSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private GameObject selectedFrame;

    [Header("Selection Visual")]
    [SerializeField] private float selectedScale = 1.15f;

    private Vector3 normalScale;

    private void Awake()
    {
        normalScale = transform.localScale;

        if (cardImage == null)
            cardImage = GetComponent<Image>();
    }

    public void SetCard(EffectCardDefinition card, bool isSelected)
    {
        bool hasCard = card != null;
        gameObject.SetActive(hasCard);

        if (!hasCard)
        {
            transform.localScale = normalScale;
            return;
        }

        if (cardImage != null)
        {
            cardImage.sprite = card.cardImage;
            cardImage.enabled = card.cardImage != null;
            cardImage.preserveAspect = true;
        }

        if (cardNameText != null)
            cardNameText.text = card.DisplayName;

        if (selectedFrame != null)
            selectedFrame.SetActive(isSelected);

        transform.localScale = isSelected
            ? normalScale * selectedScale
            : normalScale;

        if (isSelected)
            transform.SetAsLastSibling();
    }
}