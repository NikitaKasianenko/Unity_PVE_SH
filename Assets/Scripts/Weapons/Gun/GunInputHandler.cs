using UnityEngine;

public class GunInputHandler : MonoBehaviour
{

    [Header("Input Keys")]
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private KeyCode fireToggleKey = KeyCode.B;
    [SerializeField] private KeyCode idleToggleKey = KeyCode.V;


    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            EventBus.Instance.FireInput?.Invoke(true);
        }
        else
        {
            EventBus.Instance.FireInput?.Invoke(false);
        }


        if (Input.GetKey(KeyCode.Mouse1))
        {
            EventBus.Instance.AimingInput?.Invoke(true);
        }
        else
        {
            EventBus.Instance.AimingInput?.Invoke(false);
        }

        if (Input.GetKeyDown(reloadKey))
        {
            EventBus.Instance.ReloadInput?.Invoke();
        }

        if (Input.GetKeyDown(fireToggleKey))
        {
            EventBus.Instance.ToggleFireModeInput?.Invoke();
        }

        if (Input.GetKeyDown(idleToggleKey))
        {
            EventBus.Instance.ToggleIdleInput?.Invoke();
        }

    }
}
