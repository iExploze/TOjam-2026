using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Spinner : MonoBehaviour
{
    [Header("Spin")]
    [SerializeField] private float speedDegreesPerSecond = 90f;
    [SerializeField] private bool useLocalSpace = true;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float upwardForce = 2f;
    [SerializeField] private float hitCooldownSeconds = 0.2f;
    [SerializeField] private bool onlyAffectPlayers = true;

    private readonly Dictionary<Rigidbody, float> lastHitTimes = new Dictionary<Rigidbody, float>();
    private Rigidbody spinnerRb;

    private void Awake()
    {
        spinnerRb = GetComponent<Rigidbody>();
        spinnerRb.isKinematic = true;
        spinnerRb.useGravity = false;
        spinnerRb.interpolation = RigidbodyInterpolation.Interpolate;
        spinnerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void Update()
    {
        float step = speedDegreesPerSecond * Time.deltaTime;
        Vector3 rotation = new Vector3(0f, step, 0f);

        if (useLocalSpace)
        {
            transform.Rotate(rotation, Space.Self);
        }
        else
        {
            transform.Rotate(rotation, Space.World);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryKnockback(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryKnockback(collision);
    }

    private void TryKnockback(Collision collision)
    {
        Rigidbody targetRb = collision.rigidbody;
        if (targetRb == null)
            return;

        if (onlyAffectPlayers && targetRb.GetComponent<PlayerController>() == null)
            return;

        float now = Time.time;
        if (lastHitTimes.TryGetValue(targetRb, out float lastHitTime) && now - lastHitTime < hitCooldownSeconds)
            return;

        Vector3 awayDirection = targetRb.worldCenterOfMass - transform.position;
        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.001f)
            awayDirection = transform.forward;

        awayDirection.Normalize();

        Vector3 impulse = (awayDirection * knockbackForce) + (Vector3.up * upwardForce);
        targetRb.AddForce(impulse, ForceMode.Impulse);
        lastHitTimes[targetRb] = now;
    }
}
