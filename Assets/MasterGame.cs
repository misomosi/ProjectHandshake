using UnityEngine;
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
	public SuccessPanel successPanel;
	public FailurePanel failurePanel;
	public handStage currentHandStage {
		get{ return _currentHandStage; }
		set {
			if (_currentHandStage == value) {
				return;
			}
			switch (_currentHandStage) {
			case handStage.Joust:
				ExitJoust ();
				break;
			case handStage.Shake:
				ExitShake ();
				break;
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

	void Awake() {
		// Find the objects in the scene
		player = Object.FindObjectOfType<PlayerControl> ();
		opponent = Object.FindObjectOfType<OpponentControl> ();
		shakezone = Object.FindObjectOfType<ShakeZone> ();
		successPanel = Object.FindObjectOfType<SuccessPanel> ();
		failurePanel = Object.FindObjectOfType<FailurePanel> ();

		// Determine which level this is
		
	}

	void Start ()
	{
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
	}

	void OnSuccess() {
		Debug.Log ("Success!");
		successPanel.gameObject.SetActive (true);
	}

	void OnFailure() {
		gameOver = true;
		Debug.Log ("Failure!");
		successPanel.gameObject.SetActive (true);
		// Show the failure screen
	}

	void ExitJoust () {

	}

	void ExitShake () {
		shakezone.gameObject.SetActive (false);
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
