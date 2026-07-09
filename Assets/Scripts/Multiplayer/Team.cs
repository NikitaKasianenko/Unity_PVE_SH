using UnityEngine;

namespace Game.Multiplayer
{
    public static class Team
    {
        public const int None = -1;
        public const int A = 0;
        public const int B = 1;

        public static readonly Color ColorA = new Color(0.20f, 0.45f, 1.00f);
        public static readonly Color ColorB = new Color(1.00f, 0.27f, 0.22f);
        public static readonly Color ColorNone = new Color(0.6f, 0.6f, 0.6f);

        public static Color ColorFor(int team) =>
            team == A ? ColorA : team == B ? ColorB : ColorNone;

        public static string NameFor(int team) =>
            team == A ? "A" : team == B ? "B" : "-";

        public static bool IsValid(int team) => team == A || team == B;
    }
}
