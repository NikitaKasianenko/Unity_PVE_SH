using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class LocalBodyVisibility : NetworkBehaviour
    {
        [Tooltip("Renderers that belong to THIS player's body — hidden for the local owner, visible to everyone else.")]
        [SerializeField] private Renderer[] localHiddenRenderers;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            if (localHiddenRenderers == null) return;

            foreach (var r in localHiddenRenderers)
                if (r != null) r.enabled = false;
        }
    }
}
