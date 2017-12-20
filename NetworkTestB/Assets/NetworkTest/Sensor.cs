using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour {

	public enum SensorLocation {
		Forward, Backward
	}

	public List<Controller> output;

	public Collider toBeSensed;

	public InsectLeg leg;
	public SensorLocation location;

	public void ConnectToNeuron(Controller neuron) {
		output.Add(neuron);
	}

	public void DisconnectFromNeuron(Controller neuron) {
		output.Remove(neuron);
	}

	private void OnTriggerStay(Collider other) {
		if (other.Equals(toBeSensed)) {
			foreach (Controller neuron in output) {
				neuron.currentInputs.Add(new Controller.NeuralInput(1000, Time.time)); //Large strength to always fire connected neuron.  See about streamlining/ask Dr. Chiel if this is intended behaviour
			}
			//Prevent extreme movement
			if (location == SensorLocation.Forward) {
				//Prevent forward movement
				leg.canRotateForward = false;
			}
			else if (location == SensorLocation.Backward) {
				//Prevent backward movement
				leg.canRotateBackward = false;
			}
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.Equals(toBeSensed)) {
			//Upon exiting sensor area, free up movement
			if (location == SensorLocation.Forward) {
				leg.canRotateForward = true;
			}
			else if (location == SensorLocation.Backward) {
				leg.canRotateBackward = true;
			}
		}
	}
}
