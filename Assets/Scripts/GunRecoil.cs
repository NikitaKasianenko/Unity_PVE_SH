using UnityEngine;


[System.Serializable]
public class GunRecoil
{
    [SerializeField] public Vector2[] recoilPoints;
    private int currentRecoilIndex = 0;
    public float recoilResetDelay = 0.5f;
    private float lastShotTime = 0f;

    [SerializeField] CameraRecoil camRecoil;
    [SerializeField] CharacterRecoil characterRecoil;


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
