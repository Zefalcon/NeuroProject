using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muscle : MonoBehaviour {

	public InsectLeg leg; //Each muscle is attached to a leg.

	public List<Input> currentInputs;

	private bool isActivated = false;

	public int table;

	//Muscle operates similarly to neurons receiving input, but will always activate regardless of strength and has no hard or soft cooldown.
	public class Input {
		float timeReceived;
		float waitTime = 1f;
		public Input(float timeReceived) {
			this.timeReceived = timeReceived;
		}
		public bool IsCurrent(float currentTime) {
			return (currentTime - timeReceived) < waitTime;
		}
	}

	// Use this for initialization
	protected virtual void Start () {
		currentInputs = new List<Input>();
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		//Check inputs for activation
		bool activate = false;
		List<Input> toRemove = new List<Input>();
		if (currentInputs != null) {
			foreach (Input input in currentInputs) {
				if (input.IsCurrent(Time.time)) {
					activate = true;
				}
				else {
					//Remove old inputs
					toRemove.Add(input);
				}
			}
		}

		for(int i=0; i<toRemove.Count; i++) {
			currentInputs.Remove(toRemove[i]);
		}

		//If not already activated, with input, activate.
		if (activate && !isActivated) {
			isActivated = true;
			ActivateMuscle();
		}
		//If already activated, with no input, deactivate.
		else if (!activate && isActivated) {
			isActivated = false;
			DeactivateMuscle();
		}
		//Otherwise, muscle is already in the correct  state.
		else {
			
		}
	}

	//Specified for the two types of muscle.
	public virtual void ActivateMuscle() {
		//Activates the function of the muscle, whatever it is.
	}

	//Specified for the two types of muscle.
	public virtual void DeactivateMuscle() {
		//Deactivates the muscle in whatever way it should be.
	}
}
