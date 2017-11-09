using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectLeg : MonoBehaviour {

	public enum Direction {
		Forward, Backward, FootUp, FootDown, Still
	}

	public int forward; //Which direction is "forward"?  1 if positive z rotation, -1 if negative z rotation

	Direction legMotion = Direction.Still;
	Direction footPosition = Direction.FootDown;

	public GameObject foot;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		switch (legMotion) {
			case (Direction.Forward): {
					transform.Rotate(Vector3.back, forward);
					break;
				}
			case (Direction.Backward): {
					transform.Rotate(Vector3.back, -forward);
					break;
				}
			case (Direction.Still): {
					transform.Rotate(Vector3.back, 0f);
					break;
				}
		}
		switch (footPosition) {
			case (Direction.FootDown): {
					foot.SetActive(true);
					break;
				}
			case (Direction.FootUp): {
					foot.SetActive(false);
					break;
				}
		}

		//Testing
		/*if (Input.GetKeyDown(KeyCode.F)) {
			RotateForward();
		}
		if (Input.GetKeyDown(KeyCode.B)) {
			RotateBackward();
		}
		if (Input.GetKeyDown(KeyCode.U)) {
			SetFootUp();
		}
		if (Input.GetKeyDown(KeyCode.D)) {
			SetFootDown();
		}*/
	}

	public void RotateForward() {
		legMotion = Direction.Forward;
	}

	public void RotateBackward() {
		legMotion = Direction.Backward;
	}

	public void SetFootDown() {
		footPosition = Direction.FootDown;
	}

	public void SetFootUp() {
		footPosition = Direction.FootUp;
	}
}
