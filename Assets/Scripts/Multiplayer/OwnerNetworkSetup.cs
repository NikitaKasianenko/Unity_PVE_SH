using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class OwnerNetworkSetup : NetworkBehaviour
    {
        [Tooltip("Components active only on the local owner (FPS controller, character recoil, etc.).")]
        [SerializeField] private Behaviour[] ownerOnlyBehaviours;

        [Tooltip("GameObjects active only on the local owner (the first-person camera + weapon rig). Ship these DISABLED in the prefab.")]
        [SerializeField] private GameObject[] ownerOnlyObjects;

        private bool _applied;

        public override void OnNetworkSpawn()
        {
            Apply(IsOwner);
        }

        private void Start()
        {
            if (!_applied && (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening))
                Apply(true);
        }

        private void Apply(bool isOwner)
        {
            _applied = true;

            if (ownerOnlyBehaviours != null)
                foreach (var b in ownerOnlyBehaviours)
                    if (b != null) b.enabled = isOwner;

            if (ownerOnlyObjects != null)
                foreach (var go in ownerOnlyObjects)
                    if (go != null) go.SetActive(isOwner);
        }
    }
}
