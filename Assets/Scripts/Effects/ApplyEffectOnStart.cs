using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class ApplyEffectOnStart : MonoBehaviour
{
    [SerializeField] private PlayerController targetPlayer;
    [SerializeField] private PlayerEffect effectToApply;
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool callRoundStartAfterApply = true;
    [SerializeField] private float startDelay = 0.1f;

    private void Start()
    {
        if (!applyOnStart)
            return;

        StartCoroutine(ApplyRoutine());
    }

    private IEnumerator ApplyRoutine()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        if (targetPlayer == null)
        {
            Debug.LogWarning($"{nameof(ApplyEffectOnStart)} on {name} is missing a targetPlayer.", this);
            yield break;
        }

        if (effectToApply == null)
        {
            Debug.LogWarning($"{nameof(ApplyEffectOnStart)} on {name} is missing an effectToApply.", this);
            yield break;
        }

        if (targetPlayer.Effects == null)
        {
            Debug.LogWarning($"{nameof(ApplyEffectOnStart)} on {name} could not find a PlayerEffectController on target player.", this);
            yield break;
        }

        targetPlayer.Effects.AddEffect(effectToApply);

        if (callRoundStartAfterApply)
            targetPlayer.Effects.NotifyRoundStart();
    }
}
