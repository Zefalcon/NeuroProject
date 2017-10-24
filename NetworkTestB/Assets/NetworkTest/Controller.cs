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

	[SyncVar]
	private int tableNum;
	[SyncVar]
	private int seatNum;

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
	void Start () {
		defaultColor = this.GetComponent<MeshRenderer>().material.color;
		changedColor = Color.green;

		currentInputs = new List<NeuralInput>();

		if (isLocalPlayer) {
			manager = GameObject.FindObjectOfType<TableSpawnNetworkManager>();
			transform.position = manager.GetSpawnPosition();
			int[] pos = manager.GetPositionIdentities();
			tableNum = pos[0];
			seatNum = pos[1];
			CmdApplyPositionNumbers(gameObject, tableNum, seatNum);
			CmdPlayerEntered(gameObject, tableNum, seatNum);
			//GameSave.PlayerEntered(gameObject, tableNum, seatNum, false);
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

	// Update is called once per frame
	void Update () {

		if (!isLocalPlayer) {
			return;
		}

		if (isServer) {
			//Only server can save/load the game
			//TODO: Make instructor, not server
			if (Input.GetKeyDown(KeyCode.End)) {
				//Save the game
				GameSave.SaveGame();
			}
			if (Input.GetKeyDown(KeyCode.Return)) {
				//Load the game
				GameSave.LoadGame();
			}
		}

		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

		//Check if manually fired
		if (Input.GetKeyDown(KeyCode.Space)) {
			last_firing = Time.time;
			ColorChangeParser(new ColorChanger(transform.gameObject, changedColor), false);
		}

		//Check if fired due to connections
		float threshold = float.MaxValue;
		if(Time.time < last_firing + hardNeuralCooldown) {
			//Do NOT fire.  Keep threshold at infinity
		}
		else if(Time.time < last_firing + hardNeuralCooldown + softNeuralCooldown) {
			//Threshold is maxxed.
			threshold = neuralInputThresholdHigh;
		}
		else {
			//Threshold is normal
			threshold = neuralInputThreshold;
		}
		float combinedInputStrength = 0;
		for (int i=0; i<currentInputs.Count; i++) {
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

		var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
		var y = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

		transform.Translate(x, 0, 0);
		transform.Translate(0, y, 0);
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
		//toInform.GetComponent<Controller>().currentInputs.Add(new NeuralInput(strength, Time.time));
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
		//obj.GetComponent<Controller>().connectionsToOthers.Remove(toRemove.GetComponent<Connection>());
		//GameSave.ConnectionRemoved(toRemove.GetComponent<Connection>());
		NetworkServer.Destroy(toRemove);
	}
}
