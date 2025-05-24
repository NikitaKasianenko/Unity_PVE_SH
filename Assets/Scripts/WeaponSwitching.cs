using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public GameObject[] weapons;
    [SerializeField] int currentWeapon = 0;
    void Start()
    {
        SelectWeapon(currentWeapon);
    }
    private void OnEnable()
    {
        EventBus.Instance.SelectWeaponInput += SelectWeapon;
    }
    void SelectWeapon(int index)
    {

        if (index < 0 || index >= weapons.Length)
        {
            return;
        }


        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == index)
            {
                weapon.gameObject.SetActive(true);
                currentWeapon = index;
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }


    }
}
