using UnityEngine;
using System.Collections;
using System;

public class FluidGrid : MonoBehaviour
{
    // This class is deeply inspired by http://www.intpowertechcorp.com/GDC03.pdf

    private float[,] m_oldVelocitiesX = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_oldVelocitiesY = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_currentVelocitiesX = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_currentVelocitiesY = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];

    private float[,] m_oldDensitiesR = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_currentDensitiesR = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_oldDensitiesG = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_currentDensitiesG = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_oldDensitiesB = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_currentDensitiesB = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_oldDensitiesA = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    private float[,] m_currentDensitiesA = new float[FluidDataBank.GRID_WIDTH, FluidDataBank.GRID_HEIGHT];
    
    public bool RelaxDensities = true;
    public int DensityDiffusionIterations = 3;
    public float DensityDiffusionAlpha = 0.05f;

    public bool AdvectDensities = true;

    public bool RelaxVelocities = true;
    public int VelocityDiffusionIterations = 3;
    public float VelocityDiffusionAlpha = 0.1f; // viscosity

    public bool AdvectVelocities = true;

    public bool ProjectVelocities = true;
    public int ProjectionIterations = 10;

    public bool RenderVelocities = false;
    public bool RenderDensities = false;

    public float GlobalDecayRate = 0.9f;

    public bool ResetField = false;

    void Start()
    {
        DoResetField();
	}

    void DoResetField()
    {
        for (int x = 0; x < FluidDataBank.GRID_WIDTH; ++x)
        {
            for (int y = 0; y < FluidDataBank.GRID_HEIGHT; ++y)
            {
                m_currentVelocitiesX[x, y] = 0.0f; // Mathf.Cos(x / 23.0f) * 3.0f;
                m_currentVelocitiesY[x, y] = 0.0f; // -Mathf.Sin(y / 37.0f) * 3.0f;
                m_currentDensitiesR[x, y] = Mathf.Cos(x / 3.0f) * Mathf.Sin(y / 9.0f);
                m_currentDensitiesG[x, y] = Mathf.Cos(x / 5.0f) * Mathf.Sin(y / 8.0f);
                m_currentDensitiesB[x, y] = Mathf.Cos(x / 7.0f) * Mathf.Sin(y / 4.0f);
                //m_currentDensitiesA[x, y] = 0.5f;
                //m_currentDensitiesR[x, y] = 0.0f;
                //m_currentDensitiesG[x, y] = 0.0f;
                //m_currentDensitiesB[x, y] = 0.0f;
                m_currentDensitiesA[x, y] = 0.0f;
            }
        }
    }

    Vector3 GridToWorldPoint(int x, int y)
    {
        return transform.TransformPoint(new Vector3((float)x / FluidDataBank.GRID_WIDTH, (float)y / FluidDataBank.GRID_HEIGHT, 0.0f));
    }

    Vector3 GridToWorldVector(float x, float y)
    {
        return transform.TransformVector(new Vector3((float)x / FluidDataBank.GRID_WIDTH, (float)y / FluidDataBank.GRID_HEIGHT, 0.0f));
    }

    private void GridCoordinatesToInterpolationParameters(float sampleX, float sampleY, out int leftX, out int rightX, out int bottomY, out int topY, out float alphaX, out float alphaY)
    {
        leftX = Clamp(Mathf.FloorToInt(sampleX), 0, FluidDataBank.GRID_WIDTH - 1);
        rightX = Clamp(Mathf.CeilToInt(sampleX), 0, FluidDataBank.GRID_WIDTH - 1);
        bottomY = Clamp(Mathf.FloorToInt(sampleY), 0, FluidDataBank.GRID_HEIGHT - 1);
        topY = Clamp(Mathf.CeilToInt(sampleY), 0, FluidDataBank.GRID_HEIGHT - 1);
        alphaX = sampleX - leftX;
        alphaY = sampleY - bottomY;
    }

    private void WorldPositionToGrid(Vector3 worldPosition, out float gridX, out float gridY)
    {
        var local = transform.InverseTransformPoint(worldPosition);

        gridX = Clamp(local.x, 0.0f, 1.0f) * FluidDataBank.GRID_WIDTH;
        gridY = Clamp(local.y, 0.0f, 1.0f) * FluidDataBank.GRID_HEIGHT;
    }
    
    private void WorldPositionToGrid(Vector3 worldPosition, out int leftX, out int rightX, out int bottomY, out int topY, out float alphaX, out float alphaY)
    {
        float gridX;
        float gridY;
        WorldPositionToGrid(worldPosition, out gridX, out gridY);

        GridCoordinatesToInterpolationParameters(gridX, gridY, out leftX, out rightX, out bottomY, out topY, out alphaX, out alphaY);
    }

    float SampleGrid(float[,] grid, int leftX, int rightX, int bottomY, int topY, float alphaX, float alphaY)
    {
        //if (leftX < 0 || rightX > FluidDataBank.GRID_WIDTH - 1 || bottomY < 0 || topY > FluidDataBank.GRID_HEIGHT - 1
        //    || rightX < 0 || leftX > FluidDataBank.GRID_WIDTH - 1 || topY < 0 || bottomY > FluidDataBank.GRID_HEIGHT - 1)
        //{
        //    return 0.0f;
        //}

        return Mathf.Lerp(
            Mathf.Lerp(grid[leftX, bottomY], grid[rightX, bottomY], alphaX),
            Mathf.Lerp(grid[leftX, topY], grid[rightX, topY], alphaX),
            alphaY);
    }

//    float Sample
//{
//                int leftX = Math.Max(Mathf.FloorToInt(sampleX), 0);
//                int rightX = Math.Min(Mathf.CeilToInt(sampleX), FluidDataBank.GRID_WIDTH - 1);
//                int bottomY = Math.Max(Mathf.FloorToInt(sampleY), 0);
//                int topY = Math.Min(Mathf.CeilToInt(sampleY), FluidDataBank.GRID_HEIGHT - 1);
//                float alphaX = sampleX - leftX;
//                float alphaY = sampleY - bottomY;
//                var sampleColor = Mathf.Lerp(
//                    Mathf.Lerp(source[leftX, bottomY], source[rightX, bottomY], alphaX),
//                    Mathf.Lerp(source[leftX, topY], source[rightX, topY], alphaX),
//                    alphaY);
//}

    public Vector3 GetVelocityAtPoint(Vector3 position)
    {
        int leftX;
        int rightX;
        int bottomY;
        int topY;
        float alphaX;
        float alphaY;
        WorldPositionToGrid(position, out leftX, out rightX, out bottomY, out topY, out alphaX, out alphaY);

        float velX = SampleGrid(m_currentVelocitiesX, leftX, rightX, bottomY, topY, alphaX, alphaY);
        float velY = SampleGrid(m_currentVelocitiesY, leftX, rightX, bottomY, topY, alphaX, alphaY);

        return new Vector3(velX, velY);
    }

    public Vector4 GetColorAtPoint(Vector3 position)
    {
        int leftX;
        int rightX;
        int bottomY;
        int topY;
        float alphaX;
        float alphaY;
        WorldPositionToGrid(position, out leftX, out rightX, out bottomY, out topY, out alphaX, out alphaY);

        float r = SampleGrid(m_currentDensitiesR, leftX, rightX, bottomY, topY, alphaX, alphaY);
        float g = SampleGrid(m_currentDensitiesG, leftX, rightX, bottomY, topY, alphaX, alphaY);
        float b = SampleGrid(m_currentDensitiesB, leftX, rightX, bottomY, topY, alphaX, alphaY);
        float a = SampleGrid(m_currentDensitiesA, leftX, rightX, bottomY, topY, alphaX, alphaY);

        return new Vector4(r, g, b, a);
    }

    public void InjectColorAtPoint(Vector3 position, Vector4 color, float radius, float lerp)
    {
        //int leftX;
        //int rightX;
        //int bottomY;
        //int topY;
        //float alphaX;
        //float alphaY;
        //WorldPositionToGrid(position, out leftX, out rightX, out bottomY, out topY, out alphaX, out alphaY);
        //float radiusX = Mathf.Max(radius * FluidDataBank.GRID_HEIGHT /*not a typo, radius is relative to grid height*/, 1.0f);
        //float radiusY = Mathf.Max(radius * FluidDataBank.GRID_HEIGHT, 1.0f);
        radius = Mathf.Max(radius * FluidDataBank.GRID_HEIGHT, 1.0f);
        float sampleCenterX;
        float sampleCenterY;
        WorldPositionToGrid(position, out sampleCenterX, out sampleCenterY);

        // HACK HACK HACK (like much of the rest of this file)
        int numSamples = Math.Max((int)(radius * radius * 10), 1);
        for (int i = 0; i < numSamples; ++i)
        {
            float angle = UnityEngine.Random.value * 2.0f * Mathf.PI;
            float sampleRadius = Mathf.Pow(UnityEngine.Random.value, 0.5f) * radius;
            //float sampleX = sampleCenterX + Random.value * radiusX;
            //float sampleY = sampleCenterY + Random.value * radiusY;
            float sampleX = sampleCenterX + Mathf.Cos(angle) * sampleRadius;
            float sampleY = sampleCenterY + Mathf.Sin(angle) * sampleRadius;
            //m_currentDensitiesR[leftX, bottomY] = Mathf.Lerp(m_currentDensitiesR[leftX, bottomY], color.R, alphaX);
            int ix = Clamp(Mathf.RoundToInt(sampleX), 0, FluidDataBank.GRID_WIDTH - 1);
            int iy = Clamp(Mathf.RoundToInt(sampleY), 0, FluidDataBank.GRID_HEIGHT - 1);
            m_currentDensitiesR[ix, iy] = Mathf.Lerp(m_currentDensitiesR[ix, iy], color.x, lerp);
            m_currentDensitiesG[ix, iy] = Mathf.Lerp(m_currentDensitiesG[ix, iy], color.y, lerp);
            m_currentDensitiesB[ix, iy] = Mathf.Lerp(m_currentDensitiesB[ix, iy], color.z, lerp);
            m_currentDensitiesA[ix, iy] = Mathf.Lerp(m_currentDensitiesA[ix, iy], color.w, lerp);
        }
    }

    public void InjectVelocityAtPoint(Vector3 position, Vector3 velocity, float radius, float lerp)
    {
        //int leftX;
        //int rightX;
        //int bottomY;
        //int topY;
        //float alphaX;
        //float alphaY;
        //WorldPositionToGrid(position, out leftX, out rightX, out bottomY, out topY, out alphaX, out alphaY);
        //float sampleX;
        //float sampleY;
        //WorldPositionToGrid(position, out sampleX, out sampleY);

        //m_currentVelocitiesX[Mathf.RoundToInt(sampleX), Mathf.RoundToInt(sampleY)] = velocity.x;
        //m_currentVelocitiesY[Mathf.RoundToInt(sampleX), Mathf.RoundToInt(sampleY)] = velocity.y;

        radius = Mathf.Max(radius * FluidDataBank.GRID_HEIGHT, 1.0f);
        float sampleCenterX;
        float sampleCenterY;
        WorldPositionToGrid(position, out sampleCenterX, out sampleCenterY);

        // HACK HACK HACK (like much of the rest of this file) COPY PASTE IT'S LATE
        int numSamples = Math.Max((int)(radius * radius * 10), 1);
        for (int i = 0; i < numSamples; ++i)
        {
            float angle = UnityEngine.Random.value * 2.0f * Mathf.PI;
            float sampleRadius = Mathf.Pow(UnityEngine.Random.value, 0.5f) * radius;
            //float sampleX = sampleCenterX + Random.value * radiusX;
            //float sampleY = sampleCenterY + Random.value * radiusY;
            float sampleX = sampleCenterX + Mathf.Cos(angle) * sampleRadius;
            float sampleY = sampleCenterY + Mathf.Sin(angle) * sampleRadius;
            //m_currentDensitiesR[leftX, bottomY] = Mathf.Lerp(m_currentDensitiesR[leftX, bottomY], color.R, alphaX);
            int ix = Clamp(Mathf.RoundToInt(sampleX), 0, FluidDataBank.GRID_WIDTH - 1);
            int iy = Clamp(Mathf.RoundToInt(sampleY), 0, FluidDataBank.GRID_HEIGHT - 1);
            m_currentVelocitiesX[ix, iy] = Mathf.Lerp(m_currentVelocitiesX[ix, iy], velocity.x, lerp);
            m_currentVelocitiesY[ix, iy] = Mathf.Lerp(m_currentVelocitiesY[ix, iy], velocity.y, lerp);
        }
    }

    void Relax(float[,] destination, float[,] source, int numIterations, float alpha, BoundaryMode boundaryMode, float deltaTime)
    {
        CopyGrid(destination, source);

        var k = alpha * deltaTime;

        // Gauss-Siedel relaxation of the target density matrix
        for (int iteration = 0; iteration < numIterations; ++iteration)
        {
            for (int x = 1; x < FluidDataBank.GRID_WIDTH - 1; ++x)
            {
                for (int y = 1; y < FluidDataBank.GRID_HEIGHT - 1; ++y)
                {
                    destination[x, y] = (source[x, y] + k * (destination[x - 1, y] + destination[x + 1, y] + destination[x, y - 1] + destination[x, y + 1])) / (1 + 4 * k);
                }
            }
        }

        SetBoundaries(destination, boundaryMode);
    }

    void Advect(float[,] destination, float[,] source, float[,] velocityX, float[,] velocityY, BoundaryMode boundaryMode, float deltaTime)
    {
        // Instead of taking an explicit "integrate forward" approach, we do this implicit-style by tracing backwards from each cell center;\
        // since this requires us to compute a source value between cell centers, we simply linearly interpolate between the four neighbouring cells
        for (int x = 1; x < FluidDataBank.GRID_WIDTH - 1; ++x)
        {
            for (int y = 1; y < FluidDataBank.GRID_HEIGHT - 1; ++y)
            {
                float sampleX = x - deltaTime * velocityX[x, y];
                float sampleY = y - deltaTime * velocityY[x, y];

                int leftX;
                int rightX;
                int bottomY;
                int topY;
                float alphaX;
                float alphaY;

                GridCoordinatesToInterpolationParameters(sampleX, sampleY, out leftX, out rightX, out bottomY, out topY, out alphaX, out alphaY);
                destination[x, y] = SampleGrid(source, leftX, rightX, bottomY, topY, alphaX, alphaY);
            }
        }

        SetBoundaries(destination, boundaryMode);
    }

    void Project(float[,] velocitiesX, float[,] velocitiesY, float[,] tempA, float[,] tempB, int numIterations)
    {
        // Get rid of divergence in the velocity vector field
        var divergence = tempA;
        var relaxedDivergence = tempB;

        for (int x = 1; x < FluidDataBank.GRID_WIDTH - 1; ++x)
        {
            for (int y = 1; y < FluidDataBank.GRID_HEIGHT - 1; ++y)
            {
                divergence[x, y] = -0.5f * ((velocitiesX[x + 1, y] - velocitiesX[x - 1, y]) + (velocitiesY[x, y + 1] - velocitiesY[x, y - 1]));
                relaxedDivergence[x, y] = 0.0f;
            }
        }

        SetBoundaries(divergence, BoundaryMode.ContinuousEdges);
        SetBoundaries(relaxedDivergence, BoundaryMode.ContinuousEdges);

        for (int iteration = 0; iteration < numIterations; ++iteration)
        {
            for (int x = 1; x < FluidDataBank.GRID_WIDTH - 1; ++x)
            {
                for (int y = 1; y < FluidDataBank.GRID_HEIGHT - 1; ++y)
                {
                    relaxedDivergence[x, y] = (divergence[x, y]
                        + relaxedDivergence[x - 1, y]
                        + relaxedDivergence[x + 1, y]
                        + relaxedDivergence[x, y - 1]
                        + relaxedDivergence[x, y + 1])
                        / 4;
                }
            }
            SetBoundaries(relaxedDivergence, BoundaryMode.ContinuousEdges);
        }

        for (int x = 1; x < FluidDataBank.GRID_WIDTH - 1; ++x)
        {
            for (int y = 1; y < FluidDataBank.GRID_HEIGHT - 1; ++y)
            {
                velocitiesX[x, y] -= 0.5f * (relaxedDivergence[x + 1, y] - relaxedDivergence[x - 1, y]);
                velocitiesY[x, y] -= 0.5f * (relaxedDivergence[x, y + 1] - relaxedDivergence[x, y - 1]);
            }
        }

        SetBoundaries(velocitiesX, BoundaryMode.MirrorHorizontal);
        SetBoundaries(velocitiesY, BoundaryMode.MirrorVertical);
    }

    enum BoundaryMode
    {
        ContinuousEdges,
        MirrorHorizontal,
        MirrorVertical
    }

    void SetBoundaries(float [,] grid, BoundaryMode mode)
    {
        for (int x = 1; x < FluidDataBank.GRID_WIDTH - 1; ++x)
        {
            grid[x, 0] = grid[x, 1] * ((mode == BoundaryMode.MirrorVertical) ? -1 : 1);
            grid[x, FluidDataBank.GRID_HEIGHT - 1] = grid[x, FluidDataBank.GRID_HEIGHT - 2] * ((mode == BoundaryMode.MirrorVertical) ? -1 : 1);
        }
        
        for (int y = 1; y < FluidDataBank.GRID_HEIGHT - 1; ++y)
        {
            grid[0, y] = grid[1, y] * ((mode == BoundaryMode.MirrorHorizontal) ? -1 : 1);
            grid[FluidDataBank.GRID_WIDTH - 1, y] = grid[FluidDataBank.GRID_WIDTH - 2, y] * ((mode == BoundaryMode.MirrorHorizontal) ? -1 : 1);
        }

        grid[0, 0] = 0.5f * (grid[0, 1] + grid[1, 0]);
        grid[FluidDataBank.GRID_WIDTH - 1, 0] = 0.5f * (grid[FluidDataBank.GRID_WIDTH - 2, 0] + grid[FluidDataBank.GRID_WIDTH - 1, 1]);
        grid[0, FluidDataBank.GRID_HEIGHT - 1] = 0.5f * (grid[0, FluidDataBank.GRID_HEIGHT - 2] + grid[1, FluidDataBank.GRID_HEIGHT - 1]);
        grid[FluidDataBank.GRID_WIDTH - 1, FluidDataBank.GRID_HEIGHT - 1] = 0.5f * (grid[FluidDataBank.GRID_WIDTH - 2, FluidDataBank.GRID_HEIGHT - 1] + grid[FluidDataBank.GRID_WIDTH - 1, FluidDataBank.GRID_HEIGHT - 2]);
    }

    void CopyGrid(float[,] destination, float[,] source)
    {
        for (int x = 0; x < FluidDataBank.GRID_WIDTH; ++x)
        {
            for (int y = 0; y < FluidDataBank.GRID_HEIGHT; ++y)
            {
                destination[x, y] = source[x, y];
            }
        }
    }

    void AddBogusSource()
    {
        float t = Time.time * 1.0f;
        float angle = t * 1.0f;
        float centerX = FluidDataBank.GRID_WIDTH * 0.5f;
        float centerY = FluidDataBank.GRID_HEIGHT * 0.5f;
        float radius = 10.0f;
        float velX = centerX + Mathf.Cos(angle) * radius;
        float velY = centerY + Mathf.Sin(angle) * radius;
        //m_currentVelocitiesX[Mathf.RoundToInt(velX), Mathf.RoundToInt(velY)] += 100.0f;

        float r = (0.5f + 0.5f * Mathf.Sin(t * 0.01f)) * 10.0f;
        float g = 0.0f;// (0.5f + 0.5f * Mathf.Sin(t * 0.02f)) * 10.0f;
        float b = 0.0f;// (0.5f + 0.5f * Mathf.Sin(t * 0.03f)) * 10.0f;
        float a = 10.0f;// (0.5f + 0.5f * Mathf.Sin(t * 0.03f)) * 10.0f;
        var worldPosition = transform.TransformPoint(velX / FluidDataBank.GRID_WIDTH, velY / FluidDataBank.GRID_HEIGHT, 0.0f);

        InjectVelocityAtPoint(worldPosition, new Vector3(100, 0, 0), 0.1f, 1.0f);
        InjectColorAtPoint(worldPosition, new Vector4(r, g, b, a), 0.1f, 1.0f);
    }

    void SwapRefs<T>(ref T a, ref T b)
    {
        var x = a;
        a = b;
        b = x;
    }

    int Clamp(int value, int min, int max)
    {
        return Math.Max(Math.Min(value, max), min);
    }

    float Clamp(float value, float min, float max)
    {
        return Mathf.Max(Mathf.Min(value, max), min);
    }

    void Update()
    {
        var deltaTime = Time.deltaTime;

        if (ResetField)
        {
            DoResetField();
            ResetField = false;
        }

        //AddBogusSource();

        // Density update
        SwapRefs(ref m_currentDensitiesR, ref m_oldDensitiesR);
        SwapRefs(ref m_currentDensitiesG, ref m_oldDensitiesG);
        SwapRefs(ref m_currentDensitiesB, ref m_oldDensitiesB);
        SwapRefs(ref m_currentDensitiesA, ref m_oldDensitiesA);
        if (RelaxDensities)
        {
            Relax(m_currentDensitiesR, m_oldDensitiesR, DensityDiffusionIterations, DensityDiffusionAlpha, BoundaryMode.ContinuousEdges, deltaTime);
            Relax(m_currentDensitiesG, m_oldDensitiesG, DensityDiffusionIterations, DensityDiffusionAlpha, BoundaryMode.ContinuousEdges, deltaTime);
            Relax(m_currentDensitiesB, m_oldDensitiesB, DensityDiffusionIterations, DensityDiffusionAlpha, BoundaryMode.ContinuousEdges, deltaTime);
            Relax(m_currentDensitiesA, m_oldDensitiesA, DensityDiffusionIterations, DensityDiffusionAlpha, BoundaryMode.ContinuousEdges, deltaTime);
        }
        else
        {
            CopyGrid(m_currentDensitiesR, m_oldDensitiesR);
            CopyGrid(m_currentDensitiesG, m_oldDensitiesG);
            CopyGrid(m_currentDensitiesB, m_oldDensitiesB);
            CopyGrid(m_currentDensitiesA, m_oldDensitiesA);
        }

        SwapRefs(ref m_currentDensitiesR, ref m_oldDensitiesR);
        SwapRefs(ref m_currentDensitiesG, ref m_oldDensitiesG);
        SwapRefs(ref m_currentDensitiesB, ref m_oldDensitiesB);
        SwapRefs(ref m_currentDensitiesA, ref m_oldDensitiesA);
        if (AdvectDensities)
        {
            Advect(m_currentDensitiesR, m_oldDensitiesR, m_currentVelocitiesX, m_currentVelocitiesY, BoundaryMode.ContinuousEdges, deltaTime);
            Advect(m_currentDensitiesG, m_oldDensitiesG, m_currentVelocitiesX, m_currentVelocitiesY, BoundaryMode.ContinuousEdges, deltaTime);
            Advect(m_currentDensitiesB, m_oldDensitiesB, m_currentVelocitiesX, m_currentVelocitiesY, BoundaryMode.ContinuousEdges, deltaTime);
            Advect(m_currentDensitiesA, m_oldDensitiesA, m_currentVelocitiesX, m_currentVelocitiesY, BoundaryMode.ContinuousEdges, deltaTime);
        }
        else
        {
            CopyGrid(m_currentDensitiesR, m_oldDensitiesR);
            CopyGrid(m_currentDensitiesG, m_oldDensitiesG);
            CopyGrid(m_currentDensitiesB, m_oldDensitiesB);
            CopyGrid(m_currentDensitiesA, m_oldDensitiesA);
        }

        // Velocity update
        SwapRefs(ref m_currentVelocitiesX, ref m_oldVelocitiesX);
        SwapRefs(ref m_currentVelocitiesY, ref m_oldVelocitiesY);
        if (RelaxVelocities)
        {
            Relax(m_currentVelocitiesX, m_oldVelocitiesX, VelocityDiffusionIterations, VelocityDiffusionAlpha, BoundaryMode.MirrorHorizontal, deltaTime);
            Relax(m_currentVelocitiesY, m_oldVelocitiesY, VelocityDiffusionIterations, VelocityDiffusionAlpha, BoundaryMode.MirrorVertical, deltaTime);
        }
        else
        {
            CopyGrid(m_currentVelocitiesX, m_oldVelocitiesX);
            CopyGrid(m_currentVelocitiesY, m_oldVelocitiesY);
        }

        // Projection works in place; old velocities used as scratch arrays here
        if (ProjectVelocities)
        {
            Project(m_currentVelocitiesX, m_currentVelocitiesY, m_oldVelocitiesX, m_oldVelocitiesY, ProjectionIterations);
        }

        SwapRefs(ref m_currentVelocitiesX, ref m_oldVelocitiesX);
        SwapRefs(ref m_currentVelocitiesY, ref m_oldVelocitiesY);
        if (AdvectVelocities)
        {
            Advect(m_currentVelocitiesX, m_oldVelocitiesX, m_oldVelocitiesX, m_oldVelocitiesY, BoundaryMode.MirrorHorizontal, deltaTime);
            Advect(m_currentVelocitiesY, m_oldVelocitiesY, m_oldVelocitiesX, m_oldVelocitiesY, BoundaryMode.MirrorVertical, deltaTime);
        }
        else
        {
            CopyGrid(m_currentVelocitiesX, m_oldVelocitiesX);
            CopyGrid(m_currentVelocitiesY, m_oldVelocitiesY);
        }
        if (ProjectVelocities)
        {
            Project(m_currentVelocitiesX, m_currentVelocitiesY, m_oldVelocitiesX, m_oldVelocitiesY, ProjectionIterations);
        }

        float decay = Mathf.Pow(GlobalDecayRate, Time.deltaTime);
        for (int x = 0; x < FluidDataBank.GRID_WIDTH; ++x)
        {
            for (int y = 0; y < FluidDataBank.GRID_HEIGHT; ++y)
            {
                m_currentDensitiesR[x, y] *= decay;
                m_currentDensitiesG[x, y] *= decay;
                m_currentDensitiesB[x, y] *= decay;
                m_currentDensitiesA[x, y] *= decay;
            }
        }

        for (int x = 0; x < FluidDataBank.GRID_WIDTH; ++x)
        {
            for (int y = 0; y < FluidDataBank.GRID_HEIGHT; ++y)
            {
                var origin = GridToWorldPoint(x, y);
                if (RenderVelocities) Debug.DrawLine(origin, origin + GridToWorldVector(m_currentVelocitiesX[x, y], m_currentVelocitiesY[x, y]), new Vector4(1, 0, 0));
                if (RenderDensities)
                {
                    var lineLength = 0.1f;
                    var r = m_currentDensitiesR[x, y];
                    var g = m_currentDensitiesG[x, y];
                    var b = m_currentDensitiesB[x, y];
                    Debug.DrawLine(origin + GridToWorldVector(-lineLength, -lineLength), origin + GridToWorldVector(lineLength, lineLength), new Vector4(r, g, b));
                    Debug.DrawLine(origin + GridToWorldVector(lineLength, -lineLength), origin + GridToWorldVector(-lineLength, lineLength), new Vector4(r, g, b));
                }
            }
        }

        //var c1 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
        //var c2 = transform.TransformPoint(new Vector3(0.5f, 0.5f, 0));
        //Debug.DrawLine(c1, c2);

        //var textureData = VelocityTexture.GetPixels();
        //for (int x = 0; x < FluidDataBank.GRID_WIDTH; ++x)
        //{
        //    for (int y = 0; y < FluidDataBank.GRID_HEIGHT; ++y)
        //    {
        //        var vel = m_velocities[x, y];
        //        textureData[y * FluidDataBank.GRID_HEIGHT + x] = new Vector4(vel.x, vel.y, 0);
        //    }
        //}

        Texture2D densityTexture = FluidDataBank.instance.getFluidDensityTexture();
        if (densityTexture != null)
        {
            int size = FluidDataBank.GRID_WIDTH * FluidDataBank.GRID_HEIGHT;
            Color[] array = new Color[size];
            for (int y = 0; y < FluidDataBank.GRID_HEIGHT; ++y)
            {
                for (int x = 0; x < FluidDataBank.GRID_WIDTH; ++x)
                {
                    array[y * FluidDataBank.GRID_WIDTH + x] = new Color(
                        Clamp(m_currentDensitiesR[x, y], 0.0f, 1.0f),
                        Clamp(m_currentDensitiesG[x, y], 0.0f, 1.0f),
                        Clamp(m_currentDensitiesB[x, y], 0.0f, 1.0f),
                        Clamp(m_currentDensitiesA[x, y], 0.0f, 1.0f));
                }
            }
            densityTexture.SetPixels(array);
            densityTexture.Apply();
        }
    }
}
