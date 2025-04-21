using UnityEngine;

public class quanttest : MonoBehaviour
{
    void Start()
    {
        Vector3 vector = new Vector3(1.0f, 1.0f, 1.0f);

        // 1) Quaternion (+0.808, +0.492, -0.015, +0.325) — w, x, y, z
        Quaternion q1 = new Quaternion(0.492f, -0.015f, 0.325f, 0.808f);
        Vector3 rotated1 = q1 * vector;
        Debug.Log("Rotated Vector 1: " + rotated1);

        // 2) Quaternion (+0.271, +0.271, +0.653, +0.653) — w, x, y, z
        Quaternion q2 = new Quaternion(0.271f, 0.653f, 0.653f, 0.271f);
        Vector3 rotated2 = q2 * vector;
        Debug.Log("Rotated Vector 2: " + rotated2);
    }
}
