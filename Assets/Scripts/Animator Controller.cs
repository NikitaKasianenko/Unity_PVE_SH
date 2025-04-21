using UnityEngine;

public class AnimatorController : MonoBehaviour
{

    [SerializeField] public Animator m_Animator;
    [SerializeField] public GameObject player;
    Rigidbody m_Rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Rigidbody = player.GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float magnitude = m_Rigidbody.linearVelocity.magnitude;
        m_Animator.SetFloat("Mag", magnitude);


        if (Input.GetKey(KeyCode.Mouse1))
        {
            m_Animator.SetBool("Aim", true);
        }
        else
        {
            m_Animator.SetBool("Aim", false);
        }
    }
}
