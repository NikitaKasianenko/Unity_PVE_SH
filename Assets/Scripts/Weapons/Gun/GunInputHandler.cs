using UnityEngine;

public class GunInputHandler : MonoBehaviour
{
    public bool IsAutoShooting() => UnityEngine.Input.GetButton("Fire1");
    public bool IsSemiShooting() => UnityEngine.Input.GetButtonDown("Fire1");
    public bool IsReloading() => UnityEngine.Input.GetKeyDown(KeyCode.R);
    public bool ToggleIdle() => UnityEngine.Input.GetKeyDown(KeyCode.V);
    public bool IsAiming() => UnityEngine.Input.GetKey(KeyCode.Mouse1);
    public bool ToggleFireMode() => UnityEngine.Input.GetKeyDown(KeyCode.B);
}
