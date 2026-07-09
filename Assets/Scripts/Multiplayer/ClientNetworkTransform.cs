using Unity.Netcode.Components;
using UnityEngine;

namespace Game.Multiplayer
{
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
