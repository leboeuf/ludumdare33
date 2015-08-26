using UnityEngine;
using System.Collections;

public class FluidDataBank : MonoBehaviour
{
    public static int GRID_WIDTH = 64;
    public static int GRID_HEIGHT = 32;

    private Texture2D FluidDensityTexture;

    public Texture2D getFluidDensityTexture() 
    { 
        return FluidDensityTexture; 
    }

    //Here is a private reference only this class can access
    private static FluidDataBank _instance;
    private static GameObject _fluidDataBankPrefab;

    //This is the public reference that other classes will use
    public static FluidDataBank instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<FluidDataBank>();
            if (_instance == null && Application.isPlaying)
            {
                _fluidDataBankPrefab = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/FluidDataBank"));
                _instance = _fluidDataBankPrefab.GetComponent<FluidDataBank>();
            }
            return _instance;
        }
    }

    public void Start()
    {
        FluidDensityTexture = new Texture2D(GRID_WIDTH, GRID_HEIGHT, TextureFormat.RGBAFloat, false, true);

        // Init to a black transparent texture
        int size = FluidDataBank.GRID_WIDTH * FluidDataBank.GRID_HEIGHT;
        Color[] array = new Color[size];
        for (int y = 0; y < FluidDataBank.GRID_HEIGHT; ++y)
        {
            for (int x = 0; x < FluidDataBank.GRID_WIDTH; ++x)
            {
                array[y * FluidDataBank.GRID_WIDTH + x] = new Color(1.0f, 0.0f, 0.0f, 0.0f);
            }
        }
        FluidDensityTexture.SetPixels(array);
        FluidDensityTexture.Apply();
    }
}
