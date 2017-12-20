using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class CubeLabel : MonoBehaviour {

	int table;
	int seat;

	// Use this for initialization
	void Start () {
		table = GetComponentInParent<Controller>().GetTableNum();
		seat = GetComponentInParent<Controller>().GetSeatNum();
		GetComponent<TextMesh>().text = "" + table + "," + seat;
		transform.parent.name = table + "," + seat;
	}
	
	// Update is called once per frame
	void Update () {
		int t = GetComponentInParent<Controller>().GetTableNum();
		int s = GetComponentInParent<Controller>().GetSeatNum();
		if (t != table || s != seat) {
			//Name changed, update label
			table = t;
			seat = s;
			GetComponent<TextMesh>().text = "" + table + "," + seat;
			transform.parent.name = table + "," + seat;
		}
	}
}
