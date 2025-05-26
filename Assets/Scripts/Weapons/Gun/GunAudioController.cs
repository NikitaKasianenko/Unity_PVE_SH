using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class GunAudioController : MonoBehaviour
{
    [Header("SFX Clips")]

    private AudioClip aimSFX, reloadSFX, shootSFX, shootEmptySFX, fireModeSFX, weildSFX;

    [SerializeField] private AudioSource _audioSource;
    private Coroutine _fadeOutCoroutine;

    private void Awake()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponentInParent<AudioSource>();
        }

    }

    private void OnEnable()
    {
        EventBus.Instance.GunDataInit += SetClips;
        EventBus.Instance.GunWeild += PlayWeild;
        EventBus.Instance.GunFireSound += PlayShoot;
        EventBus.Instance.GunEmptySound += PlayEmpty;
        EventBus.Instance.GunReloadSound += PlayReload;
        EventBus.Instance.GunAimSound += PlayAim;
        EventBus.Instance.GunFireModeToggleSound += PlayFireModeToggle;
        EventBus.Instance.GunChangeEnd += OnGunChange;

    }

    private void OnGunChange()
    {
        _audioSource.Stop();
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        float startVolume = _audioSource.volume;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float factor = Mathf.Pow(1f - (time / duration), 2f);
            _audioSource.volume = startVolume * factor;
            yield return null;
        }

        _audioSource.Stop();
        _audioSource.volume = startVolume;
    }

    public void SetClips(GunData data)
    {
        aimSFX = data.aimSFX;
        reloadSFX = data.reloadSFX;
        shootSFX = data.shootSFX;
        shootEmptySFX = data.shootEmptySFX;
        fireModeSFX = data.fireModeSFX;
        weildSFX = data.weildSFX;
    }

    public void PlayShoot() => _audioSource.PlayOneShot(shootSFX);
    public void PlayEmpty() => _audioSource.PlayOneShot(shootEmptySFX);
    public void PlayReload() => _audioSource.PlayOneShot(reloadSFX);
    public void PlayAim() => _audioSource.PlayOneShot(aimSFX);
    public void PlayWeild() => _audioSource.PlayOneShot(weildSFX);
    public void PlayFireModeToggle() => _audioSource.PlayOneShot(fireModeSFX);

}
