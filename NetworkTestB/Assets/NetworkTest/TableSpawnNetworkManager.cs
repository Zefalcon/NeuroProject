using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TableSpawnNetworkManager : NetworkManager {

	public GameObject TableSelector; //TableSelector should have 6 buttons and a text box.  Buttons determine which table player is at, text box determines what seat they are in.

	int tableSelected = 0;
	int seatSelected = 0;
	bool isInstructor = false;
	[SerializeField]
	[Tooltip("The required password instructors must enter")]
	string password = "Wingdings"; //Change this to change the password required.

	static Vector3[] tableLocations;
	static Vector3[] seatOffsets;

	/// <summary>
	/// Tables are a hexagon shape (I believe pointed end towards the front of the room; if not, will revise).
	/// Seats, then, should be placed on the corners of the opposite hexagon (flat towards the front of the room).
	/// This results in 2 seats on the "upper" plane, 2 on the "equatorial" plane, and 2 on the "lower" plane.  
	/// The upper and lower seats will have offsets of 1/2 a side in the x direction and sqrt(3)/2 of a side in the y direction.
	/// The equatorial seats, then, will have offsets only in the x direction of a full side length.
	/// This creates an easy method of finding world-space seat locations based off of the table location in-world.
	/// </summary>
	static float UP_OFFSET = 0.866f * 3; //Hexagonal offset in the vertical(y) direction.  For seats 1/2(positive), 4/5(negative).
	static float HALF_SIDE_OFFSET = 0.5f * 3; //Hexagonal offset in the horizontal direction for upper and lower sides.  For seats 2/4(positive), 1/5(negative).
	static float FULL_SIDE_OFFSET = 1f * 3; //Hexagonal offset in the horizontal direction for equatorial sides.  For seats 3(positive), 6(negative).

	/// <summary>
	/// Tables placed 2 units apart (1 unit horizontally from 0,0,0 each, and a max of 2 units vertically)
	/// </summary>
	static float VERTICAL_TABLE_OFFSET = 20f;
	static float HORIZONTAL_TABLE_OFFSET = 10f;

	private void Awake() {
		seatOffsets = new Vector3[7]; //7 both for ease of access and in case of extra students/instructors
		seatOffsets[0] = Vector3.zero;
		seatOffsets[1] = new Vector3(-HALF_SIDE_OFFSET, UP_OFFSET, 0);
		seatOffsets[2] = new Vector3(HALF_SIDE_OFFSET, UP_OFFSET, 0);
		seatOffsets[3] = new Vector3(FULL_SIDE_OFFSET, 0, 0);
		seatOffsets[4] = new Vector3(HALF_SIDE_OFFSET, -UP_OFFSET, 0);
		seatOffsets[5] = new Vector3(-HALF_SIDE_OFFSET, -UP_OFFSET, 0);
		seatOffsets[6] = new Vector3(-FULL_SIDE_OFFSET, 0, 0);

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
		//TableSelector = GameObject.Find("TableSelectionArea");
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader) {
		Vector3 location = tableLocations[tableSelected] + seatOffsets[seatSelected];

		GameObject player = GameObject.Instantiate(playerPrefab, location, Quaternion.identity);
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
	}

	public override void OnStopHost() {
		TableSelector.gameObject.SetActive(true);
		base.OnStopHost();
	}

	public override void OnClientDisconnect(NetworkConnection conn) {
		TableSelector.gameObject.SetActive(true);
		base.OnClientDisconnect(conn);
	}

	public void SetTableNum(int table) {
		tableSelected = table;
	}

	public void SetSeatNum(int seat) {
		seatSelected = seat;
	}

	public void SetInstructor(bool instructor, string passcode) {
		if (passcode.Equals(password)) {
			//Password accepted.
			isInstructor = instructor;
			tableSelected = 0;
			seatSelected = 0;
		}
		else {
			Debug.LogWarning("Incorrect password.");
		}
	}

	public Vector3 GetSpawnPosition() {
		Vector3 location = tableLocations[tableSelected] + seatOffsets[seatSelected];
		return location;
	}

	
}
