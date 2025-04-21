using UnityEngine;

public class Sway : MonoBehaviour
{
    public float swayClamp = 0.09f;
    public float smoothing = 3f;
    private Vector3 origin;

    private void Start()
    {
        origin = transform.localPosition;
    }

    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        input.x = Mathf.Clamp(input.x, -swayClamp, swayClamp);
        input.y = Mathf.Clamp(input.y, -swayClamp, swayClamp);

        Vector3 target = new Vector3(-input.x, -input.y, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, origin +  target , smoothing * Time.deltaTime);
    }

}
