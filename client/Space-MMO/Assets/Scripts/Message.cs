using UnityEngine;
using System.Collections;

public enum SOCKET_TAG : uint {
	NOP = 0,
	SET_ID = 1,
};

public class Message {

	public int len;
	public SOCKET_TAG tag;
	public string data;

}
