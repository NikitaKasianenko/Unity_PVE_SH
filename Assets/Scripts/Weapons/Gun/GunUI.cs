using TMPro;
using UnityEngine;

public class GunUI : MonoBehaviour
{
    [Header("UI References")]
    public GunAmmo gunAmmo;
    public TMP_Text ammoText;


    private void OnEnable()
    {
        EventBus.Instance.GunAmmoChange += UpdateAmmoUI;
    }

    private void OnDisable()
    {
        EventBus.Instance.GunAmmoChange -= UpdateAmmoUI;
    }


    private void UpdateAmmoUI(GunAmmo ammo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{ammo.currentAmmo}/{ammo.maxAmmo}";
        }
        else
        {
            Debug.LogWarning("Ammo text UI element is not assigned.");
        }

    }

}
