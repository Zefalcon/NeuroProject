using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class SphereChangingLabel : MonoBehaviour {

	public SphereSwapper swapper;

	int table;
	int seat;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if(swapper != null) {
			int t = swapper.GetCurrentSphere().GetComponent<Controller>().GetTableNum();
			int s = swapper.GetCurrentSphere().GetComponent<Controller>().GetSeatNum();
			if (t != table || s != seat) {
				//Name changed, update label
				table = t;
				seat = s;
				GetComponent<Text>().text = table + "," + seat;
			}
		}
	}

	public void SetSwapper(SphereSwapper instructor) {
		swapper = instructor;
	}
}
