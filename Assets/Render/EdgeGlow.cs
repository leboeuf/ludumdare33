using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Edge Detection/Edge Glow")]
public class EdgeGlow : PostEffectsBase
{
    public float edgeExp = 1.0f;
    public float sampleDist = 1.0f;
    public float edgeMultiplier = 3.0f;

    public Shader edgeGlowShader;
    private Material edgeGlowMaterial = null;

    public override bool CheckResources()
    {
        CheckSupport(true);

        edgeGlowMaterial = CheckShaderAndCreateMaterial(edgeGlowShader, edgeGlowMaterial);
        SetCameraFlag();

        if (!isSupported)
            ReportAutoDisable();

        return isSupported;
    }

    void SetCameraFlag()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }

    void OnEnable()
    {
        SetCameraFlag();
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }

        edgeGlowMaterial.SetFloat("_SampleDistance", sampleDist);
        edgeGlowMaterial.SetFloat("_Exponent", edgeExp);
        edgeGlowMaterial.SetFloat("_EdgeMultiplier", edgeMultiplier);
        
        Graphics.Blit(source, destination, edgeGlowMaterial);
    }
}
