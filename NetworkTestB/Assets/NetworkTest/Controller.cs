using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Controller : NetworkBehaviour {

	private Color changedColor;
	private Color defaultColor;
	

	[SyncVar]
	private Color currentColor;

	// Use this for initialization
	void Start () {
		defaultColor = this.GetComponent<MeshRenderer>().material.color;
		currentColor = defaultColor;
		changedColor = Color.green;
	}
	
	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer) {
			return;
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			if (currentColor.Equals(defaultColor)){
				currentColor = changedColor;
			}
			else {
				currentColor = defaultColor;
			}
		}

		CmdColorChange(transform.gameObject, currentColor);

		var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
		var y = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

		transform.Translate(x, 0, 0);
		transform.Translate(0, y, 0);
	}

	[ClientRpc]
	void RpcColorChange(GameObject obj, Color toChange) {
		obj.GetComponent<MeshRenderer>().material.color = toChange;
	}

	[Command]
	void CmdColorChange(GameObject obj, Color toChange) {
		RpcColorChange(obj, toChange);
	}
}
