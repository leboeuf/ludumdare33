using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/DEBUG/FluidDensity")]
public class DEBUG_FluidDensityGrid : PostEffectsBase
{
    public Shader FluidDensityShader;
    private Material FluidDensityMaterial = null;

    public override bool CheckResources()
    {
        CheckSupport(true);

        FluidDensityMaterial = CheckShaderAndCreateMaterial(FluidDensityShader, FluidDensityMaterial);

        return isSupported;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false || FluidDataBank.instance == null || FluidDataBank.instance.getFluidDensityTexture() == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        FluidDensityMaterial.SetTexture("_FluidDensityTex", FluidDataBank.instance.getFluidDensityTexture());
        
        Graphics.Blit(source, destination, FluidDensityMaterial);
    }
}
