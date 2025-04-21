using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

#if KRIPTO_FX_LWRP_RENDERING
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif

[ExecuteInEditMode]
public class FPS_Decal : MonoBehaviour
{
    public float randomScalePercent = 50;
    private Vector3 startSize;

    void Awake()
    {
        var decalProjector = GetComponent<DecalProjector>();
        if (decalProjector == null) return;
        startSize = decalProjector.size;
    }


    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            var decalProjector = GetComponent<DecalProjector>();
            if (decalProjector == null) return;
            transform.localRotation = Quaternion.Euler(180, 0, Random.Range(0, 360));
            var randomScaleRange = Random.Range(startSize.x - startSize.x * randomScalePercent * 0.01f,
                                                startSize.x + startSize.x * randomScalePercent * 0.01f);
            decalProjector.size = new Vector3(randomScaleRange, randomScaleRange, startSize.z);
        }

        
    }

}
