using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour {

	AudioSource joustAudio;
	AudioSource shakeAudio;

	// Use this for initialization
	void Start () {
		joustAudio = GameObject.Find ("JoustPhase").GetComponent<AudioSource> ();
		shakeAudio = GameObject.Find ("ShakePhase").GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		switch (MasterGame.instance.currentHandStage) {
		case MasterGame.handStage.Joust:
			joustAudio.volume = 1.0f;
			shakeAudio.volume = 0.0f;
			break;
		case MasterGame.handStage.Shake:
			joustAudio.volume = 0.0f;
			shakeAudio.volume = 1.0f;
			break;
		}


	}
}
