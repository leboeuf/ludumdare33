using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {
    public float maxSpeed;
    public float fluidVelocityMultiplier;

    Rigidbody2D rbody;
	// Use this for initialization
	void Start () {
	}

    void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
        /*if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 relPos = trans.position - mousePos;
            Vector2 force = new Vector2(relPos.x, relPos.y);
            this.body.AddForce(force);
        }*/

        AddFluidForce();

        if (rbody.velocity.magnitude > maxSpeed)
        {
            rbody.velocity = rbody.velocity.normalized * maxSpeed;
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        GameManager gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        Net net = other.gameObject.GetComponent<Net>();
        
		if (!net) return;
		
		if(net.team == Team.Blue)
			gameManager.GivePointTo(Team.Blue);
		if(net.team == Team.Red)
			gameManager.GivePointTo(Team.Red);
    }

    private void AddFluidForce() 
    {
        rbody.velocity += (Vector2)(GameObject.FindGameObjectWithTag("Fluid").GetComponent<FluidGrid>().GetVelocityAtPoint(transform.position) * fluidVelocityMultiplier);
    }
}
