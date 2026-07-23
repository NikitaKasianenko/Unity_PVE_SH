using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class WeaponBehaviour : NetworkBehaviour
    {
        [Header("Fire origin")]
        [SerializeField] private Transform fireOrigin;

        [Header("Ballistics")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float headMultiplier = 2f;
        [SerializeField] private float range = 300f;
        [SerializeField] private float fireCooldown = 0.12f;

        [Header("Lag compensation")]
        [SerializeField] private double rewindWindow = 0.25;

        [Header("Hit VFX (networked)")]
        [SerializeField] private GameObject bloodPrefab;
        [SerializeField] private GameObject headshotPrefab;

        [Header("Debug")]
        [SerializeField] private bool drawDebug = true;

        private float _nextFire;

        private void Update()
        {
#if !UNITY_SERVER
            if (!IsOwner || !IsSpawned || fireOrigin == null) return;

            if (Input.GetButton("Fire1") && Time.time >= _nextFire)
            {
                _nextFire = Time.time + fireCooldown;
                FireServerRpc(fireOrigin.position, fireOrigin.forward, NetworkManager.ServerTime.Time);
            }
#endif
        }

        [ServerRpc]
        private void FireServerRpc(Vector3 origin, Vector3 direction, double clientFireTime,
                                   ServerRpcParams rpcParams = default)
        {
            ulong shooter = rpcParams.Receive.SenderClientId;

            double now = NetworkManager.ServerTime.Time;
            double age = now - clientFireTime;
            if (age > rewindWindow) return;
            if (age < 0.0) clientFireTime = now;

            if (LagCompHitscan.Trace(origin, direction, true, clientFireTime, range,
                                     NetworkObject, drawDebug, out HitscanResult r))
            {
                float finalDamage = damage * (r.Headshot ? headMultiplier : 1f);

                if (r.Target != null && r.Target.TryGetComponent(out IServerDamageable target))
                    target.ApplyServerDamage(finalDamage, shooter, r.Headshot);

                HitFeedbackClientRpc(r.Point, r.Headshot);
            }
        }

        [ClientRpc]
        private void HitFeedbackClientRpc(Vector3 point, bool headshot)
        {
            GameObject prefab = headshot ? headshotPrefab : bloodPrefab;
            if (prefab == null) return;

            GameObject fx = Instantiate(prefab, point, Quaternion.identity);
            Destroy(fx, 2f);
        }
    }
}
