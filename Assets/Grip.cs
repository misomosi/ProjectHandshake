using UnityEngine;
using System.Collections;

public class Grip : MonoBehaviour {

	//Variables for the grip activity during handStage2
	[Header("Grip")]
	public float gripStrength;
	public float gripMash = 12;
	public float gripTargetMin;
	public float gripTargetMax;
	public float gripAbsoluteMin = 0;
	public float gripAbsoluteMax;
	public bool gripLost = false;
	int handStage;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		handStage = (int)GameObject.Find("GameStateControl").GetComponent<MasterGame>().currentHandStage;

		if(handStage == 2){
			if (Input.GetKeyDown("space") == true){
				gripStrength += gripMash;
			}
			//if (||)
		}
	}
}
