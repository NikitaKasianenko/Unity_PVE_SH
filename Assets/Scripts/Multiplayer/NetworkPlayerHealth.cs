using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    [RequireComponent(typeof(PlayerDataBehaviour))]
    public class NetworkPlayerHealth : NetworkBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [Tooltip("Seconds a player stays dead before respawning.")]
        [SerializeField] private float respawnDelay = 3f;

        [Tooltip("The body renderer — hidden while dead, and hidden for the local owner while alive.")]
        [SerializeField] private Renderer bodyRenderer;

        [Tooltip("Owner-only components disabled while dead (e.g. the FPS controller so you can't move).")]
        [SerializeField] private Behaviour[] disableWhileDead;

        public NetworkVariable<float> health = new NetworkVariable<float>();
        public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);

        private PlayerDataBehaviour _data;
        private CharacterController _cc;

        public event Action<Vector3, HitData> OnDeath;

        public override void OnNetworkSpawn()
        {
            _data = GetComponent<PlayerDataBehaviour>();
            _cc = GetComponent<CharacterController>();

            isDead.OnValueChanged += OnDeadStateChanged;
            ApplyDeadState(isDead.Value);

            if (IsServer) health.Value = maxHealth;
        }

        public override void OnNetworkDespawn()
        {
            isDead.OnValueChanged -= OnDeadStateChanged;
        }

        public void TakeDamage(float damage, Vector3 hitDirection, HitData hit)
        {
            if (!IsSpawned) return;
            if (IsOwner) return;

            if (IsServer)
                ApplyDamage(damage, NetworkManager.LocalClientId);
            else
                ApplyDamageServerRpc(damage);
        }

        public void ApplyDamage(float amount, ulong killerClientId)
        {
            if (!IsServer) return;
            if (isDead.Value) return;
            if (health.Value <= 0f) return;

            if (killerClientId != OwnerClientId && SessionManagerBehaviour.Instance != null)
            {
                int victimTeam = _data != null ? _data.TeamId : Team.None;
                int killerTeam = SessionManagerBehaviour.Instance.GetTeam(killerClientId);
                if (victimTeam == killerTeam) return;
            }

            health.Value -= amount;
            if (health.Value <= 0f)
            {
                health.Value = 0f;
                Die(killerClientId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ApplyDamageServerRpc(float amount, ServerRpcParams rpcParams = default)
        {
            ApplyDamage(amount, rpcParams.Receive.SenderClientId);
        }

        private void Die(ulong killerClientId)
        {
            isDead.Value = true;
            OnDeath?.Invoke(Vector3.zero, default);
            SessionManagerBehaviour.Instance?.HandleKill(OwnerClientId, killerClientId);
            StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);
            if (!IsServer || !IsSpawned) yield break;

            int team = _data != null ? _data.TeamId : Team.None;
            RespawnAtTeamSpawn(team);
            isDead.Value = false;
        }

        public void RespawnAtTeamSpawn(int team)
        {
            if (!IsServer) return;
            health.Value = maxHealth;

            if (TeamSpawnPoints.Instance != null &&
                TeamSpawnPoints.Instance.TryGetSpawn(team, out var pos, out _))
            {
                TeleportClientRpc(pos, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
                });
            }
        }

        [ClientRpc]
        private void TeleportClientRpc(Vector3 position, ClientRpcParams rpcParams = default)
        {
            if (_cc != null)
            {
                _cc.enabled = false;
                transform.position = position;
                _cc.enabled = true;
            }
            else
            {
                transform.position = position;
            }
        }

        private void OnDeadStateChanged(bool previous, bool dead) => ApplyDeadState(dead);

        private void ApplyDeadState(bool dead)
        {
            if (bodyRenderer != null)
                bodyRenderer.enabled = !dead && !IsOwner;

            if (IsOwner && disableWhileDead != null)
                foreach (var b in disableWhileDead)
                    if (b != null) b.enabled = !dead;
        }
    }
}
