using UnityEngine;

public class GunAmmo
{
    public int currentAmmo { get; private set; }
    public int maxAmmo { get; private set; }


    public GunAmmo(GunData data)
    {
        maxAmmo = data.magazineSize;
        currentAmmo = maxAmmo;

    }

    public bool TryUseAmmo()
    {
        if (currentAmmo <= 0) return false;
        currentAmmo--;
        return true;
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
    }
}
