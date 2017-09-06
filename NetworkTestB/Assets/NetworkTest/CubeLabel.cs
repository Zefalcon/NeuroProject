using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class CubeLabel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int id = (int)GetComponentInParent<Controller>().netId.Value;
		GetComponent<TextMesh>().text = "" + id;
		transform.parent.name = transform.parent.name + id;
		//Debug.Log("Cube " + id + " created!");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
