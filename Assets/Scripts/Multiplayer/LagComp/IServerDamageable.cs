namespace Game.Multiplayer
{
    public interface IServerDamageable
    {
        void ApplyServerDamage(float amount, ulong attackerClientId, bool headshot);
    }
}
