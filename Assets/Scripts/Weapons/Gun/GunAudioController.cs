using UnityEngine;

public class GunAudioController : MonoBehaviour
{
    [Header("SFX Clips")]

    public AudioClip aimSFX;
    public AudioClip reloadSFX;
    public AudioClip shootSFX;
    public AudioClip shootEmptySFX;

    [SerializeField] private AudioSource _audio;
    private bool _emptyPlaying;
    private bool _modeSwitchPlaying;

    public void PlayShoot() => _audio.PlayOneShot(shootSFX);

    public void PlayEmpty()
    {
        if (_emptyPlaying) return;
        _emptyPlaying = true;
        _audio.PlayOneShot(shootEmptySFX);
        Invoke(nameof(ResetEmpty), shootEmptySFX.length);
    }

    public void PlayReload() => _audio.PlayOneShot(reloadSFX);

    public void PlayAim() => _audio.PlayOneShot(aimSFX);

    public void PlayFireModeToggle(float delay)
    {
        if (_modeSwitchPlaying) return;
        _modeSwitchPlaying = true;
        _audio.PlayOneShot(shootEmptySFX);
        Invoke(nameof(ResetModeSwitch), delay);
    }

    private void ResetEmpty() => _emptyPlaying = false;
    private void ResetModeSwitch() => _modeSwitchPlaying = false;
}
