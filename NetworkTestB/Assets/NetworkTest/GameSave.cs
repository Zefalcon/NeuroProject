using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

/// <summary>
/// Only connections need to be saved to a file.
/// Connections saved as strings in the form: Start(Tbl# Seat#) End(Tbl# Seat#) Str#
/// </summary>
public static class GameSave {

	enum MuscleType {
		Forward, Backward, Stance
	}

	struct MuscleRef {
		public int table;
		public MuscleType type;
		
		public MuscleRef(int t, MuscleType m) {
			table = t;
			type = m;
		}
	}

	class MuscleConnectionFile {
		int muscleTable;
		MuscleType type;
		int startTable;
		int startSeat;

		public MuscleConnectionFile(int MusTbl, MuscleType MusType, int STbl, int SSt) {
			muscleTable = MusTbl;
			type = MusType;
			startTable = STbl;
			startSeat = SSt;
		}

		public int[] GetStart() {
			return new int[] { startTable, startSeat };
		}

		public MuscleRef GetEnd() {
			return new MuscleRef(muscleTable, type);
		}

		public bool isStartpoint(int table, int seat) {
			if (table.Equals(startTable) && seat.Equals(startSeat)) {
				return true;
			}
			else {
				return false;
			}
		}

		public bool isEndpoint(int table, MuscleType location) {
			if (table.Equals(muscleTable) && location.Equals(type)) {
				return true;
			}
			else {
				return false;
			}
		}

		public override string ToString() {
			return "Muscle: " + type + " " + muscleTable + "  Start: " + startTable + "," + startSeat;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override bool Equals(object obj) {
			if (obj.GetType() == typeof(MuscleConnectionFile)) {
				MuscleConnectionFile file = (MuscleConnectionFile)obj;
				if (!file.GetEnd().table.Equals(muscleTable)) {
					return false;
				}
				if (!file.GetEnd().type.Equals(type)) {
					return false;
				}
				if (!file.GetStart()[0].Equals(startTable)) {
					return false;
				}
				if (!file.GetStart()[1].Equals(startSeat)) {
					return false;
				}

				return true;
			}
			else {
				return false;
			}
		}
	}

	struct SensorRef {
		public int table;
		public Sensor.SensorLocation location;

		public SensorRef(int t, Sensor.SensorLocation l) {
			table = t;
			location = l;
		}
	}

	class SensorConnectionFile {
		int sensorTable;
		Sensor.SensorLocation sensorLocation;
		int endTable;
		int endSeat;

		public SensorConnectionFile(int SenTbl, Sensor.SensorLocation SenLoc, int ETbl, int ESt) {
			sensorTable = SenTbl;
			sensorLocation = SenLoc;
			endTable = ETbl;
			endSeat = ESt;
		}

		public SensorRef GetStart() {
			return new SensorRef(sensorTable, sensorLocation);
		}

		public int[] GetEnd() {
			return new int[] { endTable, endSeat };
		}

		public bool isEndpoint(int table, int seat) {
			if (table.Equals(endTable) && seat.Equals(endSeat)) {
				return true;
			}
			else {
				return false;
			}
		}

		public bool isStartpoint(int table, Sensor.SensorLocation location) {
			if(table.Equals(sensorTable) && location.Equals(sensorLocation)) {
				return true;
			}
			else {
				return false;
			}
		}

		public override string ToString() {
			return "Sensor: " + sensorLocation + " " + sensorTable + "  End: " + endTable + "," + endSeat;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override bool Equals(object obj) {
			if (obj.GetType() == typeof(SensorConnectionFile)) {
				SensorConnectionFile file = (SensorConnectionFile)obj;
				if (!file.GetStart().table.Equals(sensorTable)) {
					return false;
				}
				if (!file.GetStart().location.Equals(sensorLocation)) {
					return false;
				}
				if (!file.GetEnd()[0].Equals(endTable)) {
					return false;
				}
				if (!file.GetEnd()[1].Equals(endSeat)) {
					return false;
				}

				return true;
			}
			else {
				return false;
			}
		}
	}

	class ConnectionFile {
		int startTable;
		int endTable;
		int startSeat;
		int endSeat;
		float strength;

		public ConnectionFile(int STbl, int ETbl, int SSt, int ESt, float str) {
			startTable = STbl;
			endTable = ETbl;
			startSeat = SSt;
			endSeat = ESt;
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

		public override int GetHashCode() {
			return base.GetHashCode();
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

	class NeuronFile {
		int table;
		int seat;
		float restingThreshold;
		float recoveryThreshold;
		float absRefractoryPeriod;
		float relRefractoryPeriod;

		public NeuronFile(int tableNum, int seatNum, float restThresh, float recThresh, float ARP, float RRP) {
			table = tableNum;
			seat = seatNum;
			restingThreshold = restThresh;
			recoveryThreshold = recThresh;
			absRefractoryPeriod = ARP;
			relRefractoryPeriod = RRP;
		}

		public int GetTableNum() {
			return table;
		}

		public int GetSeatNum() {
			return seat;
		}

		public float GetRestingThreshold() {
			return restingThreshold;
		}

		public float GetRecoveryThreshold() {
			return recoveryThreshold;
		}

		public float GetAbsoluteRefractoryPeriod() {
			return absRefractoryPeriod;
		}

		public float GetRelativeRefractoryPeriod() {
			return relRefractoryPeriod;
		}
	}

	public static List<SensorConnector> sensors = new List<SensorConnector>(Resources.FindObjectsOfTypeAll<SensorConnector>());//GameObject.FindObjectsOfType<SensorConnector>()); //All the sensors
	public static List<Muscle> muscles = new List<Muscle>(Resources.FindObjectsOfTypeAll<Muscle>());//GameObject.FindObjectsOfType<Muscle>()); //All the muscles

	static List<NeuronDesignation> loadedPlayers = new List<NeuronDesignation>(); //Players already in the game before save file loaded.

	static List<ConnectionFile> connectionsToLoad = new List<ConnectionFile>(); //Connections added to this list from loaded save file.  As players join, connections to them are sent to the wait list to wait for the other end to join.
	static List<ConnectionFile> waitingConnections = new List<ConnectionFile>(); //Connections added to this list as players join.  When the player on the other end of the connection joins, these are spawned to the world.
	static List<ConnectionFile> spawnedConnections = new List<ConnectionFile>(); //Connections that have already been spawned in.  Respawn these when a new player connects (or reconnects) to ensure all connections appear for all players.

	static List<SensorConnectionFile> sensorConnectionsToLoad = new List<SensorConnectionFile>(); //As connectionsToLoad, but doesn't need the middle step
	static List<SensorConnectionFile> spawnedSensorConnections = new List<SensorConnectionFile>(); //Sensor connectiosn that have already been spawned in.

	static List<MuscleConnectionFile> muscleConnectionsToLoad = new List<MuscleConnectionFile>(); //As above
	static List<MuscleConnectionFile> spawnedMuscleConnections = new List<MuscleConnectionFile>();

	static List<NeuronFile> neuronSettingsToLoad = new List<NeuronFile>(); //Neuron parameters that have been loaded from save file.  As players join, adjust their settings appropriately
	static List<NeuronFile> adjustedNeuronSettings = new List<NeuronFile>(); //Parameters that have already been set.  Readjust when the player reconnects.

	static List<GameObject> connectionsToReset = new List<GameObject>(); //GameObjects of created connections must be saved so they can be reset appropriately.
	static List<GameObject> sensorConnectionsToReset = new List<GameObject>();
	static List<GameObject> muscleConnectionsToReset = new List<GameObject>();

	public static void SaveGame(bool backup) {
		if (!Directory.Exists("Saves")) {
			Directory.CreateDirectory("Saves");
		}

		if (backup) {
			//Save game with backup
			if (File.Exists("Saves/backup.txt")) {
				//Save file already exists.  Inform user (TODO) and write over
				File.Delete("Saves/backup.txt");
			}
			CreateSaveFile("Saves/backup.txt");
		}

		//Save game to regular file
		if (File.Exists("Saves/save.txt")) {
			File.Delete("Saves/save.txt");
		}
		CreateSaveFile("Saves/save.txt");
		Debug.Log("Game Saved!");
		if (backup) {
			Debug.Log("Backup Created!");
		}
	}

	private static void CreateSaveFile(string filename) {
		StreamWriter file = File.CreateText(filename);
		//Write all connections
		for (int i=0; i < spawnedConnections.Count; i++) {
			StringBuilder builder = new StringBuilder();
			builder.Append("Start(Tbl" + spawnedConnections[i].GetStart()[0]);
			builder.Append(" Seat" + spawnedConnections[i].GetStart()[1]);
			builder.Append(") End(Tbl" + spawnedConnections[i].GetEnd()[0]);
			builder.Append(" Seat" + spawnedConnections[i].GetEnd()[1]);
			builder.Append(") Str" + spawnedConnections[i].GetStrength());
			file.WriteLine(builder.ToString());
		}
		//Write all sensor connections
		for (int i=0; i < spawnedSensorConnections.Count; i++) {
			string location;
			switch (spawnedSensorConnections[i].GetStart().location) {
				case Sensor.SensorLocation.Backward: {
						location = "P";
						break;
					}
				case Sensor.SensorLocation.Forward: {
						location = "A";
						break;
					}
				default: {
						//So we have something
						location = "A";
						break;
					}
			}
			StringBuilder builder = new StringBuilder();
			builder.Append("Sensor(Tbl" + spawnedSensorConnections[i].GetStart().table);
			builder.Append(" Location " + location);
			builder.Append(") End(Tbl" + spawnedSensorConnections[i].GetEnd()[0]);
			builder.Append(" Seat" + spawnedSensorConnections[i].GetEnd()[1]);
			builder.Append(")");
			file.WriteLine(builder.ToString());
		}
		//Write all muscle connections
		for (int i=0; i < spawnedMuscleConnections.Count; i++) {
			string type;
			switch (spawnedMuscleConnections[i].GetEnd().type) {
				case MuscleType.Backward: {
						type = "B";
						break;
					}
				case MuscleType.Forward: {
						type = "F";
						break;
					}
				case MuscleType.Stance: {
						type = "D";
						break;
					}
				default: {
						//So we have something
						type = "D";
						break;
					}
			}
			StringBuilder builder = new StringBuilder();
			builder.Append("Muscle(Tbl" + spawnedMuscleConnections[i].GetEnd().table);
			builder.Append(" Type " + type);
			builder.Append(") Start(Tbl" + spawnedMuscleConnections[i].GetStart()[0]);
			builder.Append(" Seat" + spawnedMuscleConnections[i].GetStart()[1]);
			builder.Append(")");
			file.WriteLine(builder.ToString());
		}
		//Write all neuron parameters
		for (int i=0; i < adjustedNeuronSettings.Count; i++) {
			StringBuilder builder = new StringBuilder();
			builder.Append("Neuron(Tbl" + adjustedNeuronSettings[i].GetTableNum());
			builder.Append(" Seat" + adjustedNeuronSettings[i].GetSeatNum());
			builder.Append(") Rest" + adjustedNeuronSettings[i].GetRestingThreshold());
			builder.Append(" Rec" + adjustedNeuronSettings[i].GetRecoveryThreshold());
			builder.Append(" Abs" + adjustedNeuronSettings[i].GetAbsoluteRefractoryPeriod());
			builder.Append(" Rel" + adjustedNeuronSettings[i].GetRelativeRefractoryPeriod());
			file.WriteLine(builder.ToString());
		}
		file.Close();
	}

	public static void LoadGame() {
		LoadGame("Saves/save.txt");
	}

	public static void LoadBackup() {
		LoadGame("Saves/backup.txt");
	}

	public static void LoadGame(string file) {
		StreamReader reader = new StreamReader(file);
		while (!reader.EndOfStream) {
			string line = reader.ReadLine();

			//Check if neural connection, sensor connection, muscle connection, or neuron parameter
			string checkNeural = line.Substring(0, 5);
			string checkSensor = line.Substring(0, 6);
			string checkMuscle = line.Substring(0, 6);
			string checkParam = line.Substring(0, 6);

			if (checkNeural.Equals("Start")) {
				//Neural connection, move ahead
				ConnectionFile con;
				//Strip off the formatting to get to numbers
				line = line.Substring(9); //Start table number
				int startTable;
				if (!int.TryParse(line.Substring(0, 1), out startTable)) {
					Debug.LogError("Start Table format incorrect");
					continue;
				}
				line = line.Substring(6); //Start seat number
				int startSeat;
				if (!int.TryParse(line.Substring(0, 1), out startSeat)) {
					Debug.LogError("Start Seat format incorrect");
					continue;
				}
				line = line.Substring(10); //End table number
				int endTable;
				if (!int.TryParse(line.Substring(0, 1), out endTable)) {
					Debug.LogError("End Table format incorrect");
					continue;
				}
				line = line.Substring(6); //End seat number
				int endSeat;
				if (!int.TryParse(line.Substring(0, 1), out endSeat)) {
					Debug.LogError("End Seat format incorrect");
					continue;
				}
				line = line.Substring(6); //Strength
				float strength;
				if (!float.TryParse(line, out strength)) {
					Debug.LogError("Strength format incorrect");
					continue;
				}
				con = new ConnectionFile(startTable, endTable, startSeat, endSeat, strength);
				connectionsToLoad.Add(con);
			}
			else if (checkSensor.Equals("Sensor")) {
				//Sensor connection, move ahead
				SensorConnectionFile sen;
				//Strip off the formatting to get to numbers
				line = line.Substring(10); //Sensor table number
				int sensorTable;
				if (!int.TryParse(line.Substring(0, 1), out sensorTable)) {
					Debug.LogError("Sensor Table format incorrect");
					continue;
				}
				line = line.Substring(11); //Sensor location
				Sensor.SensorLocation location;
				switch (line.Substring(0, 1)) {
					case "P": {
							//Backward
							location = Sensor.SensorLocation.Backward;
							break;
						}
					case "A": {
							//Forward
							location = Sensor.SensorLocation.Forward;
							break;
						}
					default: {
							//Incorrect
							Debug.LogError("Sensor Location format incorrect");
							continue;
						}
				}
				line = line.Substring(10); //End table number
				int endTable;
				if (!int.TryParse(line.Substring(0, 1), out endTable)) {
					Debug.LogError("End Table format incorrect");
					continue;
				}
				line = line.Substring(6); //End seat number
				int endSeat;
				if (!int.TryParse(line.Substring(0, 1), out endSeat)) {
					Debug.LogError("End Seat format incorrect");
					continue;
				}
				sen = new SensorConnectionFile(sensorTable, location, endTable, endSeat);
				sensorConnectionsToLoad.Add(sen);
			}
			else if (checkMuscle.Equals("Muscle")) {
				//Muscle connection, move ahead
				MuscleConnectionFile mus;
				//Strip off the formatting to get to numbers
				line = line.Substring(10); //Muscle table number
				int muscleTable;
				if (!int.TryParse(line.Substring(0, 1), out muscleTable)) {
					Debug.LogError("Muscle Table format incorrect");
					continue;
				}
				line = line.Substring(7); //Muscle type
				MuscleType type;
				switch (line.Substring(0, 1)) {
					case "B": {
							//Backward
							type = MuscleType.Backward;
							break;
						}
					case "F": {
							//Forward
							type = MuscleType.Forward;
							break;
						}
					case "D": {
							//Stance
							type = MuscleType.Stance;
								break;
						}
					default: {
							//Incorrect
							Debug.LogError("Muscle Type format incorrect");
							continue;
						}
				}
				line = line.Substring(12); //Start table number
				int startTable;
				if (!int.TryParse(line.Substring(0, 1), out startTable)) {
					Debug.LogError("Start Table format incorrect");
					continue;
				}
				line = line.Substring(6); //Start seat number
				int startSeat;
				if (!int.TryParse(line.Substring(0, 1), out startSeat)) {
					Debug.LogError("Start Seat format incorrect");
					continue;
				}
				mus = new MuscleConnectionFile(muscleTable, type, startTable, startSeat);
				muscleConnectionsToLoad.Add(mus);
			}
			else if (checkParam.Equals("Neuron")) {
				//Neuron parameters, move ahead
				NeuronFile neu;
				//Strip off the formatting to get to numbers
				line = line.Substring(10); //Table number
				int table;
				if (!int.TryParse(line.Substring(0, 1), out table)) {
					Debug.LogError("Table format incorrect");
					continue;
				}
				line = line.Substring(6); //Seat number
				int seat;
				if (!int.TryParse(line.Substring(0, 1), out seat)) {
					Debug.LogError("Seat format incorrect");
					continue;
				}
				line = line.Substring(7); //Resting threshold
				float rest;
				string number = Regex.Match(line, @"[0-9\.]+").Value;
				if (!float.TryParse(number, out rest)) { 
					Debug.LogError("Resting Threshold format incorrect");
					continue;
				}
				line = line.Substring(4 + number.Length); //Recovery threshold
				float rec;
				number = Regex.Match(line, @"[0-9\.]+").Value;
				if (!float.TryParse(number, out rec)) {
					Debug.LogError("Recovery Threshold format incorrect");
					continue;
				}
				line = line.Substring(4 + number.Length); //Absolute refractory period
				float abs;
				number = Regex.Match(line, @"[0-9\.]+").Value;
				if (!float.TryParse(number, out abs)) {
					Debug.LogError("Absolute Refractory Period format incorrect");
					continue;
				}
				line = line.Substring(4 + number.Length); //Relative refractory period
				float rel;
				if (!float.TryParse(line, out rel)) {
					Debug.LogError("Relative Refractory Period format incorrect");
					continue;
				}
				neu = new NeuronFile(table, seat, rest, rec, abs, rel);
				neuronSettingsToLoad.Add(neu);
			}
			else if (line.Equals(string.Empty)) {
				//Empty file, don't bother reading
				return;
			}
			else {
				//Incorrect formatting, send error and stop
				Debug.LogError("Save file incorrect.  Please fix before loading.");
				return;
			}
		}
		reader.Close();
		Debug.Log("Game Loaded!");

		//Reset all connections.  Done once here so it isn't done several times in PlayerEntered.
		ResetConnections();
		spawnedConnections = new List<ConnectionFile>(); //Must be reset on load so ghost connections don't pop back up.
		spawnedSensorConnections = new List<SensorConnectionFile>();
		spawnedMuscleConnections = new List<MuscleConnectionFile>();

		//Spawn connections for players already in scene.
		foreach (NeuronDesignation nd in loadedPlayers) {
			if (nd.GetObject() != null) {
				PlayerEntered(nd.GetObject(), nd.GetTableNum(), nd.GetSeatNum(), true);
			}
		}
	}

	public static void PlayerEntered(GameObject obj, int table, int seat, bool preloaded) {
		//Save players so connections can load in later
		NeuronDesignation nu = new NeuronDesignation(obj, table, seat);
		//Ensure no duplicates
		if (!preloaded) {
			loadedPlayers.Add(nu);

			//Reset all connections
			RefreshConnections();

			//Respawn already-spawned connections.  //Only done on connect/reconnect, not load.
			for (int i = 0; i < spawnedConnections.Count; i++) {
				SpawnConnection(spawnedConnections[i]);
			}
			for (int i = 0; i < spawnedSensorConnections.Count; i++) {
				SpawnConnection(spawnedSensorConnections[i]);
			}
			for (int i = 0; i < spawnedMuscleConnections.Count; i++) {
				SpawnConnection(spawnedMuscleConnections[i]);
			}
		}
		else {
			//Load game is occurring.  Do NOT reset connections.  Has already been done in Load and should only be done once.
		}

		//Set parameters
		NeuronFile remove = null;
		foreach (NeuronFile file in neuronSettingsToLoad) {
			//Check table and seat
			if (file.GetTableNum().Equals(table) && file.GetSeatNum().Equals(seat)) {
				//Neuron found!  Set parameters
				obj.GetComponent<Controller>().SetNeuronParameters(obj, file.GetRestingThreshold(), file.GetRecoveryThreshold(), file.GetAbsoluteRefractoryPeriod(), file.GetRelativeRefractoryPeriod());
				remove = file;
			}
		}
		//Move completed file
		if (remove != null) {
			adjustedNeuronSettings.Add(remove);
		}
		else {
			//No file found, so make one
			Controller cont = obj.GetComponent<Controller>();
			NeuronFile newFile = new NeuronFile(table, seat, cont.GetThreshold(), cont.GetHighThreshold(), cont.GetAbsRefractoryPd(), cont.GetRelRefractoryPd());
			adjustedNeuronSettings.Add(newFile);
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

		//Check sensor connections for new player
		List<SensorConnectionFile> sensorsToRemove = new List<SensorConnectionFile>();
		foreach (SensorConnectionFile file in sensorConnectionsToLoad) {
			//Check table and seat vs. end
			if (file.isEndpoint(table, seat)) {
				//Connection found!  Instantiate
				SpawnConnection(file);
				sensorsToRemove.Add(file);
			}
			//Otherwise, ignore for now
		}
		//Delete instantiated connections
		foreach (SensorConnectionFile file in sensorsToRemove) {
			sensorConnectionsToLoad.Remove(file);
		}

		//Check muscle connections for new player
		List<MuscleConnectionFile> musclesToRemove = new List<MuscleConnectionFile>();
		foreach (MuscleConnectionFile file in muscleConnectionsToLoad) {
			//Check table and seat vs. start
			if (file.isStartpoint(table, seat)) {
				//Connection found!  Instantiate
				SpawnConnection(file);
				musclesToRemove.Add(file);
			}
			//Otherwise, ignore for now
		}
		//Delete instantiated connections
		foreach (MuscleConnectionFile file in musclesToRemove) {
			muscleConnectionsToLoad.Remove(file);
		}
	}

	public static void ConnectionMade(Connection con) {
		ConnectionFile file;
		Controller start = con.GetStart().GetComponent<Controller>();
		Controller end = con.GetEnd().GetComponent<Controller>();

		file = new ConnectionFile(start.GetTableNum(), end.GetTableNum(), start.GetSeatNum(), end.GetSeatNum(), con.connectionStrength);
		connectionsToReset.Add(con.gameObject);
		if (!spawnedConnections.Contains(file)) {
			spawnedConnections.Add(file);
		}
	}

	public static void SensorConnectionMade(SensorConnection sen) {
		SensorConnectionFile file;
		SensorConnector start = sen.GetStart().GetComponent<SensorConnector>();
		Controller end = sen.GetEnd().GetComponent<Controller>();

		file = new SensorConnectionFile(start.table, start.sensor.location, end.GetTableNum(), end.GetSeatNum());
		sensorConnectionsToReset.Add(sen.gameObject);
		if (!spawnedSensorConnections.Contains(file)) {
			spawnedSensorConnections.Add(file);
		}
	}

	public static void MuscleConnectionMade(MuscleConnection mus) {
		MuscleConnectionFile file;
		Controller start = mus.GetStart().GetComponent<Controller>();
		Muscle end = mus.GetEnd().GetComponent<Muscle>();

		MuscleType type;
		if(end is SwingMuscle) {
			//What type?
			SwingMuscle swing = end as SwingMuscle;
			switch (swing.moveDirection) {
				case SwingMuscle.SwingFunction.Backward: {
						type = MuscleType.Backward;
						break;
					}
				case SwingMuscle.SwingFunction.Forward: {
						type = MuscleType.Forward;
						break;
					}
				default: {
						//So we have something
						type = MuscleType.Forward;
						break;
					}
			}
		}
		else {
			//Stance muscle
			type = MuscleType.Stance;
		}
		file = new MuscleConnectionFile(end.table, type, start.GetTableNum(), start.GetSeatNum());
		muscleConnectionsToReset.Add(mus.gameObject);
		if (!spawnedMuscleConnections.Contains(file)) {
			spawnedMuscleConnections.Add(file);
		}
	}

	public static void ConnectionRemoved(Connection con) {
		ConnectionFile file;
		Controller start = con.GetStart().GetComponent<Controller>();
		Controller end = con.GetEnd().GetComponent<Controller>();

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

	public static void SensorConnectionRemoved(SensorConnection sen) {
		SensorConnectionFile file;
		SensorConnector start = sen.GetStart().GetComponent<SensorConnector>();
		Controller end = sen.GetEnd().GetComponent<Controller>();

		file = new SensorConnectionFile(start.table, start.sensor.location, end.GetTableNum(), end.GetSeatNum());

		int endpoint = sensorConnectionsToReset.Count;  //Avoid changing list while iterating.
		for(int i=endpoint - 1; i>=0; i--) {
			if (sensorConnectionsToReset[i].Equals(sen.gameObject)) {
				sensorConnectionsToReset.Remove(sen.gameObject);
			}
		}

		endpoint = spawnedSensorConnections.Count;
		for(int i=endpoint - 1; i>=0; i--) {
			if (spawnedSensorConnections[i].Equals(file)) {
				spawnedSensorConnections.Remove(file);
			}
		}
	}

	public static void MuscleConnectionRemoved(MuscleConnection mus) {
		MuscleConnectionFile file;
		Controller start = mus.GetStart().GetComponent<Controller>();
		Muscle end = mus.GetEnd().GetComponent<Muscle>();

		MuscleType type;
		if (end is SwingMuscle) {
			//What type?
			SwingMuscle swing = end as SwingMuscle;
			switch (swing.moveDirection) {
				case SwingMuscle.SwingFunction.Backward: {
						type = MuscleType.Backward;
						break;
					}
				case SwingMuscle.SwingFunction.Forward: {
						type = MuscleType.Forward;
						break;
					}
				default: {
						//So we have something
						type = MuscleType.Forward;
						break;
					}
			}
		}
		else {
			//Stance muscle
			type = MuscleType.Stance;
		}

		file = new MuscleConnectionFile(end.table, type, start.GetTableNum(), start.GetSeatNum());

		int endpoint = muscleConnectionsToReset.Count;  //Avoid changing list while iterating.
		for (int i = endpoint - 1; i >= 0; i--) {
			if (muscleConnectionsToReset[i].Equals(mus.gameObject)) {
				muscleConnectionsToReset.Remove(mus.gameObject);
			}
		}

		endpoint = spawnedMuscleConnections.Count;
		for (int i = endpoint - 1; i >= 0; i--) {
			if (spawnedMuscleConnections[i].Equals(file)) {
				spawnedMuscleConnections.Remove(file);
			}
		}
	}

	//Resets connections after a player logs in to ensure all connections exist properly
	public static void RefreshConnections() {
		int endpoint = connectionsToReset.Count; //Must use outside endpoint to prevent editing list while iterating.
		for (int i = endpoint - 1; i >= 0; i--) { //Go in reverse so indices don't shift awkwardly
			if (connectionsToReset[i] != null) { //If it isn't gone already, destroy it.
				connectionsToReset[i].GetComponent<Connection>().Destroy(true);
			}
		}

		endpoint = sensorConnectionsToReset.Count; //Must use outside endpoint to prevent editing list while iterating.
		for (int i = endpoint - 1; i >= 0; i--) { //Go in reverse so indices don't shift awkwardly
			if (sensorConnectionsToReset[i] != null) { //If it isn't gone already, destroy it.
				sensorConnectionsToReset[i].GetComponent<SensorConnection>().Destroy(true);
			}
		}

		endpoint = muscleConnectionsToReset.Count; //Must use outside endpoint to prevent editing list while iterating.
		for (int i = endpoint - 1; i >= 0; i--) { //Go in reverse so indices don't shift awkwardly
			if (muscleConnectionsToReset[i] != null) { //If it isn't gone already, destroy it.
				muscleConnectionsToReset[i].GetComponent<MuscleConnection>().Destroy(true);
			}
		}

		/*connectionsToReset = new List<GameObject>();
		sensorConnectionsToReset = new List<GameObject>();
		muscleConnectionsToReset = new List<GameObject>();*/
	}

	//Resets all connections by deleting them
	public static void ResetConnections() {
		GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
		ConnectionManager manager = player.GetComponent<ConnectionManager>();
		int endpoint = connectionsToReset.Count; //Must use outside endpoint to prevent editing list while iterating.
		for (int i = endpoint - 1; i >= 0; i--) { //Go in reverse so indices don't shift awkwardly
			if (connectionsToReset[i] != null) { //If it isn't gone already, destroy it.
				manager.DeleteConnection(connectionsToReset[i]);
			}
		}

		endpoint = sensorConnectionsToReset.Count; //Must use outside endpoint to prevent editing list while iterating.
		for (int i = endpoint - 1; i >= 0; i--) { //Go in reverse so indices don't shift awkwardly
			if (sensorConnectionsToReset[i] != null) { //If it isn't gone already, destroy it.
				manager.DeleteSensorConnection(sensorConnectionsToReset[i]);
			}
		}

		endpoint = muscleConnectionsToReset.Count; //Must use outside endpoint to prevent editing list while iterating.
		for (int i = endpoint - 1; i >= 0; i--) { //Go in reverse so indices don't shift awkwardly
			if (muscleConnectionsToReset[i] != null) { //If it isn't gone already, destroy it.
				manager.DeleteMuscleConnection(muscleConnectionsToReset[i]);
			}
		}

		connectionsToReset = new List<GameObject>();
		sensorConnectionsToReset = new List<GameObject>();
		muscleConnectionsToReset = new List<GameObject>();
	}

	//Updates file for given neuron
	public static void NeuronParametersChanged(Controller neuron) {
		NeuronFile toReplace = null;
		NeuronFile toAdd = null;
		foreach (NeuronFile file in adjustedNeuronSettings) {
			if(file.GetTableNum().Equals(neuron.GetTableNum()) && file.GetSeatNum().Equals(neuron.GetSeatNum())){
				//Neuron found!  Update file
				NeuronFile newFile = new NeuronFile(neuron.GetTableNum(), neuron.GetSeatNum(), neuron.GetThreshold(), neuron.GetHighThreshold(), neuron.GetAbsRefractoryPd(), neuron.GetRelRefractoryPd());
				toAdd = newFile;
				toReplace = file;
			}
		}
		if (toReplace != null) {
			adjustedNeuronSettings.Remove(toReplace);
		}
		if (toAdd != null) {
			adjustedNeuronSettings.Add(toAdd);
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

	//Helper method to spawn sensor connections based on given connection file
	private static void SpawnConnection(SensorConnectionFile file) {
		//Find start and end gameObjects
		GameObject start = null, end = null;
		foreach (NeuronDesignation nd in loadedPlayers) {
			//Find end
			if (nd.isGivenNeuron(file.GetEnd()[0], file.GetEnd()[1])) {
				end = nd.GetObject();
			}
		}
		foreach (SensorConnector sc in sensors) {
			//Find start
			if (sc.table.Equals(file.GetStart().table) && sc.sensor.location.Equals(file.GetStart().location)) {
				start = sc.gameObject;
			}
		}
		if (start && end) {
			GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
			player.GetComponent<ConnectionManager>().SensorConnection(end, start);
		}
	}

	//Helper method to spawn muscle connections based on given connection file
	private static void SpawnConnection(MuscleConnectionFile file) {
		//Find start and end gameObjects
		GameObject start = null, end = null;
		foreach (NeuronDesignation nd in loadedPlayers) {
			//Find start
			if (nd.isGivenNeuron(file.GetStart()[0], file.GetStart()[1])) {
				start = nd.GetObject();
			}
		}
		foreach (Muscle m in muscles) {
			//Find end
			MuscleType type;
			if (m is SwingMuscle) {
				//What type?
				SwingMuscle swing = m as SwingMuscle;
				switch (swing.moveDirection) {
					case SwingMuscle.SwingFunction.Backward: {
							type = MuscleType.Backward;
							break;
						}
					case SwingMuscle.SwingFunction.Forward: {
							type = MuscleType.Forward;
							break;
						}
					default: {
							//So we have something
							type = MuscleType.Forward;
							break;
						}
				}
			}
			else {
				//Stance muscle
				type = MuscleType.Stance;
			}

			if (m.table.Equals(file.GetEnd().table) && type.Equals(file.GetEnd().type)) {
				end = m.gameObject;
			}
		}
		if (start && end) {
			GameObject player = NetworkManager.singleton.client.connection.playerControllers[0].gameObject;
			player.GetComponent<ConnectionManager>().MuscleConnection(start, end);
		}
	}
}
