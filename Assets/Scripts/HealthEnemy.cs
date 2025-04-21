using System;
using System.Collections;
using UnityEngine;

public class HealthEnemy : MonoBehaviour
{
    private float health = 100f;
    private Animator animator;
    private Rigidbody[] ragdollBodies;
    public GameObject BloodPrefab;
    public GameObject HeadPrefab;
    public bool isDead { get; private set; }

    [SerializeField]
    private float knockbackForce = 10f;

    void Start()
    {
        animator = GetComponent<Animator>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        // for debugging purposes
        //Spawn();
        // for debugging purposes
      
        if (health <= 0)
        {
            if (animator.enabled)
            {
                animator.enabled = false;
                foreach (Rigidbody rb in ragdollBodies)
                {
                    rb.isKinematic = false;
                }
                StartCoroutine(Die());
            }
        }
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

    public void TakeDamage(float damage, Vector3 knockbackDirection, RaycastHit hit)
    {
        health -= damage;
        isDead = health <= 0;
        Debug.Log("Health: " + health);
        if (!isDead)
        {
            Hit();
        }
        HitEffectVFX(hit);

        if (isDead)
        {
            //ApplyKnockback(knockbackDirection);
        }
    }

    private void HitEffectVFX(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Head")){
            HeadVFX(hit);
        }
        else
        {
            BloodVFX(hit);
        }
    }

    private void BloodVFX(RaycastHit hit)
    {
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
        GameObject effect = Instantiate(BloodPrefab, hit.point, rot);
        Destroy(effect, 2f); 
    }

    private void HeadVFX(RaycastHit hit)
    {
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
        GameObject effect = Instantiate(HeadPrefab, hit.point, rot);
        Destroy(effect, 2f);
    }

    private void ApplyKnockback(Vector3 direction)
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(10f);
        animator.enabled = true;
        health = 100f;
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = true;
        }
    }

    private void Spawn()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            animator.enabled = true;
            health = 100f;
            foreach (Rigidbody rb in ragdollBodies)
            {
                rb.isKinematic = true;
            }
        }

    }

    public bool IsDead(float damage)
    {
        return health-damage <= 0;
    }
}
