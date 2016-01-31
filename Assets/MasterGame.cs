using UnityEngine;
using System.Collections;

public enum gameStage {Intro,Tutorial,Baby,HighSchool,Boss,GameOver};
public class MasterGame : MonoBehaviour
{

	// Singleton behavior
	public static MasterGame instance {
		get {
			if (applicationIsQuitting) {
				Debug.LogWarning ("[Singleton] Instance MasterGame '" +
				"' already destroyed on application quit." +
				" Won't create again - returning null.");
				return null;
			}
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
	bool autoCenterComplete = false;

	[Header ("Screen Constants")]
	public float centerX = 0;
	public float centerY = 0;


	void Start ()
	{
		player = Object.FindObjectOfType<PlayerControl> ();
		opponent = Object.FindObjectOfType<OpponentControl> ();

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
	}

	void OnSuccess() {

	}

	void OnFailure() {
		gameOver = true;

		// Show the failure screen
	}

	//functions here, if needed
	public void HandFailure(){
		currentHandStage = handStage.Failure;
	}

	public void OnDestroy ()
	{
		applicationIsQuitting = true;
	}
}
