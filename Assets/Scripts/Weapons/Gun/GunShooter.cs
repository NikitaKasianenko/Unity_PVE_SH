using UnityEngine;

public class GunShooter
{
    private readonly GunData data;
    private readonly Transform rayCaster;

    public GunShooter(GunData data, Transform rayCaster)
    {
        this.data = data;
        this.rayCaster = rayCaster;
    }

    public bool TryShoot(out RaycastHit hit)
    {
        return Physics.Raycast(rayCaster.position, rayCaster.forward, out hit, data.shootingRange, data.targerLayerMask);
    }
}
