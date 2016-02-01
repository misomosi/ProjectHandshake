using UnityEngine;
using System.Collections;

public class SuccessPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnRestartClick() {

	}

	public void OnNextClick() {
		Debug.Log ("Next click");
		MasterGame.instance.LoadNextScene ();
	}
}
