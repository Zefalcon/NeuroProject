using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwapper : MonoBehaviour {

	public Transform[] camPositions;
	Vector3 originalPosition;
	public Camera standardCamera;

	private void Start() {
		originalPosition = standardCamera.transform.position;
	}

	public void SwapCamera(int camera) {
		//Swap cameras position.  0 is original camera, 1-6 are the 6 tables, and 7 is the insect cam.
		if(camera == 0) {
			//Original cam
			Camera.main.transform.position = originalPosition;
		}
		else if(camera > 0 && camera < 8) {
			//Table or insect cam
			Camera.main.transform.position = camPositions[camera - 1].position;
		}
	}

	public void SetStartingPosition(Vector3 start) {
		originalPosition = start;
	}
}
