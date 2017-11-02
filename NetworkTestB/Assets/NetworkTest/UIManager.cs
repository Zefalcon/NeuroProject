using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour {

	public GameObject acceptConnection;
	GameObject connectionStartRef;  //First come, first serve.
	GameObject connectionEndRef;
	bool excitatoryRef;
	public GameObject deleteConnection;
	GameObject connectionRef;
	public GameObject setNeuronParameters;
	GameObject neuronToSetRef;

	//Temporary variables because Buttons can only transfer one thing at a time
	float regThresholdTemp;
	float highThresholdTemp;
	float absRefPdTemp;
	float relRefPdTemp;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ApplyAcceptConnection(Text strength) {
		//Parse strength into positive/negative
		float str = float.Parse(strength.text);
		if (str > 0) {
			//Positive, excitatory
			excitatoryRef = true;
		}
		else if (str < 0) {
			//Negative, inhibitory
			excitatoryRef = false;
		}
		else {
			//Strength 0.  Do not spawn
			return;
		}
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<ConnectionManager>().AcceptConnection(connectionStartRef, connectionEndRef, strength.text, excitatoryRef);
	}

	public void ApplyDeleteConnection() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<ConnectionManager>().DeleteConnection(connectionRef);
	}

	public void ApplyRegularThreshold(Text threshold) {
		if(!float.TryParse(threshold.text, out regThresholdTemp)) {
			//Failed parsing.  Inform console
			Debug.LogError("Regular Threshold failed to parse.");
		}
	}

	public void ApplyHighThreshold(Text threshold) {
		if(!float.TryParse(threshold.text, out highThresholdTemp)) {
			//Failed parsing.  Inform console
			Debug.LogError("High Threshold failed to parse.");
		}
	}

	public void ApplyAbsoluteRefractoryPeriod(Text refPd) {
		if(!float.TryParse(refPd.text, out absRefPdTemp)) {
			//Failed parsing.  Inform console
			Debug.LogError("Absolute Refractory Period failed to parse.");
		}
	}

	public void ApplyRelativeRefractoryPeriod(Text refPd) {
		if (!float.TryParse(refPd.text, out relRefPdTemp)) {
			//Failed parsing.  Inform console
			Debug.LogError("Relative Refractory Period failed to parse.");
		}
	}

	public void ApplyNeuronParameters() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<Controller>().SetNeuronParameters(neuronToSetRef, regThresholdTemp, highThresholdTemp, absRefPdTemp, relRefPdTemp);
	}

	public void OpenAcceptConnectionBox(GameObject start, GameObject end) {
		connectionStartRef = start;
		connectionEndRef = end;
		acceptConnection.SetActive(true);
		Text message = GameObject.Find("AcceptConnectionText").GetComponent<Text>();
		message.text = start.name + " wants to connect to you.";
	}

	public void CloseAcceptConnectionBox() {
		acceptConnection.SetActive(false);
	}

	public void OpenDeleteConnectionBox(GameObject toDelete) {
		deleteConnection.SetActive(true);
		connectionRef = toDelete;
	}

	public void CloseDeleteConnectionBox() {
		deleteConnection.SetActive(false);
	}

	public void OpenSetNeuronParametersBox(GameObject neuron) {
		setNeuronParameters.SetActive(true);
		neuronToSetRef = neuron;
	}

	public void CloseSetNeuronParametersBox() {
		setNeuronParameters.SetActive(false);
	}
}
