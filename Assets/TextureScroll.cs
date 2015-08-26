using UnityEngine;
using System.Collections;

public class TextureScroll : MonoBehaviour {

	public Vector2 scrollSpeed = new Vector2(0.1f, 0.1f);

	Renderer rend;

	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		rend.material.SetTextureOffset("_MainTex", scrollSpeed * Time.time);
	}
}
