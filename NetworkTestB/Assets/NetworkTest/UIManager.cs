using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour {

	public GameObject acceptConnection;
	GameObject connectionStartRef;  //TODO: MAJOR TESTING to ensure this doesn't screw up things with multiple users trying to connect at once
	GameObject connectionEndRef;
	bool excitatoryRef;
	NetworkConnection askerRef;
	public GameObject deleteConnection;
	GameObject connectionRef;
	public GameObject awaitingResponse;

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

	public void ApplyResponseReceived() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		//player.GetComponent<ConnectionManager>().ResponseReceived(askerRef);
	}

	public void ApplyDeleteConnection() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		player.GetComponent<ConnectionManager>().DeleteConnection(connectionRef);
	}

	public void OpenAcceptConnectionBox(GameObject start, GameObject end, NetworkConnection asker) {
		connectionStartRef = start;
		connectionEndRef = end;
		askerRef = asker;
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

	public void OpenAwaitingResponseBox() {
		awaitingResponse.SetActive(true);
	}

	public void CloseAwaitingResponseBox() {
		awaitingResponse.SetActive(false);
	}
}
