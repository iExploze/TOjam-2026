using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEffectHudSlotUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text optionalNameText;

    public void SetEffect(PlayerEffect effect)
    {
        if (effect == null)
        {
            ClearAndHide();
            return;
        }

        if (cardImage != null)
        {
            cardImage.sprite = effect.HudIcon;
            cardImage.enabled = effect.HudIcon != null;
        }

        if (optionalNameText != null)
            optionalNameText.text = effect.effectName;

        gameObject.SetActive(true);
    }

    private void ClearAndHide()
    {
        if (cardImage != null)
        {
            cardImage.sprite = null;
            cardImage.enabled = false;
        }

        if (optionalNameText != null)
            optionalNameText.text = string.Empty;

        gameObject.SetActive(false);
    }
}
