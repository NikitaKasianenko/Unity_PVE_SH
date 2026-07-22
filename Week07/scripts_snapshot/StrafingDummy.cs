using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    [RequireComponent(typeof(HitboxHistoryBehaviour))]
    public class StrafingDummy : NetworkBehaviour, IServerDamageable
    {
        [Header("Strafe motion (server-driven)")]
        [SerializeField] private float amplitude = 3f;
        [SerializeField] private float speed = 2f;
        [SerializeField] private Vector3 axis = Vector3.right;

        [Header("Measurement")]
        public NetworkVariable<int> hitCount = new NetworkVariable<int>();
        public NetworkVariable<int> headshotCount = new NetworkVariable<int>();

        public bool Moving { get; set; } = true;

        private Vector3 _center;

        public override void OnNetworkSpawn()
        {
            _center = transform.position;
        }

        private void Update()
        {
            if (!IsServer || !Moving) return;

            double t = NetworkManager.ServerTime.Time;
            float offset = Mathf.Sin((float)t * speed) * amplitude;
            transform.position = _center + axis.normalized * offset;
        }

        public void ApplyServerDamage(float amount, ulong attackerClientId, bool headshot)
        {
            if (!IsServer) return;

            hitCount.Value++;
            if (headshot) headshotCount.Value++;
        }

        public void ResetCounters()
        {
            if (!IsServer) return;

            hitCount.Value = 0;
            headshotCount.Value = 0;
        }
    }
}
