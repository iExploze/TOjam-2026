using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectHudUI : MonoBehaviour
{
    [SerializeField] private PlayerController targetPlayer;
    [SerializeField] private PlayerEffectHudSlotUI slotPrefab;
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private bool refreshOnStart = true;

    private readonly List<PlayerEffectHudSlotUI> spawnedSlots = new();

    private void OnEnable()
    {
        SubscribeToPlayer();
        Refresh();
    }

    private void Start()
    {
        if (refreshOnStart)
            Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayer();
    }

    public void SetTargetPlayer(PlayerController player)
    {
        if (targetPlayer == player)
        {
            Refresh();
            return;
        }

        UnsubscribeFromPlayer();
        targetPlayer = player;
        SubscribeToPlayer();
        Refresh();
    }

    public void Refresh()
    {
        ClearSpawnedSlots();

        if (targetPlayer == null || targetPlayer.Effects == null)
            return;

        if (slotPrefab == null || slotContainer == null)
            return;

        IReadOnlyList<PlayerEffect> activeEffects = targetPlayer.Effects.ActiveEffects;

        for (int i = 0; i < activeEffects.Count; i++)
        {
            PlayerEffect effect = activeEffects[i];

            if (effect == null || !effect.ShowOnHud)
                continue;

            PlayerEffectHudSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.SetEffect(effect);
            spawnedSlots.Add(slot);
        }
    }

    private void SubscribeToPlayer()
    {
        if (targetPlayer == null || targetPlayer.Effects == null)
            return;

        targetPlayer.Effects.EffectsChanged -= HandleEffectsChanged;
        targetPlayer.Effects.EffectsChanged += HandleEffectsChanged;
    }

    private void UnsubscribeFromPlayer()
    {
        if (targetPlayer == null || targetPlayer.Effects == null)
            return;

        targetPlayer.Effects.EffectsChanged -= HandleEffectsChanged;
    }

    private void HandleEffectsChanged()
    {
        Refresh();
    }

    private void ClearSpawnedSlots()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] == null)
                continue;

            Destroy(spawnedSlots[i].gameObject);
        }

        spawnedSlots.Clear();
    }
}
