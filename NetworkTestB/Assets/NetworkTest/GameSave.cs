using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Only connections need to be saved to a file.
/// Connections saved as strings in the form: Start(Tbl# Seat#) End(Tbl# Seat#) Str#
/// </summary>
public static class GameSave {

	class ConnectionFile {
		int startTable;
		int endTable;
		int startSeat;
		int endSeat;
		float strength;

		public ConnectionFile(int Stbl, int Etbl, int Sst, int Est, float str) {
			startTable = Stbl;
			endTable = Etbl;
			startSeat = Sst;
			endSeat = Est;
			strength = str;
		}

		public int[] GetStart() {
			return new int[] { startTable, startSeat };
		}

		public int[] GetEnd() {
			return new int[] { endTable, endSeat };
		}

		public float GetStrength() {
			return strength;
		}

		public bool isEndpoint(int table, int seat) {
			if (table.Equals(startTable) && seat.Equals(startSeat) || table.Equals(endTable) && seat.Equals(endSeat)) {
				return true;
			}
			return false;
		}

		public override string ToString() {
			return "Start: " + startTable + "," + startSeat + "  End: " + endTable + "," + endSeat + "  Strength: " + strength;
		}

		public override bool Equals(object obj) {
			if(obj.GetType() == typeof(ConnectionFile)) {
				ConnectionFile file = (ConnectionFile)obj;
				if (!file.GetStart()[0].Equals(startTable)){
					return false;
				}
				if (!file.GetStart()[1].Equals(startSeat)) {
					return false;
				}
				if (!file.GetEnd()[0].Equals(endTable)) {
					return false;
				}
				if (!file.GetEnd()[1].Equals(endSeat)) {
					return false;
				}
				if (!file.GetStrength().Equals(GetStrength())) {
					return false;
				}

				return true;
			}
			else {
				return false;
			}
		}
	}

	class NeuronDesignation {
		GameObject obj;
		int table;
		int seat;

		public NeuronDesignation(GameObject controller, int tableNum, int seatNum) {
			obj = controller;
			table = tableNum;
			seat = seatNum;
		}

		public GameObject GetObject() {
			return obj;
		}

		public int GetTableNum() {
			return table;
		}

		public int GetSeatNum() {
			return seat;
		}

		public bool isGivenNeuron(int tableNum, int seatNum) {
			return table == tableNum && seat == seatNum;
		}
	}

	static bool loaded = false;

	static List<NeuronDesignation> loadedPlayers = new List<NeuronDesignation>(); //Players already in the game before save file loaded.

	//static List<ConnectionFile> connectionsToSave = new List<ConnectionFile>(); //Connections added to this list as they are made.  Replaced with spawnedConnections.
	static List<ConnectionFile> connectionsToLoad = new List<ConnectionFile>(); //Connections added to this list from loaded save file.  As players join, connections to them are sent to the wait list to wait for the other end to join.
	static List<ConnectionFile> waitingConnections = new List<ConnectionFile>(); //Connections added to this list as players join.  When the player on the other end of the connection joins, these are spawned to the world.
	static List<ConnectionFile> spawnedConnections = new List<ConnectionFile>(); //Connections that have already been spawned in.  Respawn these when a new player connects (or reconnects) to ensure all connections appear for all players.

	static List<GameObject> connectionsToReset = new List<GameObject>(); //GameObjects of created connections must be saved so they can be reset appropriately.

	public static void SaveGame() {
		if (!Directory.Exists("Saves")) {
			Directory.CreateDirectory("Saves");
		}
		if (File.Exists("Saves/save.txt")) {
			//Save file already exists.  Inform user (TODO) and write over
			File.Delete("Saves/save.txt");
		}
		StreamWriter file = File.CreateText("Saves/save.txt");
		for (int i = 0; i < spawnedConnections.Count; i++) {
			StringBuilder builder = new StringBuilder();
			builder.Append("Start(Tbl" + spawnedConnections[i].GetStart()[0]);
			builder.Append(" Seat" + spawnedConnections[i].GetStart()[1]);
			builder.Append(") End(Tbl" + spawnedConnections[i].GetEnd()[0]);
			builder.Append(" Seat" + spawnedConnections[i].GetEnd()[1]);
			builder.Append(") Str" + spawnedConnections[i].GetStrength());
			file.WriteLine(builder.ToString());
		}
		file.Close();
		Debug.Log("Game Saved!");
	}

	public static void LoadGame() {
		StreamReader reader = new StreamReader("Saves/save.txt");
		while (!reader.EndOfStream) {
			ConnectionFile con;
			string line = reader.ReadLine();
			//Strip off the formatting to get to numbers
			line = line.Substring(9); //Start table number
			int startTable;
			if(!int.TryParse(line.Substring(0, 1), out startTable)) {
				Debug.LogError("Start Table format incorrect");
				continue;
			}
			//int startTable = int.Parse(line.Substring(0, 1));
			line = line.Substring(6); //Start seat number
			int startSeat;
			if(!int.TryParse(line.Substring(0,1), out startSeat)) {
				Debug.LogError("Start Seat format incorrect");
				continue;
			}
			//int startSeat = int.Parse(line.Substring(0, 1));
			line = line.Substring(10); //End table number
			int endTable;
			if (!int.TryParse(line.Substring(0, 1), out endTable)){
				Debug.LogError("End Table format incorrect");
				continue;
			}
			//int endTable = int.Parse(line.Substring(0, 1));
			line = line.Substring(6); //End seat number
			int endSeat;
			if(!int.TryParse(line.Substring(0,1), out endSeat)) {
				Debug.LogError("End Seat format incorrect");
				continue;
			}
			//int endSeat = int.Parse(line.Substring(0, 1));
			line = line.Substring(6); //Strength
			float strength;
			if(!float.TryParse(line, out strength)) {
				Debug.LogError("Strength format incorrect");
				continue;
			}
			//float strength = float.Parse(line);
			con = new ConnectionFile(startTable, endTable, startSeat, endSeat, strength);
			connectionsToLoad.Add(con);
		}
		reader.Close();
		Debug.Log("Game Loaded!");
		loaded = true;

		//Reset all connections.  Done once here so it isn't done several times in PlayerEntered.
		int endpoint = connectionsToReset.Count; //Must use outside endpoint to prevent editing list while iterating.
		for (int i = endpoint - 1; i >= 0; i--) { //Go in reverse so indices don't shift awkwardly.
			if (connectionsToReset[i] != null) { //If it isn't already gone, destroy it.
				connectionsToReset[i].GetComponent<Connection>().Destroy(false);
			}
		}

		connectionsToReset = new List<GameObject>();
		spawnedConnections = new List<ConnectionFile>(); //Must be reset on load so ghost connections don't pop back up.

		//Spawn connections for players already in scene.
		foreach (NeuronDesignation nd in loadedPlayers) {
			PlayerEntered(nd.GetObject(), nd.GetTableNum(), nd.GetSeatNum(), true);
		}
	}

	public static void PlayerEntered(GameObject obj, int table, int seat, bool preloaded) {
		//Save players so connections can load in later
		NeuronDesignation nu = new NeuronDesignation(obj, table, seat);
		//Ensure no duplicates
		if (!preloaded) {
			loadedPlayers.Add(nu);

			//Reset all connections 
			int endpoint = connectionsToReset.Count; //Must use outside endpoint to prevent editing list while iterating.
			for (int i = endpoint - 1; i >= 0; i--) { //Go in reverse so indices don't shift awkwardly
				if (connectionsToReset[i] != null) { //If it isn't gone already, destroy it.
					connectionsToReset[i].GetComponent<Connection>().Destroy(true);
				}
			}

			connectionsToReset = new List<GameObject>();

			//Respawn already-spawned connections.  //Only done on connect/reconnect, not load.
			for (int i = 0; i < spawnedConnections.Count; i++) {
				SpawnConnection(spawnedConnections[i]);
			}
		}
		else {
			//Load game is occurring.  Do NOT reset connections.  Has already been done in Load and should only be done once.
		}

		//Check waiting connections for new player
		List<ConnectionFile> toRemove = new List<ConnectionFile>();
		foreach (ConnectionFile file in waitingConnections) {
			//Check table and seat vs. start and end
			if (file.isEndpoint(table, seat)) {
				//Connection found!  Instantiate
				SpawnConnection(file);
				toRemove.Add(file);
			}
			//Otherwise, ignore connection for now
		}
		//Delete instantiated connections
		foreach (ConnectionFile file in toRemove) {
			waitingConnections.Remove(file);
		}

		//Check loaded connections for new player
		toRemove.Clear();
		foreach (ConnectionFile file in connectionsToLoad) {
			//Check table and seat vs. start and end
			if (file.isEndpoint(table, seat)) {
				//Connection found!  Wait for other end point
				waitingConnections.Add(file);
				toRemove.Add(file);
			}
		}
		//Delete moved connections
		foreach (ConnectionFile file in toRemove) {
			connectionsToLoad.Remove(file);
		}
	}

	public static void ConnectionMade(Connection con) {
		ConnectionFile file;
		Controller start = con.GetStart().GetComponent<Controller>();
		Controller end = con.GetEnd().GetComponent<Controller>();
		start = start.connectionToClient.playerControllers[0].gameObject.GetComponent<Controller>();
		end = end.connectionToClient.playerControllers[0].gameObject.GetComponent<Controller>();
		file = new ConnectionFile(start.GetTableNum(), end.GetTableNum(), start.GetSeatNum(), end.GetSeatNum(), con.connectionStrength);
		connectionsToReset.Add(con.gameObject);
		if (!spawnedConnections.Contains(file)) {
			spawnedConnections.Add(file);
		}
	}

	public static void ConnectionRemoved(Connection con) {
		ConnectionFile file;
		Controller start = con.GetStart().GetComponent<Controller>();
		Controller end = con.GetEnd().GetComponent<Controller>();
		start = start.connectionToClient.playerControllers[0].gameObject.GetComponent<Controller>();
		end = end.connectionToClient.playerControllers[0].gameObject.GetComponent<Controller>();
		file = new ConnectionFile(start.GetTableNum(), end.GetTableNum(), start.GetSeatNum(), end.GetSeatNum(), con.connectionStrength);

		int endpoint = connectionsToReset.Count;  //Avoid changing list while iterating.
		for(int i=endpoint - 1; i>=0; i--) {
			if (connectionsToReset[i].Equals(con.gameObject)) {
				connectionsToReset.Remove(con.gameObject);
			}
		}

		endpoint = spawnedConnections.Count;
		for(int i=endpoint - 1; i>=0; i--) {
			if (spawnedConnections[i].Equals(file)) {
				spawnedConnections.Remove(file);
			}
		}
	}

	//Helper method to spawn connections based on given connection file
	private static void SpawnConnection(ConnectionFile file) {
		//Find start and end gameObjects
		GameObject start = null, end = null;
		foreach (NeuronDesignation nd in loadedPlayers) {
			//Find start
			if (nd.isGivenNeuron(file.GetStart()[0], file.GetStart()[1])) {
				start = nd.GetObject();
			}
			//Find end
			else if (nd.isGivenNeuron(file.GetEnd()[0], file.GetEnd()[1])) {
				end = nd.GetObject();
			}
		}
		bool excitatory;
		if (file.GetStrength() > 0) {
			excitatory = true;
		}
		else {
			excitatory = false;
		}
		if (start && end) {
			GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
			player.GetComponent<ConnectionManager>().AcceptConnection(start, end, file.GetStrength().ToString(), excitatory);
		}
	}
}
