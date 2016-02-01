using UnityEngine;
using System.Collections;

public class FailurePanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnRestartClick() {
		if(MasterGame.instance.currentHandStage == MasterGame.handStage.Failure)
			MasterGame.instance.Restart ();
	}
}
