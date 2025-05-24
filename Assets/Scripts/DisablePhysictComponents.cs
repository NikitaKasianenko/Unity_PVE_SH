using UnityEngine;
using UnityEditor;

public class DisablePhysicsComponents : MonoBehaviour
{
    [MenuItem("Tools/Disable/Disable Rigidbody & BoxCollider")]
    private static void DisablePhysics()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Выдели объект в Hierarchy");
            return;
        }

        GameObject root = Selection.activeGameObject;

        int rbCount = 0;
        int colCount = 0;

        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rbCount++;
            }

            BoxCollider bc = t.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.enabled = false;
                colCount++;
            }  
            CapsuleCollider cc = t.GetComponent<CapsuleCollider>();
            if (cc != null)
            {
                cc.enabled = false;
                colCount++;
            }
        }

        Debug.Log($"Выключено: Rigidbody={rbCount}, Collider={colCount}");
    }
}
