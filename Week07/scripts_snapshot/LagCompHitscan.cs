using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public struct HitscanResult
    {
        public bool Hit;
        public bool Headshot;
        public Vector3 Point;
        public HitboxHistoryBehaviour Target;
    }

    public static class LagCompHitscan
    {
        public static bool RaySphere(Vector3 origin, Vector3 dir, Vector3 center, float radius,
                                     float maxDist, out float dist)
        {
            dist = 0f;
            Vector3 m = origin - center;
            float b = Vector3.Dot(m, dir);
            float c = Vector3.Dot(m, m) - radius * radius;

            if (c > 0f && b > 0f) return false;

            float disc = b * b - c;
            if (disc < 0f) return false;

            float t = -b - Mathf.Sqrt(disc);
            if (t < 0f) t = 0f;
            if (t > maxDist) return false;

            dist = t;
            return true;
        }

        public static bool Trace(Vector3 origin, Vector3 dir, bool rewind, double atTime,
                                 float range, NetworkObject ignore, bool debug, out HitscanResult result)
        {
            dir = dir.normalized;
            result = default;
            float best = float.MaxValue;

            var all = HitboxHistoryBehaviour.All;
            for (int i = 0; i < all.Count; i++)
            {
                HitboxHistoryBehaviour h = all[i];
                if (h == null) continue;
                if (ignore != null && h.NetworkObject == ignore) continue;

                Vector3 headPos, torsoPos;
                if (rewind)
                {
                    if (!h.TryGetRewound(atTime, out headPos, out torsoPos)) continue;
                }
                else
                {
                    if (h.Head == null || h.Torso == null) continue;
                    headPos = h.Head.position;
                    torsoPos = h.Torso.position;
                }

                if (debug)
                {
                    DrawSphere(headPos, h.HeadRadius, Color.red);
                    DrawSphere(torsoPos, h.TorsoRadius, Color.yellow);
                }

                if (RaySphere(origin, dir, headPos, h.HeadRadius, range, out float dh) && dh < best)
                {
                    best = dh;
                    result = new HitscanResult
                    { Hit = true, Headshot = true, Point = origin + dir * dh, Target = h };
                }

                if (RaySphere(origin, dir, torsoPos, h.TorsoRadius, range, out float dt) && dt < best)
                {
                    best = dt;
                    result = new HitscanResult
                    { Hit = true, Headshot = false, Point = origin + dir * dt, Target = h };
                }
            }

            if (debug) Debug.DrawRay(origin, dir * range, Color.cyan, 2f);
            return result.Hit;
        }

        private static void DrawSphere(Vector3 c, float r, Color col)
        {
            Debug.DrawLine(c + Vector3.up * r, c - Vector3.up * r, col, 2f);
            Debug.DrawLine(c + Vector3.right * r, c - Vector3.right * r, col, 2f);
            Debug.DrawLine(c + Vector3.forward * r, c - Vector3.forward * r, col, 2f);
        }
    }
}
