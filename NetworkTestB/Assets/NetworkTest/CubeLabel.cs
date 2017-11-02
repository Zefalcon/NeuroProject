using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class CubeLabel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int table = GetComponentInParent<Controller>().GetTableNum();
		int seat = GetComponentInParent<Controller>().GetSeatNum();
		GetComponent<TextMesh>().text = "" + table + "," + seat;
		transform.parent.name = table + "," + seat;
	}
	
	// Update is called once per frame
	void Update () {
		int table = GetComponentInParent<Controller>().GetTableNum();
		int seat = GetComponentInParent<Controller>().GetSeatNum();
		GetComponent<TextMesh>().text = "" + table + "," + seat;
		transform.parent.name = table + "," + seat;
	}
}
