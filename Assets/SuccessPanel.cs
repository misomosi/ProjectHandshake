using UnityEngine;
using System.Collections;

public class SuccessPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnRestartClick() {

	}

	public void OnNextClick() {
		MasterGame.instance.LoadNextScene ();
	}
}
