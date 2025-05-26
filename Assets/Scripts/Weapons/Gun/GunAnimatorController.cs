using System.Collections;
using UnityEngine;

public class GunAnimatorController : MonoBehaviour
{
    public Animator _anim;
    private int _reloadHash, _fireHash, _aimHash, _runHash, _idleHash, _hideHash, _resetHash;

    private void Awake()
    {
        InitializeHashes();
    }

    private void OnEnable()
    {
        EventBus.Instance.SetUpWeaponAnimator += SetUpAnimator;
        EventBus.Instance.GunReload += TriggerReload;
        EventBus.Instance.GunFire += SetFiring;
        EventBus.Instance.GunAim += SetAiming;
        EventBus.Instance.GunIdle += ToggleIdle;
        EventBus.Instance.GunChange += TriggerHide;
    }

    private void OnDisable()
    {
        EventBus.Instance.SetUpWeaponAnimator -= SetUpAnimator;
        EventBus.Instance.GunReload -= TriggerReload;
        EventBus.Instance.GunFire -= SetFiring;
        EventBus.Instance.GunAim -= SetAiming;
        EventBus.Instance.GunIdle -= ToggleIdle;
        EventBus.Instance.GunChange -= TriggerHide;
    }

    private void InitializeHashes()
    {
        _reloadHash = Animator.StringToHash("Reload");
        _fireHash = Animator.StringToHash("Fire");
        _aimHash = Animator.StringToHash("Aim");
        _runHash = Animator.StringToHash("Run");
        _idleHash = Animator.StringToHash("Idle");
        _hideHash = Animator.StringToHash("Hide");
        _resetHash = Animator.StringToHash("Reset");
    }

    private void Update()
    {
        EventBus.Instance.ReloadAnimState?.Invoke(WaitForReloadAnimStart());
        EventBus.Instance.IsRunning?.Invoke(IsRunning());
    }

    private void SetUpAnimator(Animator animator)
    {

        _anim = animator;
        _anim.Rebind();

        StartCoroutine(WaitForWeildAnimation());
        if (_anim.gameObject.activeInHierarchy && _anim.enabled)
        {
            _anim.Update(0f);
        }


    }

    private IEnumerator WaitForWeildAnimation()
    {
        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).IsName("weild")
        );
        EventBus.Instance.GunWeild?.Invoke();

    }



    public void TriggerReload() => _anim.SetTrigger(_reloadHash);

    public void TriggerHide()
    {
        StartCoroutine(WaitForHideAnimation());
    }

    private IEnumerator WaitForHideAnimation()
    {
        if (_anim.GetCurrentAnimatorStateInfo(0).shortNameHash == _reloadHash)
        {
            yield break;
        }

        _anim.SetTrigger(_resetHash);

        yield return new WaitUntil(() =>
            _anim.GetCurrentAnimatorStateInfo(0).shortNameHash == _resetHash
        );

        EventBus.Instance.GunChangeEnd?.Invoke();
    }

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
