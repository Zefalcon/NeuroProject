using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Controller : NetworkBehaviour {

	private Color changedColor;
	private Color defaultColor;
	private Color postConnectionColor;
	private GameObject tableSelector;

	public List<Connection> connectionsToOthers;
	public TableSpawnNetworkManager manager;

	[SyncVar]
	private Color currentColor;

	// Use this for initialization
	void Start () {
		defaultColor = this.GetComponent<MeshRenderer>().material.color;
		currentColor = defaultColor;
		changedColor = Color.green;
		postConnectionColor = Color.yellow;

		if (isLocalPlayer) {
			manager = GameObject.FindObjectOfType<TableSpawnNetworkManager>();
			transform.position = manager.GetSpawnPosition();
			/*if (GameObject.Find("TableSelectionArea")) {
				tableSelector = GameObject.Find("TableSelectionArea");
				tableSelector.SetActive(false);
				//GameObject.Find("TableSelectionArea").SetActive(false);
			}*/
		}
	}

	private void OnDestroy() {
		//GameObject.Find("TableSelectionArea").SetActive(true);
		//tableSelector.SetActive(true);
	}

	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer) {
			return;
		}

		//TODO: See if this works reasonably well.
		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

		if (Input.GetKeyDown(KeyCode.Space)) {
			if (currentColor.Equals(defaultColor)){
				currentColor = changedColor;
			}
			else {
				currentColor = defaultColor;
			}
		}

		CmdColorChange(transform.gameObject, currentColor);
		for(int i = 0; i < connectionsToOthers.Capacity; i++) {
			CmdColorChange(connectionsToOthers[i].GetEnd(), postConnectionColor);
		}

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
