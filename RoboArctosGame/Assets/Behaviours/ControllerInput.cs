using UnityEngine;
using System.Collections;

public class ControllerInput : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float left = Input.GetAxis("TriggerLeft");
        float right = Input.GetAxis("TriggerRight");

        float speed = 10 * left - 10 * right;
        print("SpeedLeft: " + speed);
        GameObject.Find("Cube").transform.Translate(0, 0, speed);
	}
}
