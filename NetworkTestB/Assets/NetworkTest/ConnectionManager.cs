using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionManager : NetworkBehaviour {

	Controller cubeInstance;
	public GameObject excitatoryConnectionPrefab;
	public GameObject inhibitoryConnectionPrefab;
	public GameObject muscleConnectionPrefab;
	public GameObject sensorConnectionPrefab;
	public bool isInstructor = false;

	//Variables for choosing what controls are available (mostly for Instructor-spawned neurons)
	private bool canConnect = true;

	private bool isBeingControlled = false; //For Instructor-spawned neurons.

	public void SetCanConnect(bool canHazConnection) {
		canConnect = canHazConnection;
	}

	public void TransferControlAway() {
		//Transfers control away from this Sphere.
		SetCanConnect(false);
		isBeingControlled = false;
		cubeInstance.TransferControlAway();
	}

	public void TransferControlTo(bool isInstructorSphere) {
		//Transfers control to this Sphere.
		SetCanConnect(!isInstructorSphere); //If the original instructor sphere, shouldn't be able to connect to things.
		isBeingControlled = true;
		cubeInstance.TransferControlTo(isInstructorSphere);
	}

	public void SetCubeInstance(Controller neuron) {
		//Sets "cube" instance to proper neuron
		cubeInstance = neuron;
	}

	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			cubeInstance = this.GetComponent<Controller>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer || isInstructor) {
			if (!isBeingControlled) { //If instructor isn't currently controlling.
				return;
			}
		}

		//Left click creates excitatory connection
		if (Input.GetMouseButtonUp(0)) {
			if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100)) {
					//Check if neuron
					if (canConnect && hit.transform.gameObject.GetComponent<Controller>() != null) {
						if (hit.transform.gameObject.Equals(cubeInstance.transform.gameObject)) {
							//Same object; don't form connection
							return;
						}
						CmdAskSpawnConnection(cubeInstance.transform.gameObject, hit.transform.gameObject);
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

					//Check if sensor connection
					SensorConnection toDeleteS = hit.transform.gameObject.GetComponent<SensorConnection>();
					if(toDeleteS != null) {
						//Check if client is connected to sensor
						if (toDeleteS.GetEnd().Equals(cubeInstance.transform.gameObject)){
							//Send delete request
							AskDeleteSensorConnection(toDeleteS.gameObject);
						}
					}

					//Check if muscle connection
					MuscleConnection toDeleteM = hit.transform.gameObject.GetComponent<MuscleConnection>();
					if(toDeleteM != null) {
						//Check if client is connected to sensor
						if (toDeleteM.GetStart().Equals(cubeInstance.transform.gameObject)) {
							//Send delete request
							AskDeleteMuscleConnection(toDeleteM.gameObject);
						}
					}

					//Check if sensor
					if(canConnect && hit.transform.gameObject.GetComponent<SensorConnector>() != null) {
						//Connect to sensor
						CmdSensorConnection(cubeInstance.transform.gameObject, hit.transform.gameObject);
					}

					//Check if muscle
					if(canConnect && hit.transform.gameObject.GetComponent<Muscle>() != null) {
						//Connect to muscle
						CmdMuscleConnection(cubeInstance.transform.gameObject, hit.transform.gameObject);
					}
				}
			}
		}
	}

	public void SensorConnection(GameObject neuron, GameObject sensor) {
		if (isLocalPlayer) {
			//Don't want to send multiple times, probably.  Super double check.
			CmdSensorConnection(neuron, sensor);
		}
	}

	[Command]
	void CmdSensorConnection(GameObject neuron, GameObject sensor) {
		//Spawn connection to Sensor
		GameObject con = Instantiate(sensorConnectionPrefab);
		con.name = "Sensor Connection: " + sensor.name + "->" + neuron.name;
		con.transform.SetParent(GameObject.Find("NewNetworkManager").transform);
		NetworkServer.Spawn(con);
		con.GetComponent<SensorConnection>().SetPoints(sensor, neuron);
		GameSave.SensorConnectionMade(con.GetComponent<SensorConnection>());
		RpcSensorConnection(neuron, sensor, con);
	}

	[ClientRpc]
	void RpcSensorConnection(GameObject neuron, GameObject sensor, GameObject connection) {
		connection.GetComponent<SensorConnection>().SetPoints(sensor, neuron);
		LineRenderer lr = connection.GetComponent<LineRenderer>();
		if(lr == null) {
			Debug.Log("No line renderer");
		}
		lr.SetPosition(0, sensor.transform.position);
		lr.SetPosition(1, neuron.transform.position);

		sensor.GetComponent<SensorConnector>().AttachToSensor(neuron.GetComponent<Controller>());
	}

	public void MuscleConnection(GameObject neuron, GameObject muscle) {
		if (isLocalPlayer) {
			//Don't want to send multiple times, probably.  Super double check.
			CmdMuscleConnection(neuron, muscle);
		}
	}

	[Command]
	void CmdMuscleConnection(GameObject neuron, GameObject muscle) {
		//Spawn connection to Muscle
		GameObject con = Instantiate(muscleConnectionPrefab);
		con.name = "Muscle Connection: " + neuron.name + "->" + muscle.name;
		con.transform.SetParent(GameObject.Find("NewNetworkManager").transform);
		NetworkServer.Spawn(con);
		con.GetComponent<MuscleConnection>().SetPoints(neuron, muscle);
		GameSave.MuscleConnectionMade(con.GetComponent<MuscleConnection>());
		RpcMuscleConnection(neuron, muscle, con);

	}

	[ClientRpc]
	void RpcMuscleConnection(GameObject neuron, GameObject muscle, GameObject connection) {
		neuron.GetComponent<Controller>().muscleConnections.Add(connection.GetComponent<MuscleConnection>());
		connection.GetComponent<MuscleConnection>().SetPoints(neuron, muscle);
		LineRenderer lr = connection.GetComponent<LineRenderer>();
		lr.SetPosition(0, neuron.transform.position);
		lr.SetPosition(1, muscle.transform.position);

	}

	void AskDeleteConnection(GameObject toDelete) {
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenDeleteConnectionBox(toDelete);
	}

	void AskDeleteSensorConnection(GameObject toDelete) {
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenDeleteSensorConnectionBox(toDelete);
	}

	void AskDeleteMuscleConnection(GameObject toDelete) {
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenDeleteMuscleConnectionBox(toDelete);
	}

	public void DeleteConnection(GameObject toDelete) {
		CmdDeleteConnection(toDelete);
	}

	public void DeleteSensorConnection(GameObject toDelete) {
		CmdDeleteSensorConnection(toDelete);
	}

	public void DeleteMuscleConnection(GameObject toDelete) {
		CmdDeleteMuscleConnection(toDelete);
	}

	[Command]
	void CmdDeleteConnection(GameObject toDelete) {
		Connection deleteConnection = toDelete.GetComponent<Connection>();
		Controller presynapse = deleteConnection.GetStart().GetComponent<Controller>();
		GameSave.ConnectionRemoved(deleteConnection);
		presynapse.RemoveConnection(presynapse.gameObject, toDelete);
	}

	[Command]
	void CmdDeleteSensorConnection(GameObject toDelete) {
		SensorConnection deleteConnection = toDelete.GetComponent<SensorConnection>();
		SensorConnector presynapse = deleteConnection.GetStart().GetComponent<SensorConnector>();
		presynapse.DetachFromSensor(deleteConnection.GetEnd().GetComponent<Controller>());
		RpcDeleteSensorConnection(toDelete);
		GameSave.SensorConnectionRemoved(deleteConnection);
		deleteConnection.Destroy(false);
	}

	[ClientRpc]
	void RpcDeleteSensorConnection(GameObject toDelete) {
		if (toDelete != null) {
			SensorConnection deleteConnection = toDelete.GetComponent<SensorConnection>();
			SensorConnector presynapse = deleteConnection.GetStart().GetComponent<SensorConnector>();
			presynapse.DetachFromSensor(deleteConnection.GetEnd().GetComponent<Controller>());
		}
	}

	[Command]
	void CmdDeleteMuscleConnection(GameObject toDelete) {
		MuscleConnection deleteConnection = toDelete.GetComponent<MuscleConnection>();
		Controller presynapse = deleteConnection.GetStart().GetComponent<Controller>();
		GameSave.MuscleConnectionRemoved(deleteConnection);
		presynapse.RemoveMuscleConnection(presynapse.gameObject, toDelete);
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
	void CmdAskSpawnConnection(GameObject start, GameObject end) {
		GameObject existingConnection = GameObject.Find("NewNetworkManager/Connection: " + start.name + "->" + end.name);
		if (existingConnection == null) {
			if(end.GetComponent<Controller>().connectionToClient == null) {
				//Instructor-spawned sphere.  Use actual network connection
				TargetConnectionRequest(end.GetComponent<Controller>().GetNetworkConnection(), start, end);
			}
			else {
				//Regular sphere
				TargetConnectionRequest(end.GetComponent<Controller>().connectionToClient, start, end);
			}
			//Proceed
		}
	}

	[TargetRpc]
	void TargetConnectionRequest(NetworkConnection network, GameObject start, GameObject end) {
		//Receiver accepts connection
		//Bring up interface for user to decide whether to connect
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenAcceptConnectionBox(start, end);
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
			//Use dummy strength.  Note: not sure if needed.
			con.GetComponent<Connection>().connectionStrength = 1;
		}
		con.name = "Connection: " + start.name + "->" + end.name;
		con.transform.SetParent(GameObject.Find("NewNetworkManager").transform);
		NetworkServer.Spawn(con);
		con.GetComponent<Connection>().SetPoints(start, end);
		GameSave.ConnectionMade(con.GetComponent<Connection>());
		RpcSpawnConnection(start, end, con, str);
	}
}
