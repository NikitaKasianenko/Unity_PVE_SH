using System.Collections;
using UnityEngine;

public class AK47 : Gun
{
    [SerializeField] private float knockbackForce = 15f;

    public override void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(rayCaster.position, rayCaster.forward, out hit, gunData.shootingRange, gunData.targerLayerMask))
        {
            if (hit.collider.gameObject.CompareTag("Enviroment") && hit.rigidbody != null)
            {
                hit.rigidbody.AddExplosionForce(350f, hit.point, 100f, 0f);
            }
            else
            {
                HealthEnemy healthEnemy = hit.collider.gameObject.GetComponentInParent<HealthEnemy>();
                if (healthEnemy != null)
                {
                    ApplyDamage(hit, healthEnemy);
                }
            }
        }
    }

    private void ApplyDamage(RaycastHit hit, HealthEnemy healthEnemy)
    {
        float damage = CalculateDamage(hit.collider.gameObject.tag);

        healthEnemy.TakeDamage(damage, rayCaster.forward, hit);

        if (healthEnemy.IsDead(damage) && hit.rigidbody != null)
        {
            StartCoroutine(ApplyKnockback(hit));
        }
    }

    private IEnumerator ApplyKnockback(RaycastHit hit)
    {
        yield return null;

        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForce(rayCaster.forward * knockbackForce, ForceMode.Impulse);
        }
    }
}