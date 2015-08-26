using UnityEngine;
using System.Collections;
using Assets.Scripts.Gameplay;

public class CharacterController : MonoBehaviour
{
    public float movementSpeed;
    public float swimForce;
    public float swimTime;
    public float swimCooldown;
    public float angularSpeed;
	public AudioClip shootSound;
	public AudioClip swimSound;

	private AudioSource audioSource;
    private GameObject fluidGrid;
    private bool canSwim = true;
    protected float mLastSwimTime = 0f;
    //private bool isSwimming;

    private float maxBoostSpeed = 8.0f;
    //private float boostDecayRate = 0.1f;
    private float boostDecayRate = 24.0f;
    private float boostSpeed = 0.0f;
    //private float fluidDensity;

    private float minFluidSpeedScale = 0.8f;
    private float maxFluidSpeedScale = 1.2f;

    public float CurrentVelocityMagnitude;
    public Vector3 CurrentVelocity;
    
    // All input methods here
    protected virtual bool IsSwimming() { return false; }
    protected virtual Vector3 GetMovementAxis() { return new Vector3(0f,0f,0f); }
    
	void Awake()
	{
		audioSource = GetComponent<AudioSource>();
        fluidGrid = GameObject.FindGameObjectWithTag("Fluid");
	}

    // Use this for initialization
    void Start()
    {
    }

    void Update()
    {
        DerivedUpdate();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //float decay = Mathf.Pow(boostDecayRate, Time.fixedDeltaTime);
        //boostSpeed *= decay;
        float decay = boostDecayRate * Time.fixedDeltaTime;
        boostSpeed -= decay;
        boostSpeed = Mathf.Max(boostSpeed, 0.0f);
        //Debug.Log(boostSpeed);

        //if (!isSwimming)
            UpdatePlayerPosition();
        UpdateAngle();
        Swim();
        Ink();

        //Debug.DrawLine(transform.position, transform.position + GetMovementAxis().normalized, Color.blue);
    }
    
    protected Team GetTeam()
    {
        Player player = GetComponentInParent<Player>();
        return player.team;
    }

    protected virtual float AngularSpeed()
    {
        return angularSpeed;
    }

	protected virtual void DerivedUpdate() {
	}

    //Sets the player's position
    private void UpdatePlayerPosition()
    {
        Vector3 tempVel = GetMovementAxis();

        if (tempVel.magnitude > 1)
            tempVel.Normalize();

        var fluidDensity = GetFluidDensity();

        CurrentVelocity = tempVel * (movementSpeed + boostSpeed) * Mathf.Lerp(minFluidSpeedScale, maxFluidSpeedScale, fluidDensity);
        CurrentVelocityMagnitude = CurrentVelocity.magnitude;

        GetComponent<Rigidbody2D>().velocity = CurrentVelocity;
    }
    //Updates the player's angle
    private void UpdateAngle()
    {
        float angle = AngleXYSigned(GetMovementAxis(), this.transform.right);

        if(angle != 0f)
        {
            float angularForce = 0;
            float absAngle = Mathf.Abs(angle);
            float angleReduceSpeed = 60;
            if (absAngle > angleReduceSpeed)
            {
                angularForce = AngularSpeed() * -Mathf.Sign(angle);
            }
            else
            {
                float damping = absAngle / angleReduceSpeed;
                angularForce = AngularSpeed() * -Mathf.Sign(angle) * damping;
            }

            GetComponent<Rigidbody2D>().angularVelocity = angularForce;
        }
        else
        {
            GetComponent<Rigidbody2D>().angularVelocity = 0f;
        }
    }

    //Swim
    private void Swim()
    {
        //if (!isSwimming && canSwim && IsSwimming())
        if (canSwim && IsSwimming())
        {
            canSwim = false;
            //isSwimming = true;

            //GetComponent<Rigidbody2D>().AddForce(swimForce * GetMovementAxis());
            boostSpeed = maxBoostSpeed;

			audioSource.PlayOneShot(swimSound, 1.0f);

            //Invoke("StopSwimming", swimTime);
            Invoke("CanSwim", swimCooldown);
            mLastSwimTime = Time.time;

            GameObject clone = Instantiate(Resources.Load("Prefabs/Fx_inkBoost"), transform.position, Quaternion.identity) as GameObject;
            clone.transform.parent = transform;
        }
    }

    //private void StopSwimming()
    //{
    //    //isSwimming = false;
    //}

    private void CanSwim()
    {
        canSwim = true;
    }

    private static float AngleXYSigned(Vector3 v1, Vector3 v2)
    {
        v1.z = 0.0f;
        v2.z = 0.0f;
        return Mathf.Atan2(
            Vector3.Dot(Vector3.forward, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    private float GetFluidDensity()
    {
        var fluidColor = fluidGrid.GetComponent<FluidGrid>().GetColorAtPoint(transform.position);

        //Debug.Log(fluidColor);
        if (GetTeam() == Team.Blue)
        {
            return Mathf.Clamp((fluidColor.z - fluidColor.x) / 2f + 0.5f, 0f, 1f);
        }
        else
        {
            //Debug.Log(fluidColor.x - fluidColor.z);
            return Mathf.Clamp((fluidColor.x - fluidColor.z) / 2f + 0.5f, 0f, 1f);
        }
    }

    private void Ink()
    {
        //if (!canSwim)
        if (boostSpeed > 0.0f)
        {
            FluidGrid fluidScript = fluidGrid.GetComponent<FluidGrid>();
            Color color = (GetTeam() == Team.Blue) ? GameManager.BlueColor : GameManager.RedColor;
            fluidScript.InjectColorAtPoint(transform.position, color, 0.04f, 1.0f);
            fluidScript.InjectVelocityAtPoint(transform.position, GetMovementAxis() * -100f, 0.04f, 1.0f);
        }
    }
}
