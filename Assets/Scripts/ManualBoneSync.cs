using UnityEngine;
using System.Collections.Generic;

public class ManualBoneSync : MonoBehaviour
{
    [System.Serializable]
    public class BoneRootPair
    {
        public Transform sourceRoot; // FP-модель
        public Transform targetRoot; // FullBody
    }

    [Header("Пары корневых костей (например, левая и правая рука)")]
    [SerializeField] private BoneRootPair[] rootPairs;

    private List<(Transform source, Transform target)> bonePairs = new();

    void Start()
    {
        bonePairs.Clear();

        foreach (var pair in rootPairs)
        {
            if (pair.sourceRoot == null || pair.targetRoot == null)
            {
                Debug.LogWarning("One of the root pairs is not assigned.");
                continue;
            }

            MatchHierarchy(pair.sourceRoot, pair.targetRoot);
        }

        Debug.Log($"[MultiHierarchyBoneSync] Matched {bonePairs.Count} bones total.");
    }

    void LateUpdate()
    {
        foreach (var (source, target) in bonePairs)
        {
            if (source != null && target != null)
            {
                target.position = source.position;
                target.rotation = source.rotation;
            }
        }
    }

    private void MatchHierarchy(Transform source, Transform target)
    {
        bonePairs.Add((source, target));

        int childCount = Mathf.Min(source.childCount, target.childCount);

        for (int i = 0; i < childCount; i++)
        {
            MatchHierarchy(source.GetChild(i), target.GetChild(i));
        }
    }
}
