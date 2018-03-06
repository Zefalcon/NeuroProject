using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SphereSwapper : NetworkBehaviour {

	public List<GameObject> spheres = new List<GameObject>();
	int instructorIndex = 0; //Is always 0, should not need to exist, but serves as a simple ref
	int currentIndex = 0;

	public GameObject spherePrefab;
	public SphereChangingLabel label;

	private void Start() {
		label = FindObjectOfType<SphereChangingLabel>();
		label.SetSwapper(this);
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
			//Testing - finalized for brevity
			if (Input.GetKeyDown(KeyCode.T)) {
				DeleteCurrentSphere();
			}
		}
	}

	public int GetCurrentIndex() {
		return currentIndex;
	}

	public GameObject GetCurrentSphere() {
		return spheres[currentIndex];
	}

	public int GetNumSpheres() {
		return spheres.Count;
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
		sphere.GetComponent<Controller>().ApplyPositionNumbers(sphere, table, seat);
		sphere.name = table + "," + seat;
		sphere.GetComponent<Controller>().DisengageInstructorMode(sphere);
		sphere.GetComponent<Controller>().SetNetworkConnection(instructor.GetComponent<Controller>().connectionToClient);
		AddNewSphere(false, sphere);
		//GameSave.PlayerEntered(sphere, table, seat, false);
		RpcSpawnSphere(table, seat, sphere, instructor);
		TargetSpawnSphere(instructor.GetComponent<Controller>().connectionToClient, table, seat, sphere, instructor);
	}

	[ClientRpc]
	void RpcSpawnSphere(int table, int seat, GameObject sphere, GameObject instructor) {
		//TODO: Might need to add more
		sphere.GetComponent<Controller>().DisengageInstructorMode(sphere);
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
		instructor.GetComponent<SphereSwapper>().SwapToSphere(instructor.GetComponent<SphereSwapper>().GetNumSpheres() - 1);
		InformOfEntrance(gameObject, sphere, table, seat); //Try here?
	}

	public void InformOfEntrance(GameObject swapper, GameObject neuron, int table, int seat) {
		swapper.GetComponent<SphereSwapper>().CmdInformOfEntrance(neuron, table, seat);
	}

	[Command]
	public void CmdInformOfEntrance(GameObject neuron, int table, int seat) {
		GameSave.PlayerEntered(neuron, table, seat, false);
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

	public void DeleteCurrentSphere() {
		DeleteSphere(currentIndex);
	}

	public void DeleteSphere(int sphereIndex) {
		if (sphereIndex <= currentIndex) {
			//currentIndex must be decremented
			currentIndex--;
		}
		else {
			//currentIndex is fine where it is
		}
		if (sphereIndex == instructorIndex) {
			//DO NOT DELETE
			return;
		}

		GameObject sphere = spheres[sphereIndex];
		spheres.Remove(sphere);
		//NetworkServer.Destroy(sphere);
		CmdDeleteSphere(sphere);
	}

	public void DeleteSphere(GameObject sphere) {
		//Deletes sphere from existance
		//Figure out what index sphere is at
		int sphereIndex = 0;

		for(int i=0; i<spheres.Count; i++) {
			Debug.Log(spheres[i] + " vs " + sphere);
			if (spheres[i].Equals(sphere)) {
				//TODO: probably fails
				sphereIndex = i;
			}
		}
		if (sphereIndex <= currentIndex) {
			//currentIndex must be decremented
			currentIndex--;		
		}
		else {
			//currentIndex is fine where it is
		}
		if(sphereIndex == instructorIndex) {
			//DO NOT DELETE
			return;
		}
		spheres.Remove(sphere);
		//NetworkServer.Destroy(sphere);
		CmdDeleteSphere(sphere);
	}

	[Command]
	void CmdDeleteSphere(GameObject sphere) {
		NetworkServer.Destroy(sphere);
	}

	public void SwapToSphere(int index) {
		//Turn off previous sphere's controls.
		/*for(int i=0; i<spheres.Count; i++) {
			spheres[i].GetComponent<ConnectionManager>().TransferControlAway();
		}*/
		spheres[currentIndex].GetComponent<ConnectionManager>().TransferControlAway();

		//Turn on next sphere's controls
		currentIndex = index;
		bool isInstructor;
		if (currentIndex == instructorIndex) { //If changing to the instructor, must enable specific controls
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
