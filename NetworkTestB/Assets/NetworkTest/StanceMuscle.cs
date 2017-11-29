using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StanceMuscle : Muscle {

	// Use this for initialization
	protected override void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
	}

	public override void ActivateMuscle() {
		base.ActivateMuscle();
		leg.SetFootDown();
	}

	public override void DeactivateMuscle() {
		base.DeactivateMuscle();
		leg.SetFootUp();
	}
}
