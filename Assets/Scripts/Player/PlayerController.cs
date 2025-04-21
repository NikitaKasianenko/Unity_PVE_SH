using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    public CinemachineCamera virtualCamera;
    [SerializeField] private AudioSource footstepSound;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float sprintTransitSpeed = 5f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Animation")]
    private int animMoveSpeed;
    private int animJump;
    private int animGrounded;


    private float verticalVelocity;
    private float currentSpeed;
    private float currentSpeedMultiplier;
    private float xRotation;

    [Header("Camera Bob Settings")]
    [SerializeField] private float bobFrequency = 1f;
    [SerializeField] private float bobAmplitude = 1f;


    [Header("Recoil")]
    private Vector3 targetRecoil = Vector3.zero;
    private Vector3 currentRecoil = Vector3.zero;

    private CinemachineBasicMultiChannelPerlin noiseComponent;
    //private float bobTimer = 0f;

    [Header("Footstep Settings")]
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private float stepInterval = 1f;

    private float nextStepTimer = 0;

    [Header("SFX")]
    [SerializeField] private AudioClip[] groundFootsteps;
    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip[] gravelFootsteps;

    [Header("Input")]
    [SerializeField] private float mouseSensitivity;
    private float moveInput;
    private float turnInput;
    private float mouseX;
    private float mouseY;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        noiseComponent = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        animator = GetComponent<Animator>();
        SetupAnimator();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        InputManagement();
        Movement();

        PlayFootstepSound();
    }

    private void LateUpdate()
    {
        CameraBob();
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
        }
        else
        {
            currentSpeedMultiplier = 1f;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * currentSpeedMultiplier, sprintTransitSpeed * Time.deltaTime);

        moveDirection.y = VerticalForceCalculation();

        controller.Move(moveDirection * Time.deltaTime);

        animator.SetFloat(animMoveSpeed, currentSpeed * Mathf.Max(Mathf.Abs(moveInput), Mathf.Abs(turnInput)));

    }


    private void Turn()
    {
        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        virtualCamera.transform.localRotation = Quaternion.Slerp(virtualCamera.transform.localRotation, Quaternion.Euler(xRotation + currentRecoil.y, currentRecoil.x, 0), 50f * Time.deltaTime);

        transform.Rotate(Vector3.up * mouseX);
    }

    //public void ApplyRecoil(GunData gunData)
    //{
    //    float recoilX = Random.Range(-gunData.recoilMax.x, gunData.recoilMax.x) * gunData.recoilAmount;
    //    float recoilY = Random.Range(-gunData.recoilMax.y, gunData.recoilMax.y) * gunData.recoilAmount;

    //    targetRecoil += new Vector3(recoilX, recoilY, 0);
    //    currentRecoil = Vector3.MoveTowards(currentRecoil, targetRecoil, Time.deltaTime * gunData.recoilSpeed);
    //}

    //public void ResetRecoil(GunData gunData)
    //{
    //    currentRecoil = Vector3.MoveTowards(currentRecoil, Vector3.zero, Time.deltaTime * gunData.ResetRecoilSpeed);
    //    currentRecoil = Vector3.MoveTowards(targetRecoil, Vector3.zero, Time.deltaTime * gunData.ResetRecoilSpeed);
    //}

    private void CameraBob()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            noiseComponent.AmplitudeGain = bobAmplitude * currentSpeedMultiplier;
            noiseComponent.FrequencyGain = bobFrequency * currentSpeedMultiplier;
        }
        else
        {
            noiseComponent.AmplitudeGain = 0.0f;
            noiseComponent.FrequencyGain = 0.0f;
        }
    }

    private void PlayFootstepSound()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            if (Time.time >= nextStepTimer)
            {
                AudioClip[] footstepClips = DetermineAudioClips();

                if (footstepClips.Length > 0)
                {
                    AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

                    footstepSound.PlayOneShot(clip);
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
        if (controller.isGrounded)
        {
            verticalVelocity = -1;
            Debug.Log("Grounded");
            animator.SetBool(animGrounded, true);

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
                animator.SetTrigger(animJump);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;

            animator.SetBool(animGrounded, false);
            Debug.Log("not Grounded");
        }
        return verticalVelocity;
    }

    private void SetupAnimator()
    {
        animMoveSpeed = Animator.StringToHash("MoveInput");
        animJump = Animator.StringToHash("Jump");
        animGrounded = Animator.StringToHash("Grounded");
    }

    private void InputManagement()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
}