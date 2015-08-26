using UnityEngine;
using System.Collections;

public class DeactivateChildrenOnStart : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
	    foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
