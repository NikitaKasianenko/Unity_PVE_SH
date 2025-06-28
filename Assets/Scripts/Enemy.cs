using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    private float maxHealth = 100f;
    private float health;
    private Animator animator;
    private Rigidbody[] ragdollBodies;
    public GameObject BloodPrefab;
    public GameObject HeadPrefab;
    public bool isDead { get; private set; }


    public event Action<Vector3, HitData> OnDeath;
    public Action OnRespawn;

    private void OnEnable()
    {
        OnRespawn += HandleRespawn;

    }
    private void OnDisable()
    {
        OnRespawn -= HandleRespawn;
    }

    void Start()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = true;
        }
    }
    private void HandleRespawn()
    {
        health = maxHealth;
    }


    public IEnumerator HitAnimation()
    {
        animator.SetTrigger("Hit");
        yield return new WaitForSeconds(0.3f);

    }

    private void Hit()
    {
        StartCoroutine(HitAnimation());

    }

    public void TakeDamage(float damage, Vector3 knockbackDirection, HitData hit)
    {
        health -= damage;
        Debug.Log("Health: " + health);

        if (health > 0)
        {
            Hit();
            HitEffectVFX(hit);
        }
        else
        {
            OnDeath?.Invoke(knockbackDirection, hit);
            HitEffectVFX(hit);
        }
    }

    private void HitEffectVFX(HitData hit)
    {
        if (hit.Collider.CompareTag("Head"))
        {
            HeadVFX(hit);
        }
        else
        {
            BloodVFX(hit);
        }
    }

    private void BloodVFX(HitData hit)
    {
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.Normal);
        GameObject effect = Instantiate(BloodPrefab, hit.Point, rot);
        Destroy(effect, 2f);
    }

    private void HeadVFX(HitData hit)
    {
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.Normal);
        GameObject effect = Instantiate(HeadPrefab, hit.Point, rot);
        Destroy(effect, 2f);
    }



}
