using System.Collections;
using UnityEngine;

public class RagdollKnockback : MonoBehaviour
{
    [SerializeField] private float knockbackForce = 15f;
    private Animator animator;
    private Rigidbody[] ragdollBodies;
    private Enemy enemy;


    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("RagdollKnockback requires an Enemy component.");
        }
    }

    private void OnEnable()
    {
        enemy.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        enemy.OnDeath -= HandleDeath;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = true;

        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = true;
        }
    }

    private void HandleDeath(Vector3 hitDirection, HitData hit)
    {
        if (animator.enabled)
        {
            animator.enabled = false;
            foreach (Rigidbody rb in ragdollBodies)
            {
                rb.isKinematic = false;
            }
            StartCoroutine(Die(hitDirection, hit));
        }
    }

    private IEnumerator Die(Vector3 direction, HitData hit)
    {
        yield return null;

        if (hit.Collider.attachedRigidbody != null)
        {
            hit.Collider.attachedRigidbody.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(10f);
        // respawn
        Respawn();
    }

    private void Respawn()
    {
        animator.enabled = true;
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = true;
        }
        enemy.OnRespawn?.Invoke();
    }
}
