using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionManager : NetworkBehaviour {

	Controller cubeInstance;
	public GameObject excitatoryConnectionPrefab;
	public GameObject inhibitoryConnectionPrefab;

	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			cubeInstance = this.GetComponent<Controller>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) {
			return;
		}

		//Left click creates excitatory connection
		if (Input.GetMouseButtonUp(0)) {
			if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
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
						//GameObject existingConnection = GameObject.Find("NewNetworkManager/Connection: " + cubeInstance.transform.gameObject.name + "->" + hit.transform.gameObject.name);
						//if (existingConnection == null) {
							//Proceed
							CmdAskSpawnConnection(cubeInstance.transform.gameObject, hit.transform.gameObject, true);
						//}
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
		//Right click creates inhibitory connection
		else if (Input.GetMouseButtonUp(1)) {
			if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
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
						GameObject existingConnection = GameObject.Find("NewNetworkManager/Connection: " + cubeInstance.transform.gameObject.name + "->" + hit.transform.gameObject.name);
						if (existingConnection == null) {
							//Proceed
							CmdAskSpawnConnection(cubeInstance.transform.gameObject, hit.transform.gameObject, false);
						}
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
		Connection deleteConnection = toDelete.GetComponent<Connection>();
		Controller presynapse = deleteConnection.GetStart().GetComponent<Controller>();
		presynapse.RemoveConnection(presynapse.gameObject, toDelete);
		//presynapse.connectionsToOthers.Remove(deleteConnection);
	}

	[ClientRpc]
	void RpcSpawnConnection(GameObject start, GameObject end, GameObject connection, float strength) {
		connection.GetComponent<Connection>().connectionStrength = strength;
		start.GetComponent<Controller>().connectionsToOthers.Add(connection.GetComponent<Connection>());
		connection.GetComponent<Connection>().SetPoints(start, end);
		LineRenderer lr = connection.GetComponent<LineRenderer>();
		lr.SetPosition(0, start.transform.position);
		lr.SetPosition(1, end.transform.position);
	}

	[Command]
	void CmdAskSpawnConnection(GameObject start, GameObject end, bool isExcitatory) {
		GameObject existingConnection = GameObject.Find("NewNetworkManager/Connection: " + start.name + "->" + end.name);
		if (existingConnection == null) {
			TargetConnectionRequest(end.GetComponent<Controller>().connectionToClient, start, end, isExcitatory);
			TargetWaitForResponse(start.GetComponent<Controller>().connectionToClient);
			//Proceed
		}
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
	void TargetConnectionRequest(NetworkConnection network, GameObject start, GameObject end, bool isExcitatory) {
		//Receiver accepts connection
		//Bring up interface for user to decide whether to connect
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenAcceptConnectionBox(start, end, start.GetComponent<Controller>().connectionToClient, isExcitatory);
		//CmdAcceptConnection(start, end);
	}

	public void AcceptConnection(GameObject start, GameObject end, string strength, bool isExcitatory) {
		if (isLocalPlayer) {
			//Don't want to send multiple times, probably.  Super double check.
			CmdAcceptConnection(start, end, strength, isExcitatory);
		}
	}

	[Command]
	void CmdAcceptConnection(GameObject start, GameObject end, string strength, bool isExcitatory) {
		GameObject con;
		if (isExcitatory) {
			con = Instantiate(excitatoryConnectionPrefab);
		}
		else {
			con = Instantiate(inhibitoryConnectionPrefab);
		}
		float str;
		if(float.TryParse(strength, out str)) {
			con.GetComponent<Connection>().connectionStrength = str;
		}
		else {
			//Use dummy strength
			con.GetComponent<Connection>().connectionStrength = 1;
		}
		con.name = "Connection: " + start.name + "->" + end.name;
		con.transform.SetParent(GameObject.Find("NewNetworkManager").transform); //TODO: Doesn't fix problem of connections disappearing upon reconnection.  Must spend time on this.
		NetworkServer.Spawn(con);
		RpcSpawnConnection(start, end, con, str);
	}
}
