using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorConnector : MonoBehaviour {

	public Sensor sensor;

	public int table;

	public void AttachToSensor(Controller neuron) {
		//Attaches given neuron to sensor contained within
		sensor.ConnectToNeuron(neuron);
	}

	public void DetachFromSensor(Controller neuron) {
		sensor.DisconnectFromNeuron(neuron);
	}
}
