using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Controller : NetworkBehaviour {

	private Color changedColor;
	private Color defaultColor;
	private GameObject tableSelector;
	public List<NeuralInput> currentInputs;
	[SyncVar]
	private float neuralInputThreshold = 10f;
	[SyncVar]
	private float neuralInputThresholdHigh = 20f;
	private float last_firing = -100f;
	[SyncVar]
	private float hardNeuralCooldown = 1f;
	[SyncVar]
	private float softNeuralCooldown = 3f;

	public List<Connection> connectionsToOthers;
	public List<MuscleConnection> muscleConnections;
	public TableSpawnNetworkManager manager;
	public CameraSwapper swapper;

	//Variables for choosing what controls are available (primarily for instructors to spawn and control neurons properly)
	private bool instructorCanClick = true;
	private bool canFire = true;

	private bool isBeingControlled = false; //For Instructor-controlled neurons
	private bool isInstructorControlled = false; //For Instructor-controlled neurons
	private int playerIndex = 0;

	private NetworkConnection actualNetworkConnection; //For use in instructor-spawned neurons.

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

	public bool IsInstructor() {
		return isInstructor;
	}

	public void SetClient() {
		isInstructor = false;
	}

	public void SetThreshold(float threshold) {
		neuralInputThreshold = threshold;
	}

	public float GetThreshold() {
		return neuralInputThreshold;
	}

	public void SetHighThreshold(float threshold) {
		neuralInputThresholdHigh = threshold;
	}

	public float GetHighThreshold() {
		return neuralInputThresholdHigh;
	}

	public void SetAbsRefractoryPd(float hardCooldown) {
		hardNeuralCooldown = hardCooldown;
	}

	public float GetAbsRefractoryPd() {
		return hardNeuralCooldown;
	}

	public void SetRelRefractoryPd(float softCooldown) {
		softNeuralCooldown = softCooldown;
	}

	public float GetRelRefractoryPd() {
		return softNeuralCooldown;
	}

	public void SetInstructorCanClick(bool canClick) {
		instructorCanClick = canClick;
	}

	public void SetCanFire(bool canHazFire) {
		canFire = canHazFire;
	}

	public void SetPlayerIndex(int index) {
		playerIndex = index;
	}

	public int GetPlayerIndex() {
		return playerIndex;
	}

	public void SetIsInstructorControlled() {
		//Sets the neuron to be "on" when the instructor is elsewhere.
		isInstructorControlled = true;
	}

	public void TransferControlAway() {
		//Transfers control away from this.
		SetInstructorCanClick(false);
		SetCanFire(false);
		isBeingControlled = false;
	}

	public void TransferControlTo(bool isInstructorSphere) {
		//Transfers control to this.
		SetInstructorCanClick(isInstructorSphere); //Sphere must be allowed to click as an instructor ONLY if it was originally the "instructor" sphere.
		SetCanFire(true);
		isBeingControlled = true;
	}

	public void SetNetworkConnection(NetworkConnection conn) {
		actualNetworkConnection = conn;
	}

	public NetworkConnection GetNetworkConnection() {
		return actualNetworkConnection;
	}

	public class NeuralInput {
		float strength;
		float timeReceived;
		float waitTime = 1f;
		public NeuralInput(float strength, float timeReceived) {
			this.strength = strength;
			this.timeReceived = timeReceived;
			Debug.Log("Input received: " + strength);
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
			CmdSetPlayerPosition(gameObject, manager.GetSpawnPosition());
			Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
			GameObject.Find("CameraSwapper").GetComponent<CameraSwapper>().SetStartingPosition(Camera.main.transform.position);
			int[] pos = manager.GetPositionIdentities();
			tableNum = pos[0];
			seatNum = pos[1];
			isInstructor = manager.GetInstructorStatus();
			isBeingControlled = true;
			CmdApplyPositionNumbers(gameObject, tableNum, seatNum);
			CmdPlayerEntered(gameObject, tableNum, seatNum);
			if (isInstructor) {
				//Engage Instructor Mode
				GetComponent<SphereSwapper>().enabled = true; //Enable SphereSwapper so instructor can create and swap between spheres.
				GetComponent<SphereSwapper>().AddNewSphere(true, gameObject); //Add instructor as a sphere that can be swapped to.
				CmdEngageInstructorMode(gameObject);
				Camera.main.gameObject.transform.position = new Vector3(0, 0, -20);
			}
		}
	}

	[Command]
	void CmdSetPlayerPosition(GameObject obj, Vector3 position) {
		obj.transform.position = position;
	}

	[Command]
	void CmdPlayerEntered(GameObject obj, int table, int seat) {
		GameSave.PlayerEntered(obj, table, seat, false);
	}

	//Note: Used only when instructor is creating a new sphere.  Corrects bad placement from leftover variables in the TableSpawnManager.
	public void ApplyPositionNumbers(GameObject obj, int table, int seat) {
		CmdApplyPositionNumbers(obj, table, seat);
	}

	[Command]
	void CmdApplyPositionNumbers(GameObject obj, int table, int seat) {
		obj.GetComponent<Controller>().SetTableNum(table);
		obj.GetComponent<Controller>().SetSeatNum(seat);
	}

	[Command]
	void CmdEngageInstructorMode(GameObject obj) {
		obj.GetComponent<Controller>().SetInstructor(); //TODO: Is this necessary?
		obj.GetComponent<MeshRenderer>().enabled = false;
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

	public void DisengageInstructorMode(GameObject obj) {
		CmdDisengageInstructorMode(obj);
	}

	[Command]
	void CmdDisengageInstructorMode(GameObject obj) {
		obj.GetComponent<Controller>().SetClient();
		obj.GetComponent<MeshRenderer>().enabled = true;
		obj.GetComponent<SphereCollider>().enabled = true;
		if(obj.GetComponentInChildren<CubeLabel>(true) != null) {
			obj.GetComponentInChildren<CubeLabel>(true).gameObject.SetActive(true);
		}
		obj.GetComponent<ConnectionManager>().isInstructor = false;
		RpcDisengageInstructorMode(obj);
	}

	[ClientRpc]
	void RpcDisengageInstructorMode(GameObject obj) {
		obj.GetComponent<Controller>().SetClient();
		obj.GetComponent<MeshRenderer>().enabled = true;
		obj.GetComponent<SphereCollider>().enabled = true;
		if (obj.GetComponentInChildren<CubeLabel>(true) != null) {
			obj.GetComponentInChildren<CubeLabel>(true).gameObject.SetActive(true);
		}
		obj.GetComponent<ConnectionManager>().isInstructor = false;
	}

	// Update is called once per frame
	void Update() {
		if(isInstructorControlled && !isBeingControlled) {
			//Make sure firing is checked even if instructor is controlling something else
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

		if (!isLocalPlayer) {
			if (!isBeingControlled) { //If not currently being controlled by the Instructor
				return;
			}
		}

		if (!isInstructor) {
			//Ensure proper appearance
			if (!GetComponent<MeshRenderer>().enabled || !GetComponentInChildren<CubeLabel>().gameObject.activeInHierarchy) { //Make sure appearance is only altered when necessary.
				CmdDisengageInstructorMode(gameObject);
			}
		}

		if (isInstructor) {
			//Ensure proper appearance
			if (GetComponent<MeshRenderer>().enabled) { //Make sure appearance is only altered when necessary.
				CmdEngageInstructorMode(gameObject);
			}

			//Instructors can click on players to set their refractory variables
			if (instructorCanClick && Input.GetMouseButtonUp(0)) {
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
			if (!UIManager.inDialogue) {
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
				if (Input.GetKeyDown(KeyCode.O)) {
					//Spawn all neurons in loaded file
					//GameSave.SpawnAllInstructorNeurons(GetComponent<SphereSwapper>());
					CmdSpawnAllNeurons(gameObject);
				}
			}
		}
		var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
		var y = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
		Camera.main.transform.Translate(x, 0, 0);
		Camera.main.transform.Translate(0, y, 0);

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
				//Set to insect view
				swapper.SwapCamera(7);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) {
				//Set to original view
				swapper.SwapCamera(0);
			}
		}

		if (!isInstructor) {

			//Check if manually fired
			if (canFire && Input.GetKeyDown(KeyCode.Space)) {
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
	public void CmdSpawnAllNeurons(GameObject swapper) {
		GameSave.SpawnAllInstructorNeurons(swapper.GetComponent<SphereSwapper>());
	}

	[Command]
	public void CmdOpenSetNeuronParameters(GameObject instructor, GameObject neuron) {
		TargetOpenSetNeuronParameters(instructor.GetComponent<Controller>().connectionToClient, neuron);
	}

	[TargetRpc]
	public void TargetOpenSetNeuronParameters(NetworkConnection network, GameObject neuron) {
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenSetNeuronParametersBox(neuron);
	}

	public void SetNeuronParameters(GameObject neuron, float regularThreshold, float highThreshold, float absoluteRefractoryPeriod, float relativeRefractoryPeriod, bool alertSave) {
		CmdSetNeuronParameters(neuron, regularThreshold, highThreshold, absoluteRefractoryPeriod, relativeRefractoryPeriod, alertSave);
	}

	[Command]
	public void CmdSetNeuronParameters(GameObject neuron, float regularThreshold, float highThreshold, float absoluteRefractoryPeriod, float relativeRefractoryPeriod, bool alertSave) {
		Controller c = neuron.GetComponent<Controller>();
		//Debug.Log("Command: " + regularThreshold);
		c.SetThreshold(regularThreshold);
		c.SetHighThreshold(highThreshold);
		c.SetAbsRefractoryPd(absoluteRefractoryPeriod);
		c.SetRelRefractoryPd(relativeRefractoryPeriod);
		if (alertSave) {
			//Is this a change from an instructor, or from a loaded save file?
			GameSave.NeuronParametersChanged(neuron.GetComponent<Controller>()); //TODO: Does this need to be in RPC, or is here best?
		}
		RpcSetNeuronParameters(neuron, regularThreshold, highThreshold, absoluteRefractoryPeriod, relativeRefractoryPeriod);
	}

	[ClientRpc]
	public void RpcSetNeuronParameters(GameObject neuron, float regularThreshold, float highThreshold, float absoluteRefractoryPeriod, float relativeRefractoryPeriod) {
		//Debug.Log("Client: " + regularThreshold);
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

		for (int i=0; i < muscleConnections.Count; i++) {
			if (muscleConnections[i].GetEnd() != null) {
				CmdNeuronFiredMuscle(muscleConnections[i].GetEnd());
				//muscleConnections[i].GetEnd().GetComponent<Muscle>().currentInputs.Add(new Muscle.Input(Time.time));  //TODO: Do we need this?
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
		if(toInform.GetComponent<Controller>().connectionToClient == null) {
			//Instructor-spawned
			TargetNeuronFired(toInform.GetComponent<Controller>().GetNetworkConnection(), toInform, strength);
		}
		else {
			TargetNeuronFired(toInform.GetComponent<Controller>().connectionToClient, toInform, strength);
		}
	}

	[TargetRpc]
	void TargetNeuronFired(NetworkConnection network, GameObject toInform, float strength) {
		toInform.GetComponent<Controller>().currentInputs.Add(new NeuralInput(strength, Time.time));
	}

	[Command]
	void CmdNeuronFiredMuscle(GameObject toInform) {
		Muscle informed = toInform.GetComponent<Muscle>();
		informed.currentInputs.Add(new Muscle.Input(Time.time));
		//RpcNeuronFiredMuscle(toInform);  //TODO: Do we need this?
	}

	[ClientRpc]
	void RpcNeuronFiredMuscle(GameObject toInform) {
		Muscle informed = toInform.GetComponent<Muscle>();
		informed.currentInputs.Add(new Muscle.Input(Time.time));
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

	public void RemoveMuscleConnection(GameObject removeFrom, GameObject toRemove) {
		CmdRemoveMuscleConnection(removeFrom, toRemove);
	}

	[Command]
	void CmdRemoveMuscleConnection(GameObject obj, GameObject toRemove) {
		obj.GetComponent<Controller>().muscleConnections.Remove(toRemove.GetComponent<MuscleConnection>());
		RpcRemoveMuscleConnection(obj, toRemove);
	}

	[ClientRpc]
	void RpcRemoveMuscleConnection(GameObject obj, GameObject toRemove) {
		if (obj != null) {
			List<MuscleConnection> connections = obj.GetComponent<Controller>().muscleConnections;
			if (connections != null && toRemove != null) {
				connections.Remove(toRemove.GetComponent<MuscleConnection>());
			}
		}
		NetworkServer.Destroy(toRemove);
	}
}
