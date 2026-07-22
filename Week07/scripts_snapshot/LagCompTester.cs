using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class LagCompTester : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private HitboxHistoryBehaviour target;
        [SerializeField] private Transform shooterOrigin;

        [Header("Test parameters")]
        [SerializeField] private int shotsPerSetting = 20;
        [SerializeField] private float shotInterval = 0.15f;
        [SerializeField] private float range = 300f;
        [SerializeField] private double rewindWindow = 0.25;
        [SerializeField] private double[] latencies = { 0.0, 0.1, 0.2, 0.3 };
        [SerializeField] private bool aimAtHead = false;
        [SerializeField] private bool runOnStart = false;

        private readonly List<string> _csv = new List<string>();

        private IEnumerator Start()
        {
            if (!runOnStart) yield break;

            yield return new WaitUntil(() =>
                NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsServer &&
                target != null && target.IsSpawned);

            yield return new WaitForSeconds(1.5f);
            yield return RunTest();
        }

        [ContextMenu("Run Latency Test")]
        public void RunFromMenu() => StartCoroutine(RunTest());

        public IEnumerator RunTest()
        {
            if (target == null || shooterOrigin == null)
            {
                Debug.LogError("[LagCompTester] target and shooterOrigin must be assigned.");
                yield break;
            }

            _csv.Clear();
            _csv.Add("latency_ms,shots,naive_hits,rewind_hits,rewind_uncapped_hits," +
                     "naive_hitrate,rewind_hitrate,rewind_uncapped_hitrate");

            foreach (double lat in latencies)
            {
                int naiveHits = 0;
                int rewindHits = 0;
                int uncappedHits = 0;

                for (int i = 0; i < shotsPerSetting; i++)
                {
                    double now = NetworkManager.Singleton.ServerTime.Time;
                    double fireTime = now - lat;

                    if (!target.TryGetRewound(fireTime, out Vector3 seenHead, out Vector3 seenTorso))
                    {
                        yield return new WaitForSeconds(shotInterval);
                        continue;
                    }

                    Vector3 origin = shooterOrigin.position;
                    Vector3 aimPoint = aimAtHead ? seenHead : seenTorso;
                    Vector3 dir = (aimPoint - origin).normalized;

                    if (LagCompHitscan.Trace(origin, dir, false, 0, range, null, false, out _))
                        naiveHits++;

                    bool geometricHit = LagCompHitscan.Trace(origin, dir, true, fireTime, range, null, true, out _);
                    if (geometricHit) uncappedHits++;
                    if (geometricHit && lat <= rewindWindow) rewindHits++;

                    yield return new WaitForSeconds(shotInterval);
                }

                _csv.Add(string.Format(CultureInfo.InvariantCulture,
                    "{0:0},{1},{2},{3},{4},{5:0.00},{6:0.00},{7:0.00}",
                    lat * 1000.0, shotsPerSetting, naiveHits, rewindHits, uncappedHits,
                    (double)naiveHits / shotsPerSetting,
                    (double)rewindHits / shotsPerSetting,
                    (double)uncappedHits / shotsPerSetting));

                Debug.Log($"[LagCompTester] {lat * 1000.0:0}ms  naive {naiveHits}/{shotsPerSetting}  " +
                          $"rewind {rewindHits}/{shotsPerSetting}  uncapped {uncappedHits}/{shotsPerSetting}");
            }

            WriteCsv();
        }

        private void WriteCsv()
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Week07"));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "hit_rate.csv");
            File.WriteAllText(path, string.Join("\n", _csv), new UTF8Encoding(false));
            Debug.Log($"[LagCompTester] wrote {path}");
        }
    }
}
