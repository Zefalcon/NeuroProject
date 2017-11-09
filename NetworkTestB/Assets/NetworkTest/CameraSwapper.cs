using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwapper : MonoBehaviour {

	public Camera[] tableCams;
	public Camera standardCamera;

	public void SwapCamera(int camera) {
		foreach (Camera c in tableCams) {
			c.enabled = false;
		}
		standardCamera.enabled = false;
		switch (camera) {
			case (1): {
					tableCams[0].enabled = true;
					break;
				}
			case (2): {
					tableCams[1].enabled = true;
					break;
				}
			case (3): {
					tableCams[2].enabled = true;
					break;
				}
			case (4): {
					tableCams[3].enabled = true;
					break;
				}
			case (5): {
					tableCams[4].enabled = true;
					break;
				}
			case (6): {
					tableCams[5].enabled = true;
					break;
				}
			case (0): {
					tableCams[6].enabled = true;
					break;
				}
			case (7): {
					standardCamera.enabled = true;
					break;
				}
			default: {
					standardCamera.enabled = true;
					break;
				}
		}
	}
}
