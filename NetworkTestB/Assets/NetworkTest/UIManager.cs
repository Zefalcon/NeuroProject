using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour {

	public static bool inDialogue = false;

	#region References
	public GameObject acceptConnection;
	GameObject connectionStartRef;  //First come, first serve.
	GameObject connectionEndRef;
	bool excitatoryRef;
	public GameObject deleteConnection;
	GameObject connectionRef;
	public GameObject deleteSensorConnection;
	GameObject sensorConnectionRef;
	public GameObject deleteMuscleConnection;
	GameObject muscleConnectionRef;
	public GameObject setNeuronParameters;
	GameObject neuronToSetRef;
	public GameObject createSphere;
	#endregion

	#region Temp Button Variables
	//Temporary variables because Buttons can only transfer one thing at a time
	float regThresholdTemp;
	float highThresholdTemp;
	float absRefPdTemp;
	float relRefPdTemp;
	bool[] valuesToApply = { false, false, false, false };
	int table;
	int seat;
	#endregion

	#region Apply UI Methods and Helpers
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

	public void ApplyDeleteSensorConnection() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<ConnectionManager>().DeleteSensorConnection(sensorConnectionRef);
	}

	public void ApplyDeleteMuscleConnection() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<ConnectionManager>().DeleteMuscleConnection(muscleConnectionRef);
	}

	public void ApplyRegularThreshold(Text threshold) {
		if (threshold.text.Equals("")) {
			//APPLY NOTHING
			valuesToApply[0] = false;
			/*Text placeholder = GameObject.Find("PlaceholderRest").GetComponent<Text>();
			if(!float.TryParse(placeholder.text, out regThresholdTemp)) {
				//Failed parsing.  Inform console
				Debug.LogError("Regular Threshold failed to parse.");
			}*/
		}
		else if(!float.TryParse(threshold.text, out regThresholdTemp)) {
			//Failed parsing.  Inform console
			Debug.LogError("Regular Threshold failed to parse.");
		}
		else {
			//Successful parsing
			valuesToApply[0] = true;
		}
	}

	public void ApplyHighThreshold(Text threshold) {
		if (threshold.text.Equals("")) {
			//APPLY NOTHING
			valuesToApply[1] = false;
			/*Text placeholder = GameObject.Find("PlaceholderRecovery").GetComponent<Text>();
			if (!float.TryParse(placeholder.text, out highThresholdTemp)) {
				//Failed parsing.  Inform console
				Debug.LogError("High Threshold failed to parse.");
			}*/
		}
		else if (!float.TryParse(threshold.text, out highThresholdTemp)) {
			//Failed parsing.  Inform console
			Debug.LogError("High Threshold failed to parse.");
		}
		else {
			//Successful parsing
			valuesToApply[1] = true;
		}
	}

	public void ApplyAbsoluteRefractoryPeriod(Text refPd) {
		if (refPd.text.Equals("")) {
			//APPLY NOTHING
			valuesToApply[2] = false;
			/*Text placeholder = GameObject.Find("PlaceholderAbsolute").GetComponent<Text>();
			if (!float.TryParse(placeholder.text, out absRefPdTemp)) {
				//Failed parsing.  Inform console
				Debug.LogError("Absolute Refractory Period failed to parse.");
			}*/
		}
		else if (!float.TryParse(refPd.text, out absRefPdTemp)) {
			//Failed parsing.  Inform console
			Debug.LogError("Absolute Refractory Period failed to parse.");
		}
		else {
			//Successful parsing
			valuesToApply[2] = true;
		}
	}

	public void ApplyRelativeRefractoryPeriod(Text refPd) {
		if (refPd.text.Equals("")) {
			//APPLY NOTHING
			valuesToApply[3] = false;
			/*Text placeholder = GameObject.Find("PlaceholderRelative").GetComponent<Text>();
			if (!float.TryParse(placeholder.text, out relRefPdTemp)) {
				//Failed parsing.  Inform console
				Debug.LogError("Relative Refractory Period failed to parse.");
			}*/
		}
		else if (!float.TryParse(refPd.text, out relRefPdTemp)) {
			//Failed parsing.  Inform console
			Debug.LogError("Relative Refractory Period failed to parse.");
		}
		else {
			//Successful parsing
			valuesToApply[3] = true;
		}
	}

	//Only applies changed neuron parameters
	public void ApplyNeuronParameters() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<Controller>().SetNeuronParameters(neuronToSetRef, regThresholdTemp, highThresholdTemp, absRefPdTemp, relRefPdTemp, true, valuesToApply);
	}

	/*public void ApplyNeuronParameters() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<Controller>().SetNeuronParameters(neuronToSetRef, regThresholdTemp, highThresholdTemp, absRefPdTemp, relRefPdTemp, true);
	}*/

	public void SetTableNum(int tableNum) {
		table = tableNum;
	}

	public void SetSeatNum(int seatNum) {
		seat = seatNum;
	}

	public void ApplyCreateSphere() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<SphereSwapper>().SpawnSphere(table, seat);
	}
	#endregion

	#region Toggle UI Methods
	public void OpenAcceptConnectionBox(GameObject start, GameObject end) {
		connectionStartRef = start;
		connectionEndRef = end;
		acceptConnection.SetActive(true);
		inDialogue = true;
		Text message = GameObject.Find("AcceptConnectionText").GetComponent<Text>();
		message.text = start.name + " wants to connect to " + end.name + ".";
	}

	public void CloseAcceptConnectionBox() {
		acceptConnection.SetActive(false);
		inDialogue = false;
	}

	public void OpenDeleteConnectionBox(GameObject toDelete) {
		deleteConnection.SetActive(true);
		connectionRef = toDelete;
		inDialogue = true;
	}

	public void CloseDeleteConnectionBox() {
		deleteConnection.SetActive(false);
		inDialogue = false;
	}

	public void OpenDeleteSensorConnectionBox(GameObject toDelete) {
		deleteSensorConnection.SetActive(true);
		sensorConnectionRef = toDelete;
		inDialogue = true;
	}

	public void CloseDeleteSensorConnectionBox() {
		deleteSensorConnection.SetActive(false);
		inDialogue = false;
	}

	public void OpenDeleteMuscleConnectionBox(GameObject toDelete) {
		deleteMuscleConnection.SetActive(true);
		muscleConnectionRef = toDelete;
		inDialogue = true;
	}

	public void CloseDeleteMuscleConnectionBox() {
		deleteMuscleConnection.SetActive(false);
		inDialogue = false;
	}

	public void OpenSetNeuronParametersBox(GameObject neuron) {
		setNeuronParameters.SetActive(true);
		inDialogue = true;
		neuronToSetRef = neuron;
		Text message = GameObject.Find("EditingNeuronText").GetComponent<Text>();
		message.text = "Currently editing " + neuron.name;

		//Set placeholders to correct values
		Text restThresh = GameObject.Find("PlaceholderRest").GetComponent<Text>();
		restThresh.text = neuron.GetComponent<Controller>().GetThreshold().ToString();
		Text recoveryThresh = GameObject.Find("PlaceholderRecovery").GetComponent<Text>();
		recoveryThresh.text = neuron.GetComponent<Controller>().GetHighThreshold().ToString();
		Text absPd = GameObject.Find("PlaceholderAbsolute").GetComponent<Text>();
		absPd.text = neuron.GetComponent<Controller>().GetAbsRefractoryPd().ToString();
		Text relPd = GameObject.Find("PlaceholderRelative").GetComponent<Text>();
		relPd.text = neuron.GetComponent<Controller>().GetRelRefractoryPd().ToString();

		//Reset text
		GameObject.Find("TextRest").GetComponent<Text>().text = string.Empty;
		GameObject.Find("TextRecovery").GetComponent<Text>().text = string.Empty;
		GameObject.Find("TextAbsolute").GetComponent<Text>().text = string.Empty;
		GameObject.Find("TextRelative").GetComponent<Text>().text = string.Empty;
		GameObject.Find("RestingThresholdInput").GetComponent<InputField>().text = string.Empty;
		GameObject.Find("RecoveryThresholdInput").GetComponent<InputField>().text = string.Empty;
		GameObject.Find("AbsoluteRefractoryPeriodInput").GetComponent<InputField>().text = string.Empty;
		GameObject.Find("RelativeRefractoryPeriodInput").GetComponent<InputField>().text = string.Empty;
	}

	public void CloseSetNeuronParametersBox() {
		setNeuronParameters.SetActive(false);
		inDialogue = false;
	}

	public void OpenCreateSphereBox() {
		createSphere.SetActive(true);
		inDialogue = true;
	}

	public void CloseCreateSphereBox() {
		createSphere.SetActive(false);
		inDialogue = false;
	}
	#endregion
}
