using System;
using UnityEngine;

[Serializable]
public class GunAudioController : MonoBehaviour
{
    [Header("SFX Clips")]

    private AudioClip aimSFX, reloadSFX, shootSFX, shootEmptySFX, fireModeSFX;

    [SerializeField] private AudioSource _audioSource;
    private bool _emptyPlaying;
    private bool _modeSwitchPlaying;

    private void Awake()
    {
        _audioSource = GetComponentInParent<AudioSource>();

    }

    private void Start()
    {
        EventBus.Instance.GunDataInit += SetClips;
        EventBus.Instance.GunFireSound += PlayShoot;
        EventBus.Instance.GunEmptySound += PlayEmpty;
        EventBus.Instance.GunReloadSound += PlayReload;
        EventBus.Instance.GunAimSound += PlayAim;
        EventBus.Instance.GunFireModeToggleSound += PlayFireModeToggle;

    }
    public void SetClips(GunData data)
    {
        aimSFX = data.aimSFX;
        reloadSFX = data.reloadSFX;
        shootSFX = data.shootSFX;
        shootEmptySFX = data.shootEmptySFX;
        fireModeSFX = data.fireModeSFX;
    }

    public void PlayShoot() => _audioSource.PlayOneShot(shootSFX);

    public void PlayEmpty()
    {
        if (_emptyPlaying) return;
        _emptyPlaying = true;
        _audioSource.PlayOneShot(shootEmptySFX);
        Invoke(nameof(ResetEmpty), shootEmptySFX.length);
    }

    public void PlayReload() => _audioSource.PlayOneShot(reloadSFX);

    public void PlayAim() => _audioSource.PlayOneShot(aimSFX);

    public void PlayFireModeToggle()
    {
        if (_modeSwitchPlaying) return;
        _modeSwitchPlaying = true;
        _audioSource.PlayOneShot(fireModeSFX);
        Invoke(nameof(ResetModeSwitch), fireModeSFX.length);
    }

    private void ResetEmpty() => _emptyPlaying = false;
    private void ResetModeSwitch() => _modeSwitchPlaying = false;
}
