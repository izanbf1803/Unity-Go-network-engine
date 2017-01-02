using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	Client client;

	// Use this for initialization
	void Start () {
		StartCoroutine("connection_test");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerable connection_test() {
		client = new Client();

		client.Send(SOCKET_TAG.SET_ID, "Hello world!");

		Message msg = client.Recv();

		print(msg.len);
		print(msg.data);
		print(msg.tag);

		while (true) {
			client.Send(SOCKET_TAG.SET_ID, "Added to queue!");
			yield return new WaitForSeconds(1);
		}
	}
}
