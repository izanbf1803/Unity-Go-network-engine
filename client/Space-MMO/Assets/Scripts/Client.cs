using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;

public class Client {

	const string CONN_HOST = "localhost";
	const int CONN_PORT = 3333;

	private TcpClient conn;
	private NetworkStream stream;
	private ASCIIEncoding ascii;

	public Client() {
		ascii = new ASCIIEncoding();
		conn = new TcpClient();

		conn.Connect(CONN_HOST, CONN_PORT);

		stream = conn.GetStream();
	}

	public void Send(SOCKET_TAG tag, string msg) {
		byte[] data = ascii.GetBytes(msg);
		int len = data.Length;

		byte[] packet = new byte[sizeof(int) * 2 + len];

		packet[0] = (byte)(len);
		packet[1] = (byte)(len >> 8);
		packet[2] = (byte)(len >> 16);
		packet[3] = (byte)(len >> 24);

		packet[4] = (byte)((int)tag);
		packet[5] = (byte)((int)tag >> 8);
		packet[6] = (byte)((int)tag >> 16);
		packet[7] = (byte)((int)tag >> 24);

		System.Buffer.BlockCopy(data, 0, packet, sizeof(int)*2, len);

		stream.Write(packet, 0, packet.Length);
	}

	public Message Recv() {
		int len;
		int tag;
		byte[] data;
		byte[] msgLen = new byte[sizeof(int)];
		byte[] msgTag = new byte[sizeof(int)];
		Message msg = new Message();

		stream.Read(msgLen, 0, sizeof(int));
		stream.Read(msgTag, 0, sizeof(int));

		len = BitConverter.ToInt32(msgLen, 0);
		tag = BitConverter.ToInt32(msgTag, 0);

		data = new byte[len];

		stream.Read(data, 0, len);

		msg.data = Decode(data);
		msg.len = len;
		msg.tag = (SOCKET_TAG)tag;

		return msg;
    }

	public string Decode(byte[] data) {
		return ascii.GetString(data, 0, data.Length);
    }

}
