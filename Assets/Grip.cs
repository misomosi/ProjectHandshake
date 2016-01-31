using UnityEngine;
using System.Collections;

public class Grip : MonoBehaviour {

	//Variables for the grip activity during handStage2
	[Header("Grip")]
	public float gripMash = 100;
	public float gripTargetMin;
	public float gripTargetMax;
	public float gripStrength;
	public float gripAbsoluteMin = 0;
	public float gripAbsoluteMax = 1000;
	public float timeToFail = 10;
	public float gripDecay = 20;
	public bool gripWrong = false;
	public bool gripLost = false;
	int handStage;

	float startTime;
	float timePass;


	// Use this for initialization
	void Start () {
		gripStrength = Average(gripTargetMin,gripTargetMax);
		if (GameObject.Find("GameStateControl").GetComponent<MasterGame>().currentGameStage == gameStage.Baby){
			timeToFail = 4;
		} else if(GameObject.Find("GameStateControl").GetComponent<MasterGame>().currentGameStage == gameStage.HighSchool){
			timeToFail = 3;
		} else if (GameObject.Find("GameStateControl").GetComponent<MasterGame>().currentGameStage == gameStage.Boss){
			timeToFail = 2;
		}
		//uncomment following 2 lines for testing and range
		//gripTargetMax = value;
		//gripTargetMin = value;
	}
	
	// Update is called once per frame
	void Update () {

		handStage = (int)GameObject.Find("GameStateControl").GetComponent<MasterGame>().currentHandStage;

		//only active if shake stage and player did not lose the stage
		if(handStage == 2 && gripLost == false){
			//adds a value to strength when user presses the "f" key
			if (Input.GetKeyDown("f") == true){
				gripStrength += gripMash;
			}
			//adds a floor and ceiling to grip
			if (gripStrength < gripAbsoluteMin){
				gripStrength = gripAbsoluteMin;
			}else if (gripStrength > gripAbsoluteMax){
				gripStrength = gripAbsoluteMax;
			}

			//occurs when out of bounds
			if (gripStrength > gripTargetMax || gripStrength < gripTargetMin){
				//occurs only on the frame after grip is out of range
				if (gripWrong == false){
					gripWrong = true;
					startTime = Time.time;
				}

				timePass = Time.time - startTime;
				//fails the stage if out of range for timeToFail seconds
				if (timePass >= timeToFail){
					gripLost = true;
				}

			//resets time and prepares for the next out-of bounds phase
			}else if(gripStrength < gripTargetMax && gripStrength > gripTargetMin){
					timePass = 0;
					gripWrong = false;
			}

			gripStrength -= gripDecay;

		//sends failure signal if stage was failed
		}else if (gripLost == true){
			GameObject.Find("GameStateControl").GetComponent<MasterGame>().HandFailure();

		} 
	}

	float Average(float x,float y){
		float avg = (x+y)/2;
		return avg;
	}

}
