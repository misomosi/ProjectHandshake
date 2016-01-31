using UnityEngine;
using System.Collections;

public class MasterGame : MonoBehaviour {

	// Use this for initialization


	//main game variables used to switch between different levels and control game progression.

	public enum gameStage {Intro,Tutorial,Baby,HighSchool,Boss,GameOver};
	public enum handStage {ReadyGo,Joust,AutoCenter,Shake,Success,Failure};
	[Header("MainGame")]
	public bool gameOver = false;
	public gameStage currentGameStage;
	public handStage currentHandStage;


	//[Header("Grip")]
	bool gripLost;

	[Header("PlayerHand")]
	bool isGrab;
	float handShakeX;
	public float handAutoDragSpeed;
	public PlayerControl player;
	bool autoCenterComplete = false;

	[Header("Screen Constants")]
	public float centerX = 0;
	public float centerY = 0;


	void Start () {
		player = Object.FindObjectOfType<PlayerControl>();
	}
	
	// Update is called once per frame
	void Update () {
		if(gameOver==false){

			//tests for currentGameStage

				isGrab = player.isGrab;
				if (isGrab == true){
					currentHandStage = handStage.AutoCenter;
					player.SendMessage("autoCenter");
				}

				/*
				if (autocenter is complete){
					currentHandStage = handStage.Shake;
				}
				*/



		}
	}

	//functions here, if needed

}
