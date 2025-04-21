using UnityEngine;

public class GunMuzzleFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public GameObject flashPrefab;
    public Transform muzzlePoint;
    public float showTime = 0.1f;

    private GameObject _instance;

    private void Start()
    {
        _instance = Instantiate(flashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
        _instance.SetActive(false);
    }

    public void ShowFlash()
    {
        _instance.SetActive(true);
        foreach (var ps in _instance.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }

        CancelInvoke(nameof(HideFlash));
        Invoke(nameof(HideFlash), showTime);
    }

    private void HideFlash() => _instance.SetActive(false);
}
