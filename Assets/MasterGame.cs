﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public enum gameStage {Intro,Tutorial,Baby,HighSchool,Boss,GameOver};
public class MasterGame : MonoBehaviour
{

	// Singleton behavior
	public static MasterGame instance {
		get {
			/*
			if (applicationIsQuitting) {
				Debug.LogWarning ("[Singleton] Instance MasterGame '" +
				"' already destroyed on application quit." +
				" Won't create again - returning null.");
				return null;
			}
			*/
			if (_instance == null) {
				_instance = FindObjectOfType<MasterGame> ();
			}
			return _instance;
		}
	}

	private static MasterGame _instance;
	private static bool applicationIsQuitting = false;

	// Use this for initialization


	//main game variables used to switch between different levels and control game progression.


	public enum handStage {ReadyGo,Joust,AutoCenter,Shake,Success,Failure};

	[Header ("MainGame")]
	public bool gameOver = false;
	public gameStage currentGameStage;
	public handStage currentHandStage {
		get{ return _currentHandStage; }
		set {
			if (value == _currentHandStage)
				return;

			if (_currentHandStage == handStage.Joust) {
				OnExitJoust ();
			} else if (_currentHandStage == handStage.Shake) {
				OnExitShake ();
			}

			_currentHandStage = value;
			switch (_currentHandStage) {
			case handStage.ReadyGo:
				OnReadyGo ();
				break;
			case handStage.Joust:
				OnJoust ();
				break;
			case handStage.AutoCenter:
				OnAutoCenter ();
				break;
			case handStage.Shake:
				OnShake ();
				break;
			case handStage.Success:
				OnSuccess ();
				break;
			case handStage.Failure:
				OnFailure ();
				break;
			}
		}
	}
	public handStage _currentHandStage;

	public SuccessPanel successPanel;
	public FailurePanel failurePanel;


	//[Header("Grip")]
	bool gripLost;

	[Header ("PlayerHand")]
	bool isGrab;
	float handShakeX;
	public float AutoCenterTime = 2.0f;
	public PlayerControl player;
	public OpponentControl opponent;
	public ShakeZone shakezone;
	bool autoCenterComplete = false;

	[Header ("Screen Constants")]
	public float centerX = 0;
	public float centerY = 0;

	private AudioSource winSound;
	private AudioSource loseSound;
	private bool hasPlayedOnce = false;

	void Awake() {
		// Find the objects in the scene
		player = Object.FindObjectOfType<PlayerControl> ();
		opponent = Object.FindObjectOfType<OpponentControl> ();
		shakezone = Object.FindObjectOfType<ShakeZone> ();
		winSound = GameObject.Find ("WinEffect").GetComponent<AudioSource>();
		loseSound = GameObject.Find ("LoseEffect").GetComponent<AudioSource>();
		successPanel = FindObjectOfType<SuccessPanel> ();

		// Determine which level this is
		
	}

	void Start ()
	{
		GameObject.Find("Canvas").SetActive(true);
		switch (_currentHandStage) {
		case handStage.ReadyGo:
			OnReadyGo ();
			break;
		case handStage.Joust:
			OnJoust ();
			break;
		case handStage.AutoCenter:
			OnAutoCenter ();
			break;
		case handStage.Shake:
			OnShake ();
			break;
		case handStage.Success:
			OnSuccess ();
			break;
		case handStage.Failure:
			OnFailure ();
			break;
		}
	}

	// These get called whenever we change to the new corresponding state
	void OnReadyGo() {

	}

	void OnJoust() {
		opponent.StopAllCoroutines ();
		opponent.SendMessage ("Joust");
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	void OnAutoCenter() {
		player.StopAllCoroutines ();
		opponent.StopAllCoroutines ();

		player.SendMessage ("AutoCenter");
		Destroy (opponent.gameObject);
	}

	void OnShake() {
		Debug.Log ("Shake has begun!");
		shakezone.gameObject.SetActive (true);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void OnSuccess() {
		if (!hasPlayedOnce) {
			winSound.Play ();
			hasPlayedOnce = true;
		}
		successPanel.gameObject.SetActive (true);
	}

	void OnExitJoust() {

	}

	void OnExitShake() {
		shakezone.gameObject.SetActive (false);
	}

	void OnFailure() {
		gameOver = true;
		if (!hasPlayedOnce) {
			loseSound.Play ();
			hasPlayedOnce = true;
		}
		// Show the failure screen
		failurePanel.gameObject.SetActive(true);
	}

	//functions here, if needed
	public void HandFailure(){
		currentHandStage = handStage.Failure;
	}

	public void OnDestroy ()
	{
		applicationIsQuitting = true;
	}

	public void Update () {
		if (Input.GetKeyDown ("r")) {
			Restart ();
		}
		if (Input.GetButtonDown ("Cancel")) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public void Restart () {
		Scene currentScene = SceneManager.GetActiveScene ();
		_instance = null;
		SceneManager.LoadScene (currentScene.buildIndex);
	}

	public void LoadNextScene () {
		Scene currentScene = SceneManager.GetActiveScene ();
		_instance = null;
		SceneManager.LoadScene (currentScene.buildIndex + 1);
	}
}
