using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    [DisallowMultipleComponent]
    public class HitboxHistoryBehaviour : NetworkBehaviour
    {
        [Header("Hitboxes (treated as spheres)")]
        [SerializeField] private Transform head;
        [SerializeField] private float headRadius = 0.18f;

        [SerializeField] private Transform torso;
        [SerializeField] private float torsoRadius = 0.35f;

        [Header("Buffer")]
        [SerializeField] private int capacity = 24;

        public float HeadRadius => headRadius;
        public float TorsoRadius => torsoRadius;
        public Transform Head => head;
        public Transform Torso => torso;

        private struct Snapshot
        {
            public double Time;
            public Vector3 Head;
            public Vector3 Torso;
        }

        private Snapshot[] _buffer;
        private int _count;
        private int _next;

        public static readonly List<HitboxHistoryBehaviour> All = new List<HitboxHistoryBehaviour>();

        public override void OnNetworkSpawn()
        {
            _buffer = new Snapshot[Mathf.Max(2, capacity)];
            _count = 0;
            _next = 0;

            All.Add(this);

            if (IsServer)
                NetworkManager.NetworkTickSystem.Tick += RecordSnapshot;
        }

        public override void OnNetworkDespawn()
        {
            All.Remove(this);

            if (IsServer && NetworkManager != null && NetworkManager.NetworkTickSystem != null)
                NetworkManager.NetworkTickSystem.Tick -= RecordSnapshot;
        }

        private void RecordSnapshot()
        {
            if (head == null || torso == null) return;

            _buffer[_next] = new Snapshot
            {
                Time = NetworkManager.ServerTime.Time,
                Head = head.position,
                Torso = torso.position
            };

            _next = (_next + 1) % _buffer.Length;
            if (_count < _buffer.Length) _count++;
        }

        public bool TryGetRewound(double time, out Vector3 headPos, out Vector3 torsoPos)
        {
            headPos = head != null ? head.position : Vector3.zero;
            torsoPos = torso != null ? torso.position : Vector3.zero;

            if (_count == 0) return false;

            int newest = (_next - 1 + _buffer.Length) % _buffer.Length;

            if (time >= _buffer[newest].Time)
            {
                headPos = _buffer[newest].Head;
                torsoPos = _buffer[newest].Torso;
                return true;
            }

            for (int i = 0; i < _count - 1; i++)
            {
                int newerIdx = (newest - i + _buffer.Length) % _buffer.Length;
                int olderIdx = (newest - i - 1 + _buffer.Length) % _buffer.Length;
                Snapshot newer = _buffer[newerIdx];
                Snapshot older = _buffer[olderIdx];

                if (time <= newer.Time && time >= older.Time)
                {
                    double span = newer.Time - older.Time;
                    float t = span > 0.0 ? (float)((time - older.Time) / span) : 0f;
                    headPos = Vector3.Lerp(older.Head, newer.Head, t);
                    torsoPos = Vector3.Lerp(older.Torso, newer.Torso, t);
                    return true;
                }
            }

            int oldest = _count < _buffer.Length ? 0 : _next;
            headPos = _buffer[oldest].Head;
            torsoPos = _buffer[oldest].Torso;
            return true;
        }
    }
}
