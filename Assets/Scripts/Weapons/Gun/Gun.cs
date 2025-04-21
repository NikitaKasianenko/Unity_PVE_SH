using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [Header("Data")]
    public GunData gunData;
    [SerializeField] public Transform RayCaster;
    [SerializeField] public Animator animator;
    [SerializeField] private TMP_Text AmmoText;
    protected AudioSource audioSource;

    [Header("Recoil")]
    //TODO 
    //recoil setting e.g serialize recoil points to GunData

    [SerializeField] public GunRecoil gunRecoil;

    [Header("SFX")]
    public AudioClip AimSFX;
    public AudioClip ReloadSFX;
    public AudioClip ShootSFX;
    public AudioClip ShootEmptySFX;
    [Header("MuzzleFlash")]
    public GameObject muzzleEffectsPrefab;
    public Transform muzzlePoint;
    public float showTime = 0.2f;
    private GameObject effectsInstance;
    private bool isEffectActive;


    public bool Cursor = true;

    protected int ReloadHashCode;
    protected int FireHashCode;
    protected int AimHashCode;
    protected int RunHashCode;
    protected int IdleHashCode;
    
    private float currentAmmo = 0f;
    private float nextTimeToFire = 0f; 
    private float nextTimeToPlayAimSFX = 0f;

    private bool isReloading = false;
    protected bool isIdle = false;
    protected bool canShoot = false;
    protected bool hasPlayedSound = false;
    private bool isPlayingEmptySound = false;
    private bool isPlayingFireModeSound = false;

    protected bool FireMode = true;

    Dictionary<string, float> damageKef = new Dictionary<string, float>
    {
        { "Hand", 1.3f },
        { "Leg", 1.2f },
        { "Head", 5.0f },
        { "Body", 1.5f }
    };

    private void Awake()
    {
        audioSource = GetComponentInParent<AudioSource>();
        if(gunRecoil == null)
            gunRecoil = new GunRecoil();

        ReloadHashCode = Animator.StringToHash("Reload");
        FireHashCode = Animator.StringToHash("Fire");
        AimHashCode = Animator.StringToHash("Aim");
        RunHashCode = Animator.StringToHash("Run");
        IdleHashCode = Animator.StringToHash("Idle");
    }

    private void Start()
    {
        currentAmmo = gunData.magazineSize;
        if (!Cursor)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        effectsInstance = Instantiate(muzzleEffectsPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
        effectsInstance.SetActive(false);
    }

    public virtual void Update()
    {
        AmmoText.text = currentAmmo.ToString();
    }

    public void TryReload()
    {
        if(!isReloading && currentAmmo < gunData.magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {

        isReloading = true;
        canShoot = false;

        animator.SetBool(FireHashCode, false);
        animator.SetTrigger(ReloadHashCode);

        float timeout = 1f;
        float elapsed = 0f;

        while (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != ReloadHashCode && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        audioSource.PlayOneShot(ReloadSFX);
        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.magazineSize;
        isReloading = false;
        canShoot = true;
    }

    public void TryShoot()
    {
        if ((isReloading))
        {
            return;
        }
        if(currentAmmo <= 0f)
        {
            canShoot = false;
            SetShootAnimation();
            if(Time.time >= nextTimeToFire && !isPlayingEmptySound && !animator.GetBool(RunHashCode)){
                audioSource.PlayOneShot(ShootEmptySFX);
                isPlayingEmptySound = true;
                StartCoroutine(ResetEmptySound(ShootEmptySFX.length));
            }
            return;
        }
        if(Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + (1 / gunData.fireRate);
            HandleShoot();
        }
    }

    private IEnumerator ResetEmptySound(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlayingEmptySound = false;
    }

    private void HandleShoot()
    {
        animator.SetBool(FireHashCode, true);
        currentAmmo--;

        muzzleFlash();
        Shoot();
        audioSource.PlayOneShot(ShootSFX);
        StartCoroutine(SetShootAnimation());
    }

    private void muzzleFlash()
    {
        effectsInstance.SetActive(true);
        foreach (var ps in effectsInstance.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        CancelInvoke(nameof(HideEffects));
        Invoke(nameof(HideEffects), showTime);
    }

    public IEnumerator SetShootAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool(FireHashCode, canShoot);

    }
    public void SetIdleAnimation()
    {
        isIdle = !isIdle;
        animator.SetBool(IdleHashCode, isIdle);
    }

    public float DetermineDamage(string tag)
    {
        if(damageKef.ContainsKey(tag))
        {
            return gunData.damage * damageKef[tag];
        }
        return 1.0f;
    }

    void HideEffects()
    {
        effectsInstance.SetActive(false);
    }


    public IEnumerator FireModeSound(float delay)
    {
        if (isPlayingFireModeSound)
            yield break;

        isPlayingFireModeSound = true;
        audioSource.PlayOneShot(ShootEmptySFX);
        yield return new WaitForSeconds(delay);
        isPlayingFireModeSound = false;
    }

    public IEnumerator AimSound(float delay)
    {
        if(Time.time >= nextTimeToPlayAimSFX)
        {
            nextTimeToFire = Time.time + (1 / 2);
            audioSource.PlayOneShot(AimSFX);

        }

        yield return new WaitForSeconds(delay);

    }

    public abstract void Shoot();
     

}
