using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    [RequireComponent(typeof(Camera))]
    public class LobbyCamera : MonoBehaviour
    {
        private void Update()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || nm.LocalClient == null || nm.LocalClient.PlayerObject == null)
                return;

            var playerCam = nm.LocalClient.PlayerObject.GetComponentInChildren<Camera>(false);
            if (playerCam != null && playerCam.isActiveAndEnabled)
                gameObject.SetActive(false);
        }
    }
}
