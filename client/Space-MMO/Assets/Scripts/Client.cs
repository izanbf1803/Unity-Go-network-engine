using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;

public class Client {

	const string CONN_HOST = "127.0.0.1";
	const int CONN_PORT = 3333;

	private Socket conn;
	private ASCIIEncoding ascii;

	public System.Collections.Generic.Queue<Message> queue;
	public object sendLock = new object();
	public object queueLock = new object();
	public int ID;


	public Client() {
		queue = new System.Collections.Generic.Queue<Message>();
		ascii = new ASCIIEncoding();
		conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		IPAddress ipAdress = IPAddress.Parse(CONN_HOST);
		IPEndPoint remoteEP = new IPEndPoint(ipAdress, CONN_PORT);

		conn.Connect(remoteEP);

		new Thread(Input).Start();
	}

	private void Input() {	// Threaded
		while (true) {
			try {
				System.Diagnostics.Debug.WriteLine("x");
				Message _in = Recv();
				lock (queueLock) {
					queue.Enqueue(_in);
				}
			} catch (SocketException e) {
				break;
			}
		}
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
		
		lock (sendLock)
			conn.Send(packet, 0, packet.Length, SocketFlags.None);
	}

	public Message Recv() {
		int len;
		int tag;
		byte[] data;
		byte[] msgLen = new byte[sizeof(int)];
		byte[] msgTag = new byte[sizeof(int)];
		Message msg = new Message();

		conn.Receive(msgLen, 0, sizeof(int), SocketFlags.None);
		conn.Receive(msgTag, 0, sizeof(int), SocketFlags.None);

		len = BitConverter.ToInt32(msgLen, 0);
		tag = BitConverter.ToInt32(msgTag, 0);

		data = new byte[len];

		conn.Receive(data, 0, len, SocketFlags.None);

		msg.Data = Decode(data);
		msg.Len = len;
		msg.Tag = (SOCKET_TAG)tag;

		return msg;
    }

	public string Decode(byte[] data) {
		return ascii.GetString(data, 0, data.Length);
    }

}
