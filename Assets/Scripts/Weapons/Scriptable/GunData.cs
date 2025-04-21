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


    [Header("Reload COnfig")]
    public int magazineSize;
    public float reloadTime;


    //[Header("Recoil Setting")]
    //public float recoilAmount;
    //public Vector2 recoilMax;
    //public float recoilSpeed;
    //public float ResetRecoilSpeed;
    // change to RecoilPoints

}
