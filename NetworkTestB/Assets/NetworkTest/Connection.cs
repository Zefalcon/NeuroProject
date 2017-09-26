using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Connection : MonoBehaviour {

	bool setUp = false;
	GameObject startPt;
	GameObject endPt;
	LineRenderer lr;
	BoxCollider box;
	public float connectionStrength;

	// Use this for initialization
	void Start () {
		lr = GetComponent<LineRenderer>();
		box = GetComponent<BoxCollider>();
	}
	
	// Update is called once per frame
	void Update () {
		if (startPt && endPt) {
			setUp = true;
			Vector3 start = startPt.transform.position;
			Vector3 end = endPt.transform.position;

			lr.SetPosition(0, start);
			lr.SetPosition(1, end);

			//Orient BoxCollider
			//Math done by ZackOfAllTrades on the Unity3D Answers board
			float width = lr.startWidth;
			Vector3 colliderPt = (start - end) * 0.5f + end;
			float length = Vector3.Distance(start, end);
			float angle = Mathf.Atan2((end.y - start.y), (end.x - start.x));
			angle *= Mathf.Rad2Deg;

			box.transform.position = colliderPt;
			box.size = new Vector3(length, width, .1f);
			box.transform.rotation = Quaternion.Euler(0, 0, angle);
		}

		if(setUp && (startPt == null || endPt == null)) {
			//One of the endpoints has been destroyed.  Destroy this.
			//TODO: Eventually, save connection and try to restore when user reconnects.
			NetworkServer.Destroy(this.gameObject);
		}
	}

	public GameObject GetStart() {
		return startPt;
	}

	public GameObject GetEnd() {
		return endPt;
	}

	public void SetPoints(GameObject start, GameObject end) {
		startPt = start;
		endPt = end;
	}
}
