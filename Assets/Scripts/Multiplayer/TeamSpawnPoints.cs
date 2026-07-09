using UnityEngine;

namespace Game.Multiplayer
{
    public class TeamSpawnPoints : MonoBehaviour
    {
        public static TeamSpawnPoints Instance { get; private set; }

        [Tooltip("Spawn markers for Team A (blue). Empty GameObjects placed in the level.")]
        [SerializeField] private Transform[] teamASpawns;

        [Tooltip("Spawn markers for Team B (red). Empty GameObjects placed in the level.")]
        [SerializeField] private Transform[] teamBSpawns;

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public bool TryGetSpawn(int team, out Vector3 position, out Quaternion rotation)
        {
            var list = team == Team.B ? teamBSpawns : teamASpawns;
            if (list != null && list.Length > 0)
            {
                var t = list[Random.Range(0, list.Length)];
                if (t != null)
                {
                    position = t.position;
                    rotation = t.rotation;
                    return true;
                }
            }

            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }
    }
}
