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
        rb.useGravity = false;  // если не хотите, чтобы пуля падала
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// direction — направление выстрела,
    /// damage — базовый урон,
    /// speed — скорость,
    /// targetLayerMask — слои, по которым мы бьем
    /// </summary>
    public void Initialize(Vector3 direction, float damage, float speed, LayerMask targetLayerMask)
    {
        this.damage = damage;
        this.hitLayers = targetLayerMask;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1) Фильтрация по слоям
        if (((1 << collision.gameObject.layer) & hitLayers) == 0)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("Bullet collided with: " + collision.gameObject.name);

        // 2) Пытаемся найти IDamageable у цели
        var target = collision.collider.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            Debug.Log("Bullet hit target");

            // направление удара
            Vector3 hitDir = rb.linearVelocity.normalized;

            // точка и нормаль контакта
            ContactPoint contact = collision.GetContact(0);
            var hitData = new HitData
            {
                Point = contact.point,
                Normal = contact.normal,
                Collider = collision.collider
            };

            // вычисляем финальный урон с учетом тега
            float mult = damageMultiplier.TryGetValue(collision.collider.tag, out var m) ? m : 1f;
            float finalDamage = damage * mult;

            target.TakeDamage(finalDamage, hitDir, hitData);
        }

        Destroy(gameObject);
    }

    // словарь множителей, как у вас был
    protected Dictionary<string, float> damageMultiplier = new Dictionary<string, float>
    {
        { "Head", 5.0f },
        { "Body", 1.5f },
        { "Hand", 1.3f },
        { "Leg", 1.2f }
    };
}

// контейнер для данных удара
public struct HitData
{
    public Vector3 Point;
    public Vector3 Normal;
    public Collider Collider;
}
