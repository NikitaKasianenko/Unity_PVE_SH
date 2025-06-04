using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [Header("Data")]
    public GunData gunData;
    [SerializeField] protected Transform rayCaster;


    [Header("MuzzleFlash")]
    [SerializeField] private GameObject muzzleEffectsPrefab;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private float showTime = 0.2f;

    [Header("SFX-delays")]
    [SerializeField] private float AimingDelay = 0.5f;
    [SerializeField] private float EmptyShootDelay = 0.5f;
    [SerializeField] private float ToggleModeDelay = 0.5f;
    private float TimeToAimingSound = 0f, TimeToEmptyShootSound = 0.0f, TimeToToggleModeSound = 0.0f;


    //Gun ammo
    protected GunAmmo gunAmmo;
    // weapon state

    protected float nextTimeToFire = 0f;
    protected bool isReloading = false;
    protected bool isAiming = false;
    protected bool isIdle = false;
    protected bool isAutoMode = true;
    protected bool isShooting = false;
    protected bool reloadAnimStarted;
    protected bool isRunning = false;


    protected bool hasPlayedAimSound = false;
    private GameObject muzzleFlashInstance;
    private Animator _anim;

    protected Dictionary<string, float> damageMultiplier = new Dictionary<string, float>
    {
        { "Head", 5.0f },
        { "Body", 1.5f },
        { "Hand", 1.3f },
        { "Leg", 1.2f }
    };


    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        gunAmmo = new GunAmmo(gunData);

        EventBus.Instance.GunDataInit?.Invoke(gunData);
        EventBus.Instance.SetUpWeaponAnimator?.Invoke(_anim);

        isAutoMode = gunData.isAutomatic;


        if (muzzleEffectsPrefab != null && muzzlePoint != null)
        {
            muzzleFlashInstance = Instantiate(muzzleEffectsPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            muzzleFlashInstance.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Debug.Log("Gun enabled: " + gameObject.name + " " + transform.localPosition);
        EventBus.Instance.SetUpWeaponAnimator?.Invoke(_anim);
        EventBus.Instance.GunDataInit?.Invoke(gunData);
        if (gunAmmo != null)
        {
            EventBus.Instance.GunAmmoChange.Invoke(gunAmmo);
        }
        EventBus.Instance.GunChange += ChangeWeapon;
        EventBus.Instance.AutoFireInput += HandleAutoShootingInput;
        EventBus.Instance.SemiFireInput += HandleSemiShootingInput;
        EventBus.Instance.AimingInput += SetAiming;
        EventBus.Instance.ReloadInput += TryReload;
        EventBus.Instance.ToggleFireModeInput += ToggleFireMode;
        EventBus.Instance.ToggleIdleInput += SetIdleMode;
        EventBus.Instance.ReloadAnimState += OnReloadAnimState;
        EventBus.Instance.IsRunning += OnRunningState;

        reloadAnimStarted = false;
        isReloading = false;

    }


    private void OnRunningState(bool state)
    {
        isRunning = state;
    }

    public void ChangeWeapon()
    {

    }

    private void OnDisable()
    {
        EventBus.Instance.GunChange -= ChangeWeapon;
        EventBus.Instance.AutoFireInput -= HandleAutoShootingInput;
        EventBus.Instance.SemiFireInput -= HandleSemiShootingInput;
        EventBus.Instance.AimingInput -= SetAiming;
        EventBus.Instance.ReloadInput -= TryReload;
        EventBus.Instance.ToggleFireModeInput -= ToggleFireMode;
        EventBus.Instance.ToggleIdleInput -= SetIdleMode;
        EventBus.Instance.ReloadAnimState -= OnReloadAnimState;
        EventBus.Instance.IsRunning -= OnRunningState;
    }

    public virtual void Update()
    {
    }

    public virtual void TryShoot()
    {
        if (!gunAmmo.HasAmmo())
        {
            NoAmmoEvent();
            return;
        }

        if (Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + (1f / gunData.fireRate);
            PerformShot();
        }
    }

    private void NoAmmoEvent()
    {
        EventBus.Instance.GunFire?.Invoke(false);
        if (Time.time - EmptyShootDelay > TimeToEmptyShootSound)
        {
            EventBus.Instance.GunEmptySound?.Invoke();
            TimeToEmptyShootSound = Time.time;
        }
    }

    protected virtual void PerformShot()
    {
        EventBus.Instance.GunFire?.Invoke(true);
        Shoot();
        gunAmmo.UseAmmo();
        ShowMuzzleFlash();
        EventBus.Instance.ApplyRecoil?.Invoke();
        EventBus.Instance.GunFireSound?.Invoke();
    }

    private void OnReloadAnimState(bool isStarted)
    {
        reloadAnimStarted = isStarted;
    }

    public abstract void Shoot();

    public virtual void TryReload()
    {
        if (!isReloading && gunAmmo.canReload())
        {
            StartCoroutine(Reload());
        }
    }

    protected virtual IEnumerator Reload()
    {
        isReloading = true;

        EventBus.Instance.GunReload?.Invoke();

        while (!reloadAnimStarted)
        {
            yield return null;
        }

        EventBus.Instance.GunReloadSound?.Invoke();
        yield return new WaitForSeconds(gunData.reloadTime);

        gunAmmo.Reload();
        isReloading = false;
    }

    public virtual void SetAiming(bool aiming)
    {
        if (!CanOperate())
        {
            EventBus.Instance.GunAim?.Invoke(false);
            return;
        }

        isAiming = aiming;
        EventBus.Instance.GunAim?.Invoke(aiming);

        if (aiming && !hasPlayedAimSound && Time.time - AimingDelay > TimeToAimingSound)
        {
            EventBus.Instance.GunAimSound?.Invoke();
            TimeToAimingSound = Time.time;
            hasPlayedAimSound = true;
        }
        if (!aiming)
        {
            hasPlayedAimSound = false;
        }
    }

    private bool CanOperate()
    {
        return !(isIdle || isReloading || isRunning);
    }

    public virtual void ToggleFireMode()
    {
        if (!gunData.isAutomatic)
        {
            return;
        }

        if (Time.time - ToggleModeDelay > TimeToToggleModeSound)
        {
            isAutoMode = !isAutoMode;
            TimeToToggleModeSound = Time.time;
            EventBus.Instance.GunFireModeToggleSound?.Invoke();
        }

    }

    public virtual void SetIdleMode()
    {
        isIdle = !isIdle;
        EventBus.Instance.GunIdle?.Invoke(isIdle);
    }



    protected virtual void ShowMuzzleFlash()
    {
        if (muzzleFlashInstance != null)
        {
            muzzleFlashInstance.SetActive(true);
            foreach (ParticleSystem ps in muzzleFlashInstance.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
            }
        }
    }
    protected float CalculateDamage(string hitTag)
    {
        if (damageMultiplier.TryGetValue(hitTag, out float multiplier))
        {
            return gunData.damage * multiplier;
        }
        return gunData.damage;
    }


    private void HandleAutoShootingInput(bool mode)
    {
        if (!isAutoMode)
        {
            return;
        }

        HandleShooting(mode);

    }
    private void HandleSemiShootingInput(bool mode)
    {
        if (isAutoMode)
        {
            return;
        }

        HandleShooting(mode);

    }

    private void HandleShooting(bool mode)
    {
        if (!CanOperate() || !mode)
        {
            EventBus.Instance.GunFire?.Invoke(false);
            return;
        }

        if (mode)
        {
            TryShoot();
        }
    }


}

