using UnityEngine;

public class Sway : MonoBehaviour
{
    public float swayClamp = 0.09f;
    public float smoothing = 5f;

    public float jumpBobAmount = 0.05f;
    public float landingBobAmount = 0.1f;
    public float bobSpeed = 6f;
    public float landingVelocityThreshold = -5f;

    private Vector3 origin;
    private float bobOffset = 0f;
    private float bobVelocity = 0f;
    private float verticalVel;

    private CharacterController characterController;

    private void Start()
    {
        origin = transform.localPosition;
        characterController = GetComponentInParent<CharacterController>();
    }

    private void Update()
    {
        JumpSway();

        bobOffset = Mathf.SmoothDamp(bobOffset, 0f, ref bobVelocity, 1f / bobSpeed);

        Vector2 input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        input.x = Mathf.Clamp(input.x, -swayClamp, swayClamp);
        input.y = Mathf.Clamp(input.y, -swayClamp, swayClamp);

        Vector3 mouseOffset = new Vector3(-input.x, -input.y, 0f);
        Vector3 totalOffset = mouseOffset + new Vector3(0f, bobOffset, 0f);

        transform.localPosition = Vector3.Lerp(transform.localPosition, origin + totalOffset, smoothing * Time.deltaTime);
    }

    private void JumpSway()
    {
        verticalVel = characterController.velocity.y;

        if (verticalVel > 0.1f)
        {
            StartBob(jumpBobAmount);
        }
        else if (verticalVel < landingVelocityThreshold)
        {
            StartBob(landingBobAmount);
        }
    }

    private void StartBob(float amount)
    {
        bobOffset = amount * verticalVel;
    }
}
