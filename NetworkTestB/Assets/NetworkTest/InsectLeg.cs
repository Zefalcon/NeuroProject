using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InsectLeg : NetworkBehaviour {

	public enum FootPosition {
		FootUp, FootDown
	}

	public int forward; //Which direction is "forward"?  1 if positive z rotation, -1 if negative z rotation

	float speed = 0f;

	FootPosition footPosition = FootPosition.FootUp;
	bool positionChanged = false; //Ensure code only run if position just changed.
	public bool canRotateForward = true;
	public bool canRotateBackward = true;

	public GameObject foot;
	
	// Update is called once per frame
	void Update () {
		bool rotate = false;
		if(forward*speed > 0) {
			//Trying to move forward (positive speed and forward direction or negative speed and forward direction)
			if (canRotateForward) {
				rotate = true;
			}
		}
		else {
			//Trying to move backward (positive speed and negative forward direction or negative speed and positive forward direction)
			if (canRotateBackward) {
				rotate = true;
			}
		}

		if (rotate) {
			transform.Rotate(Vector3.back, speed);
		}

		if (positionChanged) {
			switch (footPosition) {
				case (FootPosition.FootDown): {
						SetFootPosition(gameObject, true);
						//CmdFootPosition(foot, true);
						break;
					}
				case (FootPosition.FootUp): {
						SetFootPosition(gameObject, false);
						//CmdFootPosition(foot, false);
						break;
					}
			}
			positionChanged = false;
		}
	}

	void SetFootPosition(GameObject leg, bool down) {
		//leg.GetComponent<InsectLeg>().foot.SetActive(down);
		//foot.SetActive(down);
		CmdFootPosition(leg, down);
	}

	[Command]
	void CmdFootPosition(GameObject leg, bool down) {
		leg.GetComponent<InsectLeg>().foot.SetActive(down);
		//foot.SetActive(down);
		RpcFootPosition(leg, down);
	}

	[ClientRpc]
	void RpcFootPosition(GameObject leg, bool down) {
		leg.GetComponent<InsectLeg>().foot.SetActive(down);
		//foot.SetActive(down);
	}

	public void RotateForward() {
		//Add motion in the forward direction
		speed += (forward * 0.2f);
	}

	public void RotateBackward() {
		//Add motion in the backward direction
		speed -= (forward * 0.2f);
	}

	public void Halt(bool stopForward) {
		if (stopForward) { //Forward motion to be stopped.  Apply backward motion.
			RotateBackward();
		}
		else { //Backward motion to be stopped. Apply forward motion.
			RotateForward();
		}
	}

	public void SetFootDown() {
		footPosition = FootPosition.FootDown;
		positionChanged = true;
	}

	public void SetFootUp() {
		footPosition = FootPosition.FootUp;
		positionChanged = true;
	}
}
