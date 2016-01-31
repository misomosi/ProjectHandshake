using UnityEngine;
using System.Collections;

public class SimpleShowcase : MonoBehaviour {
    [SerializeField]
    private GameObject target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.RotateAround(target.transform.position, Vector3.up, 20 * Time.deltaTime);
	
	}
}
