using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    private Vector2[] recoilPoints;
    private int currentRecoilIndex = 0;
    public float recoilResetDelay = 0.5f;
    private float lastShotTime = 0f;

    [SerializeField] CameraRecoil camRecoil;
    [SerializeField] CharacterRecoil characterRecoil;

    private void OnEnable()
    {
        EventBus.Instance.GunDataInit += InitializeRecoil;
        EventBus.Instance.ApplyRecoil += ApplyRecoil;

    }

    private void OnDisable()
    {
        EventBus.Instance.GunDataInit -= InitializeRecoil;
        EventBus.Instance.ApplyRecoil -= ApplyRecoil;
    }

    private void InitializeRecoil(GunData recoil)
    {
        if (recoil != null)
        {
            recoilPoints = recoil.recoilPoints;
        }
        else
        {
            Debug.LogError("Recoil points are not set in GunData. Please check the configuration.");
        }
    }

    public void ApplyRecoil()
    {
        if (Time.time - lastShotTime > recoilResetDelay)
        {
            currentRecoilIndex = 0;
        }
        lastShotTime = Time.time;

        Vector2 recoil = recoilPoints[Mathf.Min(currentRecoilIndex, recoilPoints.Length - 1)];

        camRecoil.AddVerticalRecoil(recoil.y);
        characterRecoil.AddHorizontalRecoil(recoil.x);

        currentRecoilIndex++;
    }
}
