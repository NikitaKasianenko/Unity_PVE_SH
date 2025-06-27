using UnityEngine;

public class Pistol : Gun
{
    public override void Shoot()
    {

        GameObject bullet = Instantiate(gunData.bulletPrefab, rayCaster.position, rayCaster.rotation);
        bullet.GetComponent<Bullet>().Initialize(rayCaster.forward, gunData.damage, gunData.bulletSpeed, gunData.targerLayerMask);

    }
}
