using System;
using UnityEngine;
public class EventBus
{
    public static EventBus _instance;

    public static EventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EventBus();
            }
            return _instance;
        }
    }

    // inputHandler 
    public Action<bool> FireInput;
    public Action<bool> AimingInput;
    public Action ReloadInput;
    public Action ToggleIdleInput;
    public Action ToggleFireModeInput;

    //gun
    public Action GunReload;
    public Action<bool> GunFire;
    public Action<bool> GunAim;
    public Action<bool> GunIdle;

    //animator
    public Action<Animator> SetUpWeaponAnimator;
    public Action<bool> ReloadAnimState;
    public Action<bool> IsRunning;

    //GunAudioController
    public Action<GunData> GunDataInit;

    public Action GunFireSound;
    public Action GunEmptySound;
    public Action GunReloadSound;
    public Action GunAimSound;
    public Action GunFireModeToggleSound;

    //Recoil 
    public Action<float> RecoilX;
    public Action<float> RecoilY;
    public Action<Vector2[]> RecoilData;
    public Action ApplyRecoil;

    //GunAmmoChange
    public Action<GunAmmo> GunAmmoChange;
}
