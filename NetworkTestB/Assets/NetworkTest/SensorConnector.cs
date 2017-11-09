﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorConnector : MonoBehaviour {

	public Sensor sensor;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void AttachToSensor(Controller neuron) {
		//Attaches given neuron to sensor contained within
		sensor.ConnectToNeuron(neuron);
	}
}
