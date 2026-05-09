using UnityEngine;

[System.Serializable]
public class EffectCardDefinition
{
    public string cardName;
    public Sprite cardImage;
    public EffectType effectType;
    public PlayerEffect effectAsset;

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(cardName))
                return cardName;

            if (effectAsset != null)
                return effectAsset.effectName;

            return effectType.ToString();
        }
    }
}