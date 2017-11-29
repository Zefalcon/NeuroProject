using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TableSpawnNetworkManager : NetworkManager {

	public GameObject TableSelector;
	public GameObject InstructorLogin; //Instructor login should have a textbox for password input and a button to input said password.

	int tableSelected = 0;
	int seatSelected = 0;
	bool isInstructor = false;
	[SerializeField]
	[Tooltip("The required password instructors must enter")]
	string password = "Neuron"; //Change this to change the password required.

	public static Vector3[] tableLocations;
	public static Vector3[] seatOffsets;

	/// <summary>
	/// Tables are a hexagon shape flat end toward the front.
	/// Seats, then, should be placed on the corners of the opposite hexagon (point towards the front of the room).
	/// This results in 1 "forward" seat, 2 "upper" seats, 2 "lower" seats, and 1 "backward" seat.
	/// The "forward" and "backward" seats will have offests of a full side in the y direction and nothing in th the x direction.
	/// The "upper" and "lower" seats will have an offset of 1/2 a side in the y direction and sqrt(3)/2 of a side in the x direction.
	/// This creates an easy method of finding world-space seat locations based off of the table location in-world.
	/// </summary>
	static float SIDE_OFFSET = 0.866f * 3; //Hexagonal offset in the horizontal(x) direction.  For seats 2/3(positive), 5/6(negative).
	static float HALF_UP_OFFSET = 0.5f * 3; //Hexagonal offset in the vertical direction for upper and lower sides.  For seats 2/6(positive), 3/5(negative).
	static float FULL_UP_OFFSET = 1f * 3; //Hexagonal offset in the vertical direction for forward and backward sides.  For seats 1(positive), 4(negative).

	/// <summary>
	/// Tables placed 2 units apart (1 unit horizontally from 0,0,0 each, and a max of 2 units vertically)
	/// </summary>
	static float VERTICAL_TABLE_OFFSET = 10f;
	static float HORIZONTAL_TABLE_OFFSET = 5f;

	private void Awake() {
		seatOffsets = new Vector3[7]; //7 both for ease of access and in case of extra students/instructors
		seatOffsets[0] = Vector3.zero;
		seatOffsets[1] = new Vector3(0, FULL_UP_OFFSET, 0);
		seatOffsets[2] = new Vector3(SIDE_OFFSET, HALF_UP_OFFSET, 0);
		seatOffsets[3] = new Vector3(SIDE_OFFSET, -HALF_UP_OFFSET, 0);
		seatOffsets[4] = new Vector3(0, -FULL_UP_OFFSET, 0);
		seatOffsets[5] = new Vector3(-SIDE_OFFSET, -HALF_UP_OFFSET, 0);
		seatOffsets[6] = new Vector3(-SIDE_OFFSET, HALF_UP_OFFSET, 0);

		tableLocations = new Vector3[7]; //7 both for ease of access and for the case of instructors
		tableLocations[0] = Vector3.zero;
		tableLocations[1] = new Vector3(-HORIZONTAL_TABLE_OFFSET, VERTICAL_TABLE_OFFSET, 0);
		tableLocations[2] = new Vector3(HORIZONTAL_TABLE_OFFSET, VERTICAL_TABLE_OFFSET, 0);
		tableLocations[3] = new Vector3(-HORIZONTAL_TABLE_OFFSET, 0, 0);
		tableLocations[4] = new Vector3(HORIZONTAL_TABLE_OFFSET, 0, 0);
		tableLocations[5] = new Vector3(-HORIZONTAL_TABLE_OFFSET, -VERTICAL_TABLE_OFFSET, 0);
		tableLocations[6] = new Vector3(HORIZONTAL_TABLE_OFFSET, -VERTICAL_TABLE_OFFSET, 0);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
		OnServerAddPlayer(conn, playerControllerId, null);
	}

	public override void OnClientConnect(NetworkConnection conn) {
		ClientScene.AddPlayer(conn, 0);
		TableSelector.gameObject.SetActive(false);
		InstructorLogin.gameObject.SetActive(false);
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {
		Vector3 location = tableLocations[tableSelected] + seatOffsets[seatSelected];

		GameObject player = GameObject.Instantiate(playerPrefab, location, Quaternion.identity);
		player.GetComponent<Controller>().SetSeatNum(seatSelected);
		player.GetComponent<Controller>().SetTableNum(tableSelected);
		player.GetComponent<Controller>().SetInstructor();
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
	}

	public override void OnStopHost() {
		TableSelector.gameObject.SetActive(true);
		InstructorLogin.gameObject.SetActive(true);
		if (UIManager.inDialogue) {
			//Turn off all dialogue boxes
			UIManager ui = GameObject.Find("UIManager").GetComponent<UIManager>();
			ui.CloseAcceptConnectionBox();
			ui.CloseCreateSphereBox();
			ui.CloseDeleteConnectionBox();
			ui.CloseDeleteMuscleConnectionBox();
			ui.CloseDeleteSensorConnectionBox();
			ui.CloseSetNeuronParametersBox();
		}
		base.OnStopHost();
	}

	public override void OnClientDisconnect(NetworkConnection conn) {
		TableSelector.gameObject.SetActive(true);
		InstructorLogin.gameObject.SetActive(true);
		if (UIManager.inDialogue) {
			//Turn off all dialogue boxes
			UIManager ui = GameObject.Find("UIManager").GetComponent<UIManager>();
			ui.CloseAcceptConnectionBox();
			ui.CloseCreateSphereBox();
			ui.CloseDeleteConnectionBox();
			ui.CloseDeleteMuscleConnectionBox();
			ui.CloseDeleteSensorConnectionBox();
			ui.CloseSetNeuronParametersBox();
		}
		base.OnClientDisconnect(conn);
	}

	public void SetTableNum(int table) {
		if (!isInstructor) {
			//Instrutors spawn at 0,0
			tableSelected = table;
		}
		else {
			tableSelected = 0;
		}
	}

	public void SetSeatNum(int seat) {
		if (!isInstructor) {
			//Instructors spawn at 0,0
			seatSelected = seat;
		}
		else {
			seatSelected = 0;
		}
	}

	public void SetInstructor(Text passcode) {
		if (passcode.text.Equals(password)) {
			//Password accepted.
			isInstructor = true;
			tableSelected = 0;
			seatSelected = 0;
			StartClient();
		}
		else {
			Debug.LogWarning("Incorrect password.");
		}
	}

	public bool GetInstructorStatus() {
		return isInstructor;
	}

	public Vector3 GetSpawnPosition() {
		Vector3 location = tableLocations[tableSelected] + seatOffsets[seatSelected];
		return location;
	}

	public int[] GetPositionIdentities() {
		int[] position = new int[2] { tableSelected, seatSelected };
		return position;
	}

	
}
