using System;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Vector3 hitDirection, HitData hit);
    event Action<Vector3, HitData> OnDeath;

}
