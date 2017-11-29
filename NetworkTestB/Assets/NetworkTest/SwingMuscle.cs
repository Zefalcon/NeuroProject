using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMuscle : Muscle {

	public SwingFunction moveDirection;

	public enum SwingFunction {
		Forward, Backward
	}

	// Use this for initialization
	protected override void Start () {
		base.Start();
	}

	// Update is called once per frame
	protected override void Update() {
		base.Update();
	}

	public override void ActivateMuscle() {
		base.ActivateMuscle();
		//Swing muscles move the leg back and forth.
		switch (moveDirection) {
			case (SwingFunction.Forward): {
					leg.RotateForward();
					break;
				}
			case (SwingFunction.Backward): {
					leg.RotateBackward();
					break;
				}
		}
	}

	public override void DeactivateMuscle() {
		base.DeactivateMuscle();
		//Halt muscle.
		switch (moveDirection) {
			case SwingFunction.Forward: {
					leg.Halt(true);
					break;
				}
			case SwingFunction.Backward: {
					leg.Halt(false);
					break;
				}
		}
	}
}
