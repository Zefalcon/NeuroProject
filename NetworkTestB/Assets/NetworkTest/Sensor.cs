using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour {

	public List<Controller> output;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ConnectToNeuron(Controller neuron) {
		output.Add(neuron);
	}

	private void OnTriggerEnter(Collider other) {
			foreach (Controller neuron in output) {
				neuron.currentInputs.Add(new Controller.NeuralInput(1000, Time.time)); //Large strength to always fire connected neuron.  See about streamlining/ask Dr. Chiel if this is intended behaviour
			}
	}
}
