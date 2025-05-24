using UnityEngine;

public class GunAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    private int _reloadHash, _fireHash, _aimHash, _runHash, _idleHash;

    public GunAnimatorController()
    {
        InitializeHashes();
        EventBus.Instance.GunReload += TriggerReload;
        EventBus.Instance.GunFire += SetFiring;
        EventBus.Instance.GunAim += SetAiming;
        EventBus.Instance.GunIdle += ToggleIdle;

    }

    private void InitializeHashes()
    {
        _reloadHash = Animator.StringToHash("Reload");
        _fireHash = Animator.StringToHash("Fire");
        _aimHash = Animator.StringToHash("Aim");
        _runHash = Animator.StringToHash("Run");
        _idleHash = Animator.StringToHash("Idle");
    }

    private void Update()
    {
        EventBus.Instance.ReloadAnimState?.Invoke(WaitForReloadAnimStart());
        EventBus.Instance.IsRunning?.Invoke(IsRunning());
    }

    public void TriggerReload() => _anim.SetTrigger(_reloadHash);
    public void SetFiring(bool value) => _anim.SetBool(_fireHash, value);
    public bool IsFiring() => _anim.GetBool(_fireHash);
    public void SetAiming(bool value) => _anim.SetBool(_aimHash, value);
    public bool IsAiming() => _anim.GetBool(_aimHash);
    public void SetRunning(bool value) => _anim.SetBool(_runHash, value);
    public bool IsRunning() => _anim.GetBool(_runHash);
    public void ToggleIdle(bool value) => _anim.SetBool(_idleHash, value);
    public bool WaitForReloadAnimStart()
    {
        return _anim.GetCurrentAnimatorStateInfo(0).shortNameHash == _reloadHash;
    }

}
