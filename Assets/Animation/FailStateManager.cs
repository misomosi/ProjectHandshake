﻿using UnityEngine;
using System.Collections;

public class FailStateManager : MonoBehaviour {

	//public MasterGame handStage;

	Animator anim;
	public bool buttonPlayAgain = false;

	void Awake(){
		anim = GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () {
		//gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (MasterGame.instance.currentHandStage == MasterGame.handStage.Failure){
			anim.SetTrigger("FailScreen");
		} else if(MasterGame.instance.currentHandStage == MasterGame.handStage.Success){
			anim.SetTrigger("SuccessScreen");
		}
		//if (buttonPlayAgain == true){
		//	Application.LoadLevel("ProjHandshakeTest0.03");
		//}
	}
}
