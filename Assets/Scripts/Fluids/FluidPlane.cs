using UnityEngine;
using System.Collections;

public class FluidPlane : MonoBehaviour
{

    private Material fluidMaterial;
    public Shader fluidShader;

    // Use this for initialization
    void Start()
    {
        fluidMaterial = new Material(fluidShader);
        fluidMaterial.hideFlags = HideFlags.DontSave;
    }

    // Update is called once per frame
    void Update()
    {
        FluidDataBank data = FluidDataBank.instance;

        if (data != null)
        {
            Texture2D tex = data.getFluidDensityTexture();
            if (tex != null)
            {
                fluidMaterial.SetTexture("_FluidTex", tex);
                MeshRenderer renderer = GetComponent<MeshRenderer>();
                renderer.material = fluidMaterial;
            }
        }
    }
}
