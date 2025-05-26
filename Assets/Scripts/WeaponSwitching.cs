using System.Collections;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public GameObject[] weapons;
    [SerializeField] int currentWeapon = 0;
    private bool canChangeWeapon = false;

    void Start()
    {
        SelectWeaponInternal(currentWeapon);
    }

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
        StartCoroutine(WaitAndSelectWeapon(index));
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
        if (index < 0 || index >= weapons.Length)
            return;

        int i = 0;
        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(i == index);
            if (i == index) currentWeapon = index;
            i++;
        }
    }
}
