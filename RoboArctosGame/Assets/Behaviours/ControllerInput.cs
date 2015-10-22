using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class ControllerInput : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		print("startup");
		var client = new UdpClient();
		IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000); // endpoint where server is listening
		client.Connect(ep);
		
		client.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);
		temp = 0;
	}
	
	float temp;
	// Update is called once per frame
	void Update () {
        float left = Input.GetAxis("TriggerLeft");
        float right = Input.GetAxis("TriggerRight");

        float speed = 10 * left - 10 * right;
		if(temp != speed) {
			temp = speed;
			print("SpeedLeft: " + speed);
			GameObject.Find("Cube").transform.Translate(0, 0, speed);
		}
	}
}
