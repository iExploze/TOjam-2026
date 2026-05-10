using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerEffect : ScriptableObject
{
    [Header("Effect Info")]
    public string effectName;
    [TextArea] public string description;

    [Header("HUD Display")]
    [SerializeField] private Sprite hudIcon;
    [SerializeField] private bool showOnHud = true;

    public Sprite HudIcon => hudIcon;
    public bool ShowOnHud => showOnHud;

    public virtual void OnApply(PlayerController player) { }

    public virtual void ModifyStats(PlayerStats stats) { }

    public virtual void OnRoundStart(PlayerController player) { }

    public virtual void Tick(PlayerController player, float deltaTime) { }

    public virtual void OnRoundEnd(PlayerController player) { }

    public virtual void OnRemove(PlayerController player) { }
}

[RequireComponent(typeof(PlayerController))]
public class PlayerEffectController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool logEffects = true;

    private readonly List<PlayerEffect> activeEffects = new();

    private PlayerController player;

    public IReadOnlyList<PlayerEffect> ActiveEffects => activeEffects;
    public event System.Action EffectsChanged;

    public void Initialize(PlayerController owner)
    {
        player = owner;
    }

    private void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
    }

    public void AddEffect(PlayerEffect effectAsset)
    {
        if (effectAsset == null)
            return;

        PlayerEffect effectInstance = Instantiate(effectAsset);

        activeEffects.Add(effectInstance);

        effectInstance.OnApply(player);

        player.RecalculateStats();

        EffectsChanged?.Invoke();

        if (logEffects)
            Debug.Log($"{player.name} received effect: {effectInstance.effectName}");
    }

    public void RemoveEffect(PlayerEffect effect)
    {
        if (effect == null)
            return;

        if (activeEffects.Remove(effect))
        {
            effect.OnRemove(player);
            player.RecalculateStats();

            EffectsChanged?.Invoke();

            if (logEffects)
                Debug.Log($"{player.name} removed effect: {effect.effectName}");
        }
    }

    public void ClearEffects()
    {
        if (activeEffects.Count == 0)
            return;

        foreach (PlayerEffect effect in activeEffects)
        {
            if (effect != null)
                effect.OnRemove(player);
        }

        activeEffects.Clear();

        player.RecalculateStats();
        EffectsChanged?.Invoke();
        /*
        if (logEffects)
            Debug.Log($"{player.name} cleared all effects.");
        */
    }

    public void ModifyStats(PlayerStats stats)
    {
        foreach (PlayerEffect effect in activeEffects)
        {
            if (effect != null)
                effect.ModifyStats(stats);
        }
    }

    public void TickEffects(float deltaTime)
    {
        foreach (PlayerEffect effect in activeEffects)
        {
            if (effect != null)
                effect.Tick(player, deltaTime);
        }
    }

    public void NotifyRoundStart()
    {
        foreach (PlayerEffect effect in activeEffects)
        {
            if (effect != null)
                effect.OnRoundStart(player);
        }
    }

    public void NotifyRoundEnd()
    {
        foreach (PlayerEffect effect in activeEffects)
        {
            if (effect != null)
                effect.OnRoundEnd(player);
        }
    }
}
