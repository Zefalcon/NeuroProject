using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Controller : NetworkBehaviour {

	private Color changedColor;
	private Color defaultColor;
	//private Color postConnectionColor;
	private GameObject tableSelector;
	public List<NeuralInput> currentInputs;
	private float neuralInputThreshold = 10f;

	public List<Connection> connectionsToOthers;
	public TableSpawnNetworkManager manager;

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
		//postConnectionColor = Color.yellow;

		currentInputs = new List<NeuralInput>();

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

		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

		//Check if manually fired
		if (Input.GetKeyDown(KeyCode.Space)) {
			ColorChangeParser(new ColorChanger(transform.gameObject, changedColor), false);
			//CmdColorChange(transform.gameObject, changedColor);
			//for (int i = 0; i < connectionsToOthers.Count; i++) {
			//	StartCoroutine("DelayedColorChangeCommand", new ColorChanger(connectionsToOthers[i].GetEnd(), postConnectionColor));
			//}
		}

		//Check if fired due to connections
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
		if (combinedInputStrength >= neuralInputThreshold) {
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
			CmdNeuronFired(connectionsToOthers[i].GetEnd(), connectionsToOthers[i].connectionStrength);
			Debug.Log("Strength: " + connectionsToOthers[i].connectionStrength);
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
}
