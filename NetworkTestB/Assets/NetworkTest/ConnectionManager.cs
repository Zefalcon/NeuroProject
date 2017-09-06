using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionManager : NetworkBehaviour {

	Controller cubeInstance;
	public GameObject connectionPrefab;

	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			cubeInstance = this.GetComponent<Controller>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Testing
		if (!isLocalPlayer) {
			return;
		}

		if (Input.GetMouseButtonUp(0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 100)) {
				//Check if cube
				if (hit.transform.gameObject.GetComponent<Controller>() != null) {
					if (hit.transform.gameObject.Equals(cubeInstance.transform.gameObject)) {
						//Same object; don't form connection
						return;
					}
					//Check if connection already exists
					GameObject existingConnection = GameObject.Find("NetworkManager/Connection: " + cubeInstance.transform.gameObject.name + "->" + hit.transform.gameObject.name);
					if (existingConnection == null) {
						//Proceed
						CmdAskSpawnConnection(cubeInstance.transform.gameObject, hit.transform.gameObject);
					}
				}
				//Check if connection
				Connection toDelete = hit.transform.gameObject.GetComponent<Connection>();
				if (toDelete != null) {
					//Check if client is connected to said connection
					if (toDelete.GetStart().Equals(cubeInstance.transform.gameObject) ||
						toDelete.GetEnd().Equals(cubeInstance.transform.gameObject)) {
						//Send delete request
						AskDeleteConnection(toDelete.gameObject);
					}
				}
			}
		}
	}

	void AskDeleteConnection(GameObject toDelete) {
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenDeleteConnectionBox(toDelete);
	}

	public void DeleteConnection(GameObject toDelete) {
		CmdDeleteConnection(toDelete);
	}

	[Command]
	void CmdDeleteConnection(GameObject toDelete) {
		NetworkServer.Destroy(toDelete);
	}

	[ClientRpc]
	void RpcSpawnConnection(GameObject start, GameObject end, GameObject connection) {
		connection.GetComponent<Connection>().SetPoints(start, end);
		LineRenderer lr = connection.GetComponent<LineRenderer>();
		lr.SetPosition(0, start.transform.position);
		lr.SetPosition(1, end.transform.position);
	}

	[Command]
	void CmdAskSpawnConnection(GameObject start, GameObject end) {
		TargetConnectionRequest(end.GetComponent<Controller>().connectionToClient, start, end);
		TargetWaitForResponse(start.GetComponent<Controller>().connectionToClient);
		//NetworkServer.Spawn(con);
		//RpcSpawnConnection(start, end, con);
	}

	[TargetRpc]
	void TargetWaitForResponse(NetworkConnection network) {
		//GameObject.Find("UIManager").GetComponent<UIManager>().OpenAwaitingResponseBox();
	}

	public void ResponseReceived(NetworkConnection network) {
		if (isLocalPlayer) {
			TargetResponseReceived(network);
		}
	}

	[TargetRpc]
	void TargetResponseReceived(NetworkConnection network) {
		GameObject.Find("UIManager").GetComponent<UIManager>().CloseAwaitingResponseBox();
	}

	[TargetRpc]
	void TargetConnectionRequest(NetworkConnection network, GameObject start, GameObject end) {
		//Receiver accepts connection
		//Bring up interface for user to decide whether to connect
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenAcceptConnectionBox(start, end, start.GetComponent<Controller>().connectionToClient);
		//CmdAcceptConnection(start, end);
	}

	public void AcceptConnection(GameObject start, GameObject end, string strength) {
		if (isLocalPlayer) {
			//Don't want to send multiple times, probably.  Super double check.
			CmdAcceptConnection(start, end, strength);
		}
	}

	[Command]
	void CmdAcceptConnection(GameObject start, GameObject end, string strength) {
		GameObject con = Instantiate(connectionPrefab);
		float str;
		if(float.TryParse(strength, out str)) {
			con.GetComponent<Connection>().connectionStrength = str;
		}
		else {
			//Use dummy strength
			con.GetComponent<Connection>().connectionStrength = 1;
		}
		con.name = "Connection: " + start.name + "->" + end.name;
		con.transform.SetParent(GameObject.Find("NetworkManager").transform); //TODO: Doesn't fix problem of connections disappearing upon reconnection.  Must spend time on this.
		NetworkServer.Spawn(con);
		RpcSpawnConnection(start, end, con);
	}
}
