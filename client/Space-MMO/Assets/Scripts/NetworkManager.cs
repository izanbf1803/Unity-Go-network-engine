using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	
	public Client client;

	// Use this for initialization
	void Start () {
		System.Diagnostics.Debug.WriteLine("test");
		client = new Client();

		lock (client.sendLock)
			client.Send(SOCKET_TAG.SET_ID, "Hello world!");

		print(client.queue.Count);
    }
	
	// Update is called once per frame
	void Update () {
		ClientUpdate();

		if (Input.GetKeyDown(KeyCode.A)) {
			client.Send(SOCKET_TAG.SEND_KEY, "A");
		}
	}

	void ClientUpdate() {
		while (client.queue.Count > 0) {
			Message msg;
			lock (client.queueLock)
				msg = client.queue.Dequeue();

			switch (msg.Tag) {
				case SOCKET_TAG.SET_ID:
					client.ID = int.Parse(msg.Data);
					break;
			}
		}
	}
}
