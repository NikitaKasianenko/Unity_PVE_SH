using TMPro;
using UnityEngine;

public class GunUI : MonoBehaviour
{
    [Header("UI References")]
    public GunAmmo gunAmmo;
    public TMP_Text ammoText;

    private void Update()
    {
        if (gunAmmo != null && ammoText != null)
            ammoText.text = $"{gunAmmo.currentAmmo} / {gunAmmo.maxAmmo}";
    }
}
