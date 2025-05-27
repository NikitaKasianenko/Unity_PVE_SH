using System.Collections;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public GameObject[] weapons;
    [SerializeField] int currentWeapon = 0;
    private bool canChangeWeapon = false;

    private void OnEnable()
    {
        EventBus.Instance.SelectWeaponInput += OnSelectWeaponInput;
        EventBus.Instance.GunChangeEnd += GetEnableWeaponChange;
    }

    private void OnDisable()
    {
        EventBus.Instance.SelectWeaponInput -= OnSelectWeaponInput;
        EventBus.Instance.GunChangeEnd -= GetEnableWeaponChange;
    }

    private void OnSelectWeaponInput(int index)
    {
        if (index != currentWeapon && !(index < 0 || index >= weapons.Length))
        {
            StartCoroutine(WaitAndSelectWeapon(index));
        }
    }

    private void GetEnableWeaponChange()
    {
        canChangeWeapon = true;
    }

    private IEnumerator WaitAndSelectWeapon(int index)
    {
        EventBus.Instance.GunChange?.Invoke();
        yield return new WaitUntil(() => canChangeWeapon);
        canChangeWeapon = false;
        SelectWeaponInternal(index);
    }

    private void SelectWeaponInternal(int index)
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(i == index);
            if (i == index) currentWeapon = index;
            i++;
        }
    }
}
