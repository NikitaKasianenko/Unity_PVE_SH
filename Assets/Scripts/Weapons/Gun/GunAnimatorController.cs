using UnityEngine;

public class GunAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    private int _reloadHash, _fireHash, _aimHash, _runHash, _idleHash;

    private void Awake()
    {
        _reloadHash = Animator.StringToHash("Reload");
        _fireHash = Animator.StringToHash("Fire");
        _aimHash = Animator.StringToHash("Aim");
        _runHash = Animator.StringToHash("Run");
        _idleHash = Animator.StringToHash("Idle");
    }

    public void TriggerReload() => _anim.SetTrigger(_reloadHash);
    public void SetFiring(bool value) => _anim.SetBool(_fireHash, value);
    public void SetAiming(bool value) => _anim.SetBool(_aimHash, value);
    public void SetRunning(bool value) => _anim.SetBool(_runHash, value);
    public void ToggleIdle(bool value) => _anim.SetBool(_idleHash, value);
}
