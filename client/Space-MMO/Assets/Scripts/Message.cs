using UnityEngine;
using System.Collections;

public enum SOCKET_TAG : uint {
	NOP = 0,
	SET_ID = 1,
	SEND_KEY = 2,
};

public class Message {

	public int Len;
	public SOCKET_TAG Tag;
	public string Data;

}
