using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Distortion/Wave")]
public class WaveDistortion : PostEffectsBase {

    public Shader shaderWaveDistortion;
    public Texture distortionTexture;
    private Material material;

    public Vector2 distortionTilling = new Vector2(8.0f, 8.0f);
    public Vector2 distortionOffset = new Vector2(0.0f, 0.0f);

    public Vector2 Intensity = new Vector2(0.003f, 0.004f);
    public Vector2 Scrolling = new Vector2(0.5f, 0.5f);

    public Texture Mask;

    public override bool CheckResources()
    {
        CheckSupport(false);

        material = CreateMaterial(shaderWaveDistortion, material);
        if (material == null || distortionTexture == null) return false;

        return isSupported;
    }


    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Vector4 intensityScroll = new Vector4(Intensity.x, Intensity.y, Scrolling.x, Scrolling.y);
        material.SetVector("_IntensityAndScrolling", intensityScroll);
        material.SetTexture("_DistoTex", distortionTexture);

        Vector4 distoTex_TillingOffset = new Vector4(distortionTilling.x, distortionTilling.y, distortionOffset.x, distortionOffset.y);
        material.SetVector("_DistoTex_TillingOffset", distoTex_TillingOffset);

        if (Mask != null)
            material.SetTexture("_DistoMaskTex", Mask);

        Graphics.Blit(source, destination, material);
    }
}
