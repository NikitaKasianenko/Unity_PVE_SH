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
    public Action<bool> AutoFireInput;
    public Action<bool> SemiFireInput;
    public Action<bool> AimingInput;
    public Action ReloadInput;
    public Action ToggleIdleInput;
    public Action ToggleFireModeInput;
    public Action<int> SelectWeaponInput;

    //gun
    public Action GunReload;
    public Action<bool> GunFire;
    public Action<bool> GunAim;
    public Action<bool> GunIdle;

    //animator
    public Action<Animator> SetUpWeaponAnimator;
    public Action<bool> ReloadAnimState;
    public Action<bool> IsRunning;
    public Action GunChange;
    public Action GunChangeEnd;

    //GunAudioController
    public Action<GunData> GunDataInit;

    //SFX
    public Action GunFireSound;
    public Action GunEmptySound;
    public Action GunReloadSound;
    public Action GunAimSound;
    public Action GunFireModeToggleSound;
    public Action GunWeild;

    //Recoil 
    public Action<float> RecoilX;
    public Action<float> RecoilY;
    public Action ApplyRecoil;

    //GunAmmoChange
    public Action<GunAmmo> GunAmmoChange;
}
