using System;
using UnityEngine;

public class GunInputHandler : MonoBehaviour
{
    public bool IsAutoShooting => Input.GetButton("Fire1");
    public bool IsSemiShooting => Input.GetButtonDown("Fire1");
    public bool IsReloading => Input.GetKeyDown(KeyCode.R);
    public bool ToggleIdle => Input.GetKeyDown(KeyCode.V);
    public bool IsAiming => Input.GetKey(KeyCode.Mouse1);
    public bool ToggleFireMode => Input.GetKeyDown(KeyCode.B);
}
