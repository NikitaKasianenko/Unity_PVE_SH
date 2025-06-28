using UnityEngine;
using UnityStandardAssets.Utility;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private CharacterController characterContoller;
    public Camera camera;
    private Vector3 m_OriginalCameraPosition;
    [SerializeField][Range(0f, 1f)] private float m_RunstepLenghten;
    private Animator animator;
    [SerializeField] private CurveControlledBob headBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();
    private Quaternion characterTargetRot;
    private Quaternion cameraTargetRot;

    [SerializeField] private AudioSource footStepAudioSource;


    [Header("Input")]
    [SerializeField] private float mouseSensitivity = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float sprintTransitSpeed = 5f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;

    private bool previousGrounded = true;
    private bool isWalking = true;
    private float verticalVelocity;
    private float currentSpeed;
    private float currentSpeedMultiplier;


    [Header("Footstep Settings")]
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private float stepInterval = 1f;
    [SerializeField] private float stepIntervalHeadBob = 4f;

    private float nextStepTimer = 0;

    [Header("SFX")]
    [SerializeField] private AudioClip[] groundFootsteps;
    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip[] gravelFootsteps;


    private float moveInput;
    private float turnInput;
    private float mouseX;
    private float mouseY;

    private void OnEnable()
    {
        EventBus.Instance.SetUpWeaponAnimator += SetUpAnimator;
    }


    private void Start()
    {
        camera = Camera.main;
        characterContoller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraTargetRot = camera.transform.localRotation;
        characterTargetRot = transform.localRotation;
        m_OriginalCameraPosition = camera.transform.localPosition;
        headBob.Setup(camera, stepIntervalHeadBob);
    }

    private void SetUpAnimator(Animator anim)
    {
        animator = anim;
    }

    private void AnimatorSet()
    {
        animator.SetBool("Run", !isWalking);
        float magnitude = characterContoller.velocity.magnitude;
        float mult = !isWalking ? 2f : magnitude > 0 ? 1.3f : 0.85f;
        animator.SetFloat("Mult", mult);
        animator.SetFloat("Mag", magnitude);
    }

    private void Update()
    {
        InputManagement();
        Movement();
        PlayFootstepSound();
        AnimatorSet();

    }

    private void FixedUpdate()
    {
        UpdateCameraPosition(currentSpeedMultiplier);
    }

    private void LateUpdate()
    {
        //CameraBob();
    }

    private void Movement()
    {
        GroundMovement();
        Turn();

    }



    private void GroundMovement()
    {
        Vector3 moveDirection = transform.right * turnInput + transform.forward * moveInput;
        moveDirection.y = 0;

        moveDirection.Normalize();
        moveDirection *= currentSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeedMultiplier = sprintSpeedMultiplier;
            isWalking = false;
        }
        else
        {
            currentSpeedMultiplier = 1f;
            isWalking = true;
        }

        currentSpeedMultiplier = Input.GetKey(KeyCode.LeftShift) ? sprintSpeedMultiplier : 1f;
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * currentSpeedMultiplier, sprintTransitSpeed * Time.deltaTime);
        moveDirection.y = VerticalForceCalculation();
        characterContoller.Move(moveDirection * Time.deltaTime);


        if (!previousGrounded && characterContoller.isGrounded)
        {
            StartCoroutine(jumpBob.DoBobCycle());
        }

        previousGrounded = characterContoller.isGrounded;




    }

    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (characterContoller.velocity.magnitude > 0 && characterContoller.isGrounded)
        {
            camera.transform.localPosition = headBob.DoHeadBob(characterContoller.velocity.magnitude +
                                  (speed * (isWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = camera.transform.localPosition;
            newCameraPosition.y = camera.transform.localPosition.y - jumpBob.Offset();
        }
        else
        {
            newCameraPosition = camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - jumpBob.Offset();
        }
        camera.transform.localPosition = newCameraPosition;
    }


    private void Turn()
    {
        float yRot = mouseY * mouseSensitivity;
        float xRot = mouseX * mouseSensitivity;

        characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
        cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
        cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

        transform.localRotation = characterTargetRot;
        camera.transform.localRotation = cameraTargetRot;

    }

    public void AddRecoil(Quaternion recoilAmount)
    {
        cameraTargetRot *= recoilAmount;
    }


    private void PlayFootstepSound()
    {
        if (characterContoller.isGrounded && characterContoller.velocity.magnitude > 0.1f)
        {
            if (Time.time >= nextStepTimer)
            {
                AudioClip[] footstepClips = DetermineAudioClips();

                if (footstepClips.Length > 0)
                {
                    AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

                    footStepAudioSource.PlayOneShot(clip);
                }

                nextStepTimer = Time.time + (stepInterval / currentSpeedMultiplier);
            }
        }
    }

    private AudioClip[] DetermineAudioClips()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, 1.5f, terrainLayerMask))
        {
            string tag = hit.collider.tag;

            switch (tag)
            {
                case "Ground":
                    return groundFootsteps;
                case "Grass":
                    return grassFootsteps;
                case "Gravel":
                    return gravelFootsteps;
                default:
                    return groundFootsteps;
            }
        }
        return groundFootsteps;
    }

    private float VerticalForceCalculation()
    {
        if (characterContoller.isGrounded)
        {
            verticalVelocity = -1;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }
    private void InputManagement()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        mouseY = Input.GetAxis("Mouse X");
        mouseX = Input.GetAxis("Mouse Y");
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -90f, 90f);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}