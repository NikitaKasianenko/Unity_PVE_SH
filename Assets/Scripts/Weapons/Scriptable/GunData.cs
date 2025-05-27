using UnityEngine;


[CreateAssetMenu(fileName = "NewGunData", menuName = "Gun/GunData")]
public class GunData : ScriptableObject
{

    public string gunName;
    public LayerMask targerLayerMask;

    [Header("Fire Config")]
    public float shootingRange;
    public float fireRate;
    public float damage;
    public bool isAutomatic;


    [Header("Reload Config")]
    public int magazineSize;
    public float reloadTime;

    [Header("Recoil Config")]
    public Vector2[] recoilPoints;

    [Header("SFX")]
    public AudioClip aimSFX;
    public AudioClip reloadSFX;
    public AudioClip shootSFX;
    public AudioClip shootEmptySFX;
    public AudioClip fireModeSFX;
    public AudioClip weildSFX;

}
