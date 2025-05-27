public class GunAmmo
{
    public int currentAmmo { get; private set; }
    public int maxAmmo { get; private set; }


    public GunAmmo(GunData data)
    {
        maxAmmo = data.magazineSize;
        currentAmmo = maxAmmo;
        EventBus.Instance.GunAmmoChange?.Invoke(this);

    }

    public void UseAmmo()
    {
        if (currentAmmo <= 0) return;
        currentAmmo--;
        EventBus.Instance.GunAmmoChange?.Invoke(this);
    }

    public bool HasAmmo()
    {
        return currentAmmo > 0;
    }

    public bool canReload()
    {
        return currentAmmo < maxAmmo;
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        EventBus.Instance.GunAmmoChange?.Invoke(this);
    }
}
