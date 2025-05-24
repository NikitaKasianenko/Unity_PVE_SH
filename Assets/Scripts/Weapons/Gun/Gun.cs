using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [Header("Data")]
    public GunData gunData;
    [SerializeField] protected Transform rayCaster;


    [Header("MuzzleFlash")]
    public GameObject muzzleEffectsPrefab;
    public Transform muzzlePoint;
    public float showTime = 0.2f;

    // Состояние оружия
    protected GunAmmo gunAmmo;
    protected float nextTimeToFire = 0f;
    protected bool isReloading = false;
    protected bool isAiming = false;
    protected bool isIdle = false;
    protected bool isAutoMode = true;
    protected bool isShooting = false;
    protected bool reloadAnimStarted;
    protected bool isRunning = false; // Флаг для состояния бега
    // Вспомогательные флаги

    protected bool hasPlayedAimSound = false;
    private bool isPlayingEmptySound = false;
    private GameObject muzzleFlashInstance;
    private Animator _anim;

    protected Dictionary<string, float> damageMultipliers = new Dictionary<string, float>
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

        EventBus.Instance.SetUpWeaponAnimator?.Invoke(_anim);
        EventBus.Instance.GunDataInit?.Invoke(gunData);
        EventBus.Instance.RecoilData?.Invoke(gunData.recoilPoints);

        isAutoMode = gunData.isAutomatic;


        if (muzzleEffectsPrefab != null && muzzlePoint != null)
        {
            muzzleFlashInstance = Instantiate(muzzleEffectsPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            muzzleFlashInstance.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventBus.Instance.SetUpWeaponAnimator?.Invoke(_anim);
        EventBus.Instance.GunDataInit?.Invoke(gunData);
        EventBus.Instance.RecoilData?.Invoke(gunData.recoilPoints);

        EventBus.Instance.FireInput += HandleShootingInput;
        EventBus.Instance.AimingInput += SetAiming;
        EventBus.Instance.ReloadInput += TryReload;
        EventBus.Instance.ToggleFireModeInput += ToggleFireMode;
        EventBus.Instance.ToggleIdleInput += SetIdleMode;
        EventBus.Instance.ReloadAnimState += OnReloadAnimState;
        EventBus.Instance.IsRunning += OnRunningState;
    }


    private void OnRunningState(bool state)
    {
        isRunning = state;
    }

    public void ChangeWeapon()
    {
        EventBus.Instance.GunChange?.Invoke();
    }

    private void OnDisable()
    {
        EventBus.Instance.FireInput -= HandleShootingInput;
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
        if (isReloading)
            return;

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
        EventBus.Instance.GunEmptySound?.Invoke();
    }

    protected virtual void PerformShot()
    {
        EventBus.Instance.GunFire?.Invoke(true);
        gunAmmo.UseAmmo();

        Shoot();
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

        // Сбрасываем флаг стрельбы и запускаем анимацию перезарядки
        EventBus.Instance.GunFire?.Invoke(false);
        EventBus.Instance.GunReload?.Invoke();

        // Ждем, пока анимация запустится
        while (!reloadAnimStarted)
        {
            yield return null;
        }

        // Воспроизводим звук перезарядки
        EventBus.Instance.GunReloadSound?.Invoke();

        // Ждем окончания перезарядки
        yield return new WaitForSeconds(gunData.reloadTime);

        // Пополняем магазин
        gunAmmo.Reload();
        isReloading = false;
    }

    public virtual void SetAiming(bool aiming)
    {
        // Нельзя прицеливаться во время бега
        if (isRunning)
        { return; }

        if (isIdle)
        { EventBus.Instance.GunAim?.Invoke(false); }

        isAiming = aiming;
        EventBus.Instance.GunAim?.Invoke(aiming);

        // Воспроизводим звук прицеливания при первом нажатии
        if (aiming && !hasPlayedAimSound)
        {
            EventBus.Instance.GunAimSound?.Invoke();
            hasPlayedAimSound = true;
        }
        else if (!aiming)
        {
            hasPlayedAimSound = false;
        }
    }

    public virtual void ToggleFireMode()
    {
        if (!gunData.isAutomatic)
        {
            return;
        }

        isAutoMode = !isAutoMode;
        EventBus.Instance.GunFireModeToggleSound?.Invoke();
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

            // Запускаем все системы частиц
            foreach (ParticleSystem ps in muzzleFlashInstance.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
            }

            // Отключаем эффект через заданное время
            CancelInvoke(nameof(HideMuzzleFlash));
            Invoke(nameof(HideMuzzleFlash), showTime);
        }
    }

    private void HideMuzzleFlash()
    {
        if (muzzleFlashInstance != null)
        {
            muzzleFlashInstance.SetActive(false);
        }
    }


    protected float CalculateDamage(string hitTag)
    {
        if (damageMultipliers.TryGetValue(hitTag, out float multiplier))
        {
            return gunData.damage * multiplier;
        }
        return gunData.damage;
    }


    private void HandleShootingInput(bool mode)
    {
        if (!mode)
        {
            EventBus.Instance.GunFire?.Invoke(false);
        }

        if (isIdle || isReloading)
        {
            EventBus.Instance.GunAim?.Invoke(false);
            return;
        }

        if (mode)
        {
            TryShoot();
        }


        if (isAutoMode)
        {
            TryShoot();
        }
        else
        {
            if (!mode)
            {
                isShooting = true;
            }
            if (mode && !isShooting)
            {
                isShooting = true;
                TryShoot();
            }
        }
    }

}

