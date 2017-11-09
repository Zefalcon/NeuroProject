using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Controller : NetworkBehaviour {

	private Color changedColor;
	private Color defaultColor;
	private GameObject tableSelector;
	public List<NeuralInput> currentInputs;
	private float neuralInputThreshold = 10f;
	private float neuralInputThresholdHigh = 20f;
	private float last_firing = -100f;
	private float hardNeuralCooldown = 1f;
	private float softNeuralCooldown = 3f;

	public List<Connection> connectionsToOthers;
	public TableSpawnNetworkManager manager;
	public CameraSwapper swapper;

	[SyncVar]
	private int tableNum;
	[SyncVar]
	private int seatNum;
	[SyncVar]
	bool isInstructor = false;

	public int GetTableNum() {
		return tableNum;
	}

	public void SetTableNum(int table) {
		tableNum = table;
	}

	public int GetSeatNum() {
		return seatNum;
	}

	public void SetSeatNum(int seat) {
		seatNum = seat;
	}

	public void SetInstructor() {
		isInstructor = true;
	}

	public void SetThreshold(float threshold) {
		neuralInputThreshold = threshold;
	}

	public void SetHighThreshold(float threshold) {
		neuralInputThresholdHigh = threshold;
	}

	public void SetAbsRefractoryPd(float hardCooldown) {
		hardNeuralCooldown = hardCooldown;
	}

	public void SetRelRefractoryPd(float softCooldown) {
		softNeuralCooldown = softCooldown;
	}

	public class NeuralInput {
		float strength;
		float timeReceived;
		float waitTime = 1f;
		public NeuralInput(float strength, float timeReceived) {
			this.strength = strength;
			this.timeReceived = timeReceived;
		}
		public float Strength() {
			return strength;
		}
		public bool IsCurrent(float currentTime) {
			return (currentTime - timeReceived) < waitTime;
		}
	}

	// Use this for initialization
	void Start() {
		defaultColor = this.GetComponent<MeshRenderer>().material.color;
		changedColor = Color.green;

		currentInputs = new List<NeuralInput>();

		neuralInputThreshold = 10f;
		neuralInputThresholdHigh = 20f;
		last_firing = -100f;
		hardNeuralCooldown = 1f;
		softNeuralCooldown = 3f;

		if (isLocalPlayer) {
			manager = GameObject.FindObjectOfType<TableSpawnNetworkManager>();
			swapper = GameObject.FindObjectOfType<CameraSwapper>();
			transform.position = manager.GetSpawnPosition();
			Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
			int[] pos = manager.GetPositionIdentities(); //Fails (Instructor) if client
			tableNum = pos[0];
			seatNum = pos[1];
			isInstructor = manager.GetInstructorStatus();
			CmdApplyPositionNumbers(gameObject, tableNum, seatNum);
			CmdPlayerEntered(gameObject, tableNum, seatNum);
			if (isInstructor) { //Is set for local player
				//Engage Instructor Mode
				CmdEngageInstructorMode(gameObject);
				Camera.main.gameObject.transform.position = new Vector3(0, 0, -20); //Works regardless of server/client
			}
		}
	}

	[Command]
	void CmdPlayerEntered(GameObject obj, int table, int seat) {
		GameSave.PlayerEntered(obj, table, seat, false);
	}

	[Command]
	void CmdApplyPositionNumbers(GameObject obj, int table, int seat) {
		obj.GetComponent<Controller>().SetTableNum(table);
		obj.GetComponent<Controller>().SetSeatNum(seat);
	}

	[Command]
	void CmdEngageInstructorMode(GameObject obj) {
		obj.GetComponent<Controller>().SetInstructor();
		obj.GetComponent<MeshRenderer>().enabled = false; //Doesn't show on other clients, but does show on own if server
		obj.GetComponent<SphereCollider>().enabled = false; //Should not be clickable
		if (obj.GetComponentInChildren<CubeLabel>() != null) {
			obj.GetComponentInChildren<CubeLabel>().gameObject.SetActive(false);
		}
		obj.GetComponent<ConnectionManager>().isInstructor = true;
		RpcEngageInstructorMode(obj);
	}

	[ClientRpc]
	void RpcEngageInstructorMode(GameObject obj) {
		obj.GetComponent<Controller>().SetInstructor();
		obj.GetComponent<MeshRenderer>().enabled = false;
		obj.GetComponent<SphereCollider>().enabled = false;
		if (obj.GetComponentInChildren<CubeLabel>() != null) {
			obj.GetComponentInChildren<CubeLabel>().gameObject.SetActive(false);
		}
		obj.GetComponent<ConnectionManager>().isInstructor = true;
	}

	// Update is called once per frame
	void Update() {

		if (!isLocalPlayer) {
			return;
		}

		if (isInstructor) {
			//Ensure proper appearance
			CmdEngageInstructorMode(gameObject);

			//Instructors can click on players to set their refractory variables
			if (Input.GetMouseButtonUp(0)) {
				if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;

					if (Physics.Raycast(ray, out hit, 100)) {
						//Check if cube
						if (hit.transform.gameObject.GetComponent<Controller>() != null) {
							CmdOpenSetNeuronParameters(gameObject, hit.transform.gameObject);
						}
					}
				}
			}
		}

		if (isInstructor || isServer) {
			//Only instructor OR server can save/load the game
			if (Input.GetKeyDown(KeyCode.End) || Input.GetKeyDown(KeyCode.S)) {
				//Save the game
				CmdSaveGame(false);
			}
			if (Input.GetKeyDown(KeyCode.B)) {
				//Backup save
				CmdSaveGame(true);
			}
			if (Input.GetKeyDown(KeyCode.Return)) {
				//Load the game
				CmdLoadGame();
			}
			if (Input.GetKeyDown(KeyCode.LeftShift)) {
				//Load backup
				CmdLoadBackup();
			}
			if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) {
				//Remove all connections and start fresh
				CmdResetConnections();
			}
		}
		var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
		var y = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
		Camera.main.transform.Translate(x, 0, 0);
		Camera.main.transform.Translate(0, y, 0);

		/*Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

		var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
		var y = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

		transform.Translate(x, 0, 0);
		transform.Translate(0, y, 0);*/

		//Switch between main views and basic view
		if (!UIManager.inDialogue) {
			if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) {
				//Set to table 1 view
				swapper.SwapCamera(1);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) {
				//Set to table 2 view
				swapper.SwapCamera(2);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) {
				//Set to table 3 view
				swapper.SwapCamera(3);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) {
				//Set to table 4 view
				swapper.SwapCamera(4);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5)) {
				//Set to table 5 view
				swapper.SwapCamera(5);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6)) {
				//Set to table 6 view
				swapper.SwapCamera(6);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7)) {
				//Set to original view
				swapper.SwapCamera(7);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) {
				//Set to insect view
				swapper.SwapCamera(0);
			}
		}

		if (!isInstructor) {

			//Check if manually fired
			if (Input.GetKeyDown(KeyCode.Space)) {
				last_firing = Time.time;
				ColorChangeParser(new ColorChanger(transform.gameObject, changedColor), false);
			}

			//Check if fired due to connections
			float threshold = float.MaxValue;
			if (Time.time < last_firing + hardNeuralCooldown) {
				//Do NOT fire.  Keep threshold at infinity
			}
			else if (Time.time < last_firing + hardNeuralCooldown + softNeuralCooldown) {
				//Threshold is high.
				threshold = neuralInputThresholdHigh;
			}
			else {
				//Threshold is normal
				threshold = neuralInputThreshold;
			}
			float combinedInputStrength = 0;
			for (int i = 0; i < currentInputs.Count; i++) {
				if (currentInputs[i].IsCurrent(Time.time)) {
					combinedInputStrength += currentInputs[i].Strength();
				}
				else {
					//Remove old input to streamline
					currentInputs.Remove(currentInputs[i]);
				}
			}
			if (combinedInputStrength >= threshold) {
				last_firing = Time.time;
				ColorChangeParser(new ColorChanger(transform.gameObject, changedColor), false);
			}
		}
	}

	[Command]
	public void CmdSaveGame(bool backup) {
		GameSave.SaveGame(backup);
	}

	[Command]
	public void CmdLoadGame() {
		GameSave.LoadGame();
	}

	[Command]
	public void CmdLoadBackup() {
		GameSave.LoadBackup();
	}

	[Command]
	public void CmdResetConnections() {
		GameSave.ResetConnections();
		RpcResetConnections();
		Debug.Log("All connections reset.");
	}

	[ClientRpc]
	public void RpcResetConnections() {
		//TODO: Does this help?
		GameSave.ResetConnections();
	}

	[Command]
	public void CmdOpenSetNeuronParameters(GameObject instructor, GameObject neuron) {
		TargetOpenSetNeuronParameters(instructor.GetComponent<Controller>().connectionToClient, neuron);
	}

	[TargetRpc]
	public void TargetOpenSetNeuronParameters(NetworkConnection network, GameObject neuron) {
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenSetNeuronParametersBox(neuron);
	}

	public void SetNeuronParameters(GameObject neuron, float regularThreshold, float highThreshold, float absoluteRefractoryPeriod, float relativeRefractoryPeriod) {
		CmdSetNeuronParameters(neuron, regularThreshold, highThreshold, absoluteRefractoryPeriod, relativeRefractoryPeriod);
	}

	[Command]
	public void CmdSetNeuronParameters(GameObject neuron, float regularThreshold, float highThreshold, float absoluteRefractoryPeriod, float relativeRefractoryPeriod) {
		//Controller c = neuron.GetComponent<Controller>();
		//c.SetThreshold(regularThreshold);
		//c.SetHighThreshold(highThreshold);
		//c.SetAbsRefractoryPd(absoluteRefractoryPeriod);
		//c.SetRelRefractoryPd(relativeRefractoryPeriod);
		RpcSetNeuronParameters(neuron, regularThreshold, highThreshold, absoluteRefractoryPeriod, relativeRefractoryPeriod);
	}

	[ClientRpc]
	public void RpcSetNeuronParameters(GameObject neuron, float regularThreshold, float highThreshold, float absoluteRefractoryPeriod, float relativeRefractoryPeriod) {
		Controller c = neuron.GetComponent<Controller>();
		c.SetThreshold(regularThreshold);
		c.SetHighThreshold(highThreshold);
		c.SetAbsRefractoryPd(absoluteRefractoryPeriod);
		c.SetRelRefractoryPd(relativeRefractoryPeriod);
	}

	public void ColorChangeParser(ColorChanger toChange, bool delayed) {
		if (delayed) {
			StartCoroutine("DelayedColorChangeCommand", toChange);
		}
		else {
			CmdColorChange(toChange.GetObj(), toChange.GetColor());
		}

		for (int i = 0; i < connectionsToOthers.Count; i++) {
			if (connectionsToOthers[i].GetEnd() != null) {
				CmdNeuronFired(connectionsToOthers[i].GetEnd(), connectionsToOthers[i].connectionStrength);
			}
		}
	}

	public IEnumerator DelayedColorChangeCommand(ColorChanger toChange) {
		yield return new WaitForSeconds(0.1f);
		CmdColorChange(toChange.GetObj(), toChange.GetColor());
	}

	public IEnumerator ColorChange(ColorChanger toChange) {
		MeshRenderer renderer = toChange.GetObj().GetComponent<MeshRenderer>();
		Color color = toChange.GetColor();
		renderer.material.color = color;
		yield return new WaitForSeconds(0.5f);
		renderer.material.color = defaultColor;
	}

	public struct ColorChanger {
		GameObject obj;
		Color color;
		public GameObject GetObj() {
			return obj;
		}
		public Color GetColor() {
			return color;
		}
		public ColorChanger(GameObject o, Color c) {
			obj = o;
			color = c;
		}
	}

	[ClientRpc]
	void RpcColorChange(GameObject obj, Color toChange) {
		ColorChanger c = new ColorChanger(obj, toChange);
		StartCoroutine("ColorChange", c);
	}

	[Command]
	void CmdColorChange(GameObject obj, Color toChange) {
		RpcColorChange(obj, toChange);
	}

	[Command]
	void CmdNeuronFired(GameObject toInform, float strength) {
		TargetNeuronFired(toInform.GetComponent<Controller>().connectionToClient, strength);
	}

	[TargetRpc]
	void TargetNeuronFired(NetworkConnection toInform, float strength) {
		GameObject informed = toInform.playerControllers[0].gameObject;
		informed.GetComponent<Controller>().currentInputs.Add(new NeuralInput(strength, Time.time));
	}

	public void RemoveConnection(GameObject removeFrom, GameObject toRemove) {
		CmdRemoveConnection(removeFrom, toRemove);
	}

	[Command]
	void CmdRemoveConnection(GameObject obj, GameObject toRemove) {
		obj.GetComponent<Controller>().connectionsToOthers.Remove(toRemove.GetComponent<Connection>());
		RpcRemoveConnection(obj, toRemove);
	}

	[ClientRpc]
	void RpcRemoveConnection(GameObject obj, GameObject toRemove) {
		if(obj != null) {
			List<Connection> connections = obj.GetComponent<Controller>().connectionsToOthers;
			if(connections != null && toRemove != null) {
				connections.Remove(toRemove.GetComponent<Connection>());
			}
		}
		NetworkServer.Destroy(toRemove);
	}
}
