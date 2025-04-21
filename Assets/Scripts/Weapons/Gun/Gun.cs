using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Gun : MonoBehaviour
{
    [Header("Data")]
    public GunData gunData;
    [SerializeField] protected Transform rayCaster;
    [SerializeField] protected Animator animator;
    [SerializeField] private TMP_Text ammoText;
    protected AudioSource audioSource;

    [Header("Recoil")]
    [SerializeField] public GunRecoil gunRecoil;

    [Header("SFX")]
    public AudioClip aimSFX;
    public AudioClip reloadSFX;
    public AudioClip shootSFX;
    public AudioClip shootEmptySFX;
    public AudioClip fireModeSFX;

    [Header("MuzzleFlash")]
    public GameObject muzzleEffectsPrefab;
    public Transform muzzlePoint;
    public float showTime = 0.2f;

    [Header("Input Keys")]
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private KeyCode fireToggleKey = KeyCode.B;
    [SerializeField] private KeyCode idleToggleKey = KeyCode.V;

    // Анимационные параметры
    protected int reloadHashCode;
    protected int fireHashCode;
    protected int aimHashCode;
    protected int runHashCode;
    protected int idleHashCode;

    // Состояние оружия
    protected float currentAmmo;
    protected float nextTimeToFire = 0f;
    protected bool isReloading = false;
    protected bool isAiming = false;
    protected bool isIdle = false;
    protected bool isAutoMode = true;

    // Вспомогательные флаги
    protected bool hasPlayedAimSound = false;
    private bool isPlayingEmptySound = false;
    private GameObject muzzleFlashInstance;

    // Множители урона для разных частей тела
    protected Dictionary<string, float> damageMultipliers = new Dictionary<string, float>
    {
        { "Head", 5.0f },
        { "Body", 1.5f },
        { "Hand", 1.3f },
        { "Leg", 1.2f }
    };

    protected virtual void Awake()
    {
        // Получаем компоненты
        audioSource = GetComponentInParent<AudioSource>();
        if (gunRecoil == null)
            gunRecoil = new GunRecoil();

        // Кэшируем хеши анимаций
        reloadHashCode = Animator.StringToHash("Reload");
        fireHashCode = Animator.StringToHash("Fire");
        aimHashCode = Animator.StringToHash("Aim");
        runHashCode = Animator.StringToHash("Run");
        idleHashCode = Animator.StringToHash("Idle");
    }

    protected virtual void Start()
    {
        currentAmmo = gunData.magazineSize;

        // Инициализация эффекта дульного пламени
        if (muzzleEffectsPrefab != null && muzzlePoint != null)
        {
            muzzleFlashInstance = Instantiate(muzzleEffectsPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            muzzleFlashInstance.SetActive(false);
        }
    }

    public virtual void Update()
    {
        // Обновление интерфейса
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }

        if (!IsIdle())
        {
            HandleShootingInput();
        }

        HandleAimingInput();
        HandleReloadInput();
        HandleFireModeToggleInput();
        HandleIdleToggleInput();
    }

    public virtual void TryShoot()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            animator.SetBool(fireHashCode, false);
            PlayEmptySound();
            return;
        }

        if (Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + (1f / gunData.fireRate);
            PerformShot();
        }
    }

    protected virtual void PerformShot()
    {
        animator.SetBool(fireHashCode, true);

        currentAmmo--;

        ShowMuzzleFlash();
        PlayShootSound();

        Shoot();

        if (gunRecoil != null)
        {
            gunRecoil.ApplyRecoil();
        }
    }

    public abstract void Shoot();

    public virtual void TryReload()
    {
        if (!isReloading && currentAmmo < gunData.magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    protected virtual IEnumerator Reload()
    {
        isReloading = true;

        // Сбрасываем флаг стрельбы и запускаем анимацию перезарядки
        animator.SetBool(fireHashCode, false);
        animator.SetTrigger(reloadHashCode);

        // Ждем, пока анимация запустится
        float timeout = 1f;
        float elapsed = 0f;
        while (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != reloadHashCode && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        // Воспроизводим звук перезарядки
        if (reloadSFX != null)
        {
            audioSource.PlayOneShot(reloadSFX);
        }

        // Ждем окончания перезарядки
        yield return new WaitForSeconds(gunData.reloadTime);

        // Пополняем магазин
        currentAmmo = gunData.magazineSize;
        isReloading = false;
    }

    public virtual void SetAiming(bool aiming)
    {
        // Нельзя прицеливаться во время бега
        if (animator.GetBool(runHashCode))
        { return; }

        if (IsIdle())
        { animator.SetBool(aimHashCode, false); }

        isAiming = aiming;
        animator.SetBool(aimHashCode, aiming);

        // Воспроизводим звук прицеливания при первом нажатии
        if (aiming && !hasPlayedAimSound)
        {
            if (aimSFX != null)
            {
                audioSource.PlayOneShot(aimSFX);
            }
            hasPlayedAimSound = true;
        }
        else if (!aiming)
        {
            hasPlayedAimSound = false;
        }
    }

    public virtual void ToggleFireMode()
    {
        isAutoMode = !isAutoMode;

        // Воспроизводим звук переключения режима
        if (fireModeSFX != null)
        {
            audioSource.PlayOneShot(fireModeSFX);
        }
        else if (shootEmptySFX != null) // Используем звук пустого патронника, если специальный не задан
        {
            audioSource.PlayOneShot(shootEmptySFX);
        }
    }

    public virtual void SetIdleMode(bool idle)
    {
        isIdle = idle;
        animator.SetBool(idleHashCode, idle);
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

    protected virtual void PlayShootSound()
    {
        if (audioSource != null && shootSFX != null)
        {
            audioSource.PlayOneShot(shootSFX);
        }
    }

    protected virtual void PlayEmptySound()
    {
        if (audioSource != null && shootEmptySFX != null && !isPlayingEmptySound)
        {
            audioSource.PlayOneShot(shootEmptySFX);
            isPlayingEmptySound = true;

            StartCoroutine(ResetEmptySoundFlag(shootEmptySFX.length));
        }
    }

    private IEnumerator ResetEmptySoundFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlayingEmptySound = false;
    }

    protected IEnumerator ResetFireAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool(fireHashCode, false);
    }

    protected float CalculateDamage(string hitTag)
    {
        if (damageMultipliers.TryGetValue(hitTag, out float multiplier))
        {
            return gunData.damage * multiplier;
        }
        return gunData.damage;
    }



    public bool IsAutoMode() => isAutoMode;
    public bool IsAiming() => isAiming;
    public bool IsReloading() => isReloading;
    public bool IsIdle() => isIdle;
    public int GetCurrentAmmo() => Mathf.RoundToInt(currentAmmo);


    private void HandleShootingInput()
    {
        if (IsAutoMode())
        {
            if (Input.GetButton("Fire1"))
            {
                TryShoot();
            }
            else
            {
                animator.SetBool(fireHashCode, false);
            }
        }
        else if (!IsAutoMode())
        {
            if (Input.GetButtonDown("Fire1"))
            {
                TryShoot();
            }
            else
            {
                animator.SetBool(fireHashCode, false);
            }
        }
    }

    private void HandleAimingInput()
    {
        bool isAimPressed = Input.GetKey(KeyCode.Mouse1);

        if (isAimPressed != IsAiming())
        {
            SetAiming(isAimPressed);
        }
    }

    private void HandleReloadInput()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            TryReload();
        }
    }

    private void HandleFireModeToggleInput()
    {
        if (Input.GetKeyDown(fireToggleKey))
        {
            ToggleFireMode();
        }
    }

    private void HandleIdleToggleInput()
    {
        if (Input.GetKeyDown(idleToggleKey))
        {
            SetIdleMode(!IsIdle());
        }
    }
}

