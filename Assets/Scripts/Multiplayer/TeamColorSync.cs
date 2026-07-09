using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class TeamColorSync : NetworkBehaviour
    {
        [Tooltip("The body renderer to tint (the capsule mesh).")]
        [SerializeField] private Renderer bodyRenderer;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private PlayerDataBehaviour _data;

        public override void OnNetworkSpawn()
        {
            _data = GetComponent<PlayerDataBehaviour>();
            if (_data != null)
            {
                _data.teamId.OnValueChanged += OnTeamChanged;
                Apply(_data.TeamId);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (_data != null)
                _data.teamId.OnValueChanged -= OnTeamChanged;
        }

        private void OnTeamChanged(int previous, int current) => Apply(current);

        private void Apply(int team)
        {
            if (bodyRenderer == null) return;

            var mpb = new MaterialPropertyBlock();
            bodyRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColor, Team.ColorFor(team));
            bodyRenderer.SetPropertyBlock(mpb);
        }
    }
}
