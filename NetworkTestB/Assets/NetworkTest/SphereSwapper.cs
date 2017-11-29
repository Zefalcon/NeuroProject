using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SphereSwapper : NetworkBehaviour {

	List<GameObject> spheres = new List<GameObject>();
	int instructorIndex;
	int currentIndex = 0;

	public GameObject spherePrefab;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (!UIManager.inDialogue) {
			if (Input.GetKeyDown(KeyCode.C)) {
				//Create new sphere
				CmdAskSpawnSphere(gameObject);
			}
			if (Input.GetKeyDown(KeyCode.Tab)) {
				//Swap spheres
				SwapToNextSphere();
			}
		}
	}

	public int GetCurrentIndex() {
		return currentIndex;
	}

	public GameObject GetCurrentSphere() {
		return spheres[currentIndex];
	}

	[Command]
	public void CmdAskSpawnSphere(GameObject instructor) {
		TargetAskSpawnSphere(instructor.GetComponent<Controller>().connectionToClient);
	}

	[TargetRpc]
	public void TargetAskSpawnSphere(NetworkConnection network) {
		GameObject.Find("UIManager").GetComponent<UIManager>().OpenCreateSphereBox();
	}

	public void SpawnSphere(int table, int seat) {
		CmdSpawnSphere(table, seat, gameObject);
	}

	[Command]
	void CmdSpawnSphere(int table, int seat, GameObject instructor) {
		GameObject sphere = Instantiate(spherePrefab);
		//Adjust position based on table and seat
		Vector3 position = TableSpawnNetworkManager.tableLocations[table] + TableSpawnNetworkManager.seatOffsets[seat];
		sphere.transform.position = position;

		NetworkServer.SpawnWithClientAuthority(sphere, instructor);
		GameSave.PlayerEntered(sphere, table, seat, false);
		sphere.GetComponent<Controller>().ApplyPositionNumbers(sphere, table, seat);
		sphere.name = table + "," + seat;
		sphere.GetComponent<Controller>().DisengageInstructorMode(sphere);
		sphere.GetComponent<Controller>().SetNetworkConnection(instructor.GetComponent<Controller>().connectionToClient);
		sphere.GetComponent<Controller>().SetPlayerIndex(currentIndex);
		AddNewSphere(false, sphere);
		TargetSpawnSphere(instructor.GetComponent<Controller>().connectionToClient, table, seat, sphere, instructor);
	}

	[TargetRpc]
	void TargetSpawnSphere(NetworkConnection network, int table, int seat, GameObject sphere, GameObject instructor) {
		instructor.GetComponent<SphereSwapper>().AddNewSphere(false, sphere);
		sphere.GetComponent<ConnectionManager>().SetCubeInstance(sphere.GetComponent<Controller>());
		Vector3 position = TableSpawnNetworkManager.tableLocations[table] + TableSpawnNetworkManager.seatOffsets[seat];
		sphere.transform.position = position;
		sphere.GetComponent<Controller>().DisengageInstructorMode(sphere);
		sphere.GetComponent<Controller>().ApplyPositionNumbers(sphere, table, seat);
		sphere.GetComponent<Controller>().SetNetworkConnection(network);
		sphere.GetComponent<Controller>().SetPlayerIndex(currentIndex);
		instructor.GetComponent<SphereSwapper>().SwapToNextSphere();
	}

	public void AddNewSphere(bool isInstructor, GameObject newSphere) {
		//Adds new sphere to the list of spheres
		if (isInstructor) {
			//Should only be the first sphere, but would technically work at any index.
			instructorIndex = spheres.Count; 
		}
		spheres.Add(newSphere);
		newSphere.GetComponent<Controller>().SetIsInstructorControlled();
	}

	public void SwapToNextSphere() {
		//Turn off previous sphere's controls.
		spheres[currentIndex].GetComponent<ConnectionManager>().TransferControlAway();
		currentIndex++;
		currentIndex = currentIndex % spheres.Count;

		//Turn on next sphere's controls
		bool isInstructor;
		if(currentIndex == instructorIndex) { //If changing to the instructor, must enable specific controls
			isInstructor = true;
		}
		else {
			isInstructor = false;
		}
		spheres[currentIndex].GetComponent<ConnectionManager>().TransferControlTo(isInstructor);

		//Adjust camera
		float sphereX = spheres[currentIndex].transform.position.x;
		float sphereY = spheres[currentIndex].transform.position.y;
		float sphereZ;
		if (isInstructor) {
			//Instructor camera is more zoomed out
			sphereZ = spheres[currentIndex].transform.position.z - 10;
		}
		else {
			sphereZ = spheres[currentIndex].transform.position.z;
		}
		Camera.main.transform.position = new Vector3(sphereX, sphereY, sphereZ - 10);
	}
}
