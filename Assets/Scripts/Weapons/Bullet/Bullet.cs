using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float lifetime = 5f;
    private float damage;
    private LayerMask hitLayers;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //rb.useGravity = false;  if we want to disable gravity
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }


    public void Initialize(Vector3 direction, float damage, float speed, LayerMask targetLayerMask)
    {
        this.damage = damage;
        this.hitLayers = targetLayerMask;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & hitLayers) == 0)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("Bullet collided with: " + collision.gameObject.name);

        var target = collision.collider.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            Debug.Log("Bullet hit target");

            Vector3 hitDir = rb.linearVelocity.normalized;
            ContactPoint contact = collision.GetContact(0);
            var hitData = new HitData
            {
                Point = contact.point,
                Normal = contact.normal,
                Collider = collision.collider
            };

            float mult = damageMultiplier.TryGetValue(collision.collider.tag, out var m) ? m : 1f;
            float finalDamage = damage * mult;
            target.TakeDamage(finalDamage, hitDir, hitData);
        }

        Destroy(gameObject);
    }

    protected Dictionary<string, float> damageMultiplier = new Dictionary<string, float>
    {
        { "Head", 5.0f },
        { "Body", 1.5f },
        { "Hand", 1.3f },
        { "Leg", 1.2f }
    };
}

public struct HitData
{
    public Vector3 Point;
    public Vector3 Normal;
    public Collider Collider;
}
