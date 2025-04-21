using UnityEngine;

public class CharacterRecoil : MonoBehaviour
{
    public float recoverySpeed = 5f; 
    public float maxHorizontalRecoil = 20f; 
    public float recoilSmoothTime = 0.1f; 
    public float RecoveryAmountMultiplier = 1f;

    private float currentHorizontalRecoil = 0f;
    private float targetRecoil = 0f; 
    private float recoilVelocity = 0f; 

    public void AddHorizontalRecoil(float horizontalRecoil)
    {
        horizontalRecoil *= RecoveryAmountMultiplier;
        targetRecoil += horizontalRecoil;
        targetRecoil = Mathf.Clamp(targetRecoil, -maxHorizontalRecoil, maxHorizontalRecoil);
    }

    private void LateUpdate()
    {
        
        currentHorizontalRecoil = Mathf.SmoothDamp(currentHorizontalRecoil, targetRecoil, ref recoilVelocity, recoilSmoothTime);

        targetRecoil = Mathf.Lerp(targetRecoil, 0f, recoverySpeed * Time.deltaTime);
       
        Quaternion currentRotation = transform.localRotation;
        Quaternion recoilRotation = Quaternion.Euler(0f, currentHorizontalRecoil, 0f);

        transform.localRotation = currentRotation * recoilRotation;
    }
}