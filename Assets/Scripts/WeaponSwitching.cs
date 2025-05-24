using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public GameObject[] weapons;
    [SerializeField] int currentWeapon = 1;
    void Start()
    {
        SelectWeapon(currentWeapon);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectWeapon(2);
        }
    }
    void SelectWeapon(int index)
    {
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
