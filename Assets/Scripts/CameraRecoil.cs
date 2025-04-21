using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class CameraRecoil : MonoBehaviour
{
    public float recoverySpeed = 5f;  
    public float RecoveryAmountMultiplier = 1f; 
    public float maxVerticalRecoil = 20f;  
    public float recoilSmoothTime = 0.1f;  
    private float recoveryMultiplier = 1f;  
    private float currentVerticalRecoil = 0f;

    private float targetRecoil = 0f;  
    private float recoilVelocity = 0f; 
    public bool offRecovery = false;

    [SerializeField] FirstPersonController camRecoil;
    public void AddVerticalRecoil(float verticalRecoil)
    {
        verticalRecoil *= RecoveryAmountMultiplier;

        if (!offRecovery)
        { targetRecoil += verticalRecoil; }
        else
        { targetRecoil += verticalRecoil / 10f; }

        targetRecoil = Mathf.Clamp(targetRecoil, 0f, maxVerticalRecoil);

    }

    private void LateUpdate()
    {
        recoveryMultiplier = offRecovery ? 10f : 1f;
        currentVerticalRecoil = Mathf.SmoothDamp(currentVerticalRecoil, targetRecoil, ref recoilVelocity, recoilSmoothTime);
        targetRecoil = Mathf.Lerp(targetRecoil, 0f, recoverySpeed * recoveryMultiplier * Time.deltaTime);

        Quaternion currentRotation = transform.localRotation;
        Quaternion recoilRotation = Quaternion.Euler(-currentVerticalRecoil, 0f, 0f);

        if (!offRecovery)
        { transform.localRotation = currentRotation * recoilRotation; }
        else
        { camRecoil.AddRecoil(recoilRotation); }

    }
}
