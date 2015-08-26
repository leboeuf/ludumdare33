using UnityEngine;
using System.Collections;

public class EmitToFluid : MonoBehaviour
{
    public FluidGrid Fluid;
    public Vector3 FluidVelocity;
    public Vector4 FluidColor;
    public float FluidRadius;
    public float FluidLerp;

	void Start()
    {
	}
	
    void Update()
    {
        if (Fluid != null)
        {
            Fluid.InjectColorAtPoint(transform.position, FluidColor, FluidRadius, FluidLerp);
            Fluid.InjectVelocityAtPoint(transform.position, FluidVelocity, FluidRadius, FluidLerp);
        }
	}
}
