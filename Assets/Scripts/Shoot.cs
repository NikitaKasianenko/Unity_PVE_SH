using UnityEngine;

public class Shoot : MonoBehaviour
{

    [SerializeField] public Transform startPlace;
    [SerializeField] public LayerMask mask;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {

            if (Physics.Raycast(startPlace.position, startPlace.forward, out var hit,1000,mask))
            {
                Debug.Log("Hit: " + hit.collider.gameObject.name);
            }

            if(hit.collider.gameObject.CompareTag("Enemy"))
            {
                hit.rigidbody.AddExplosionForce(350,Vector3.zero,100,10);
            }
        }
    }
}
