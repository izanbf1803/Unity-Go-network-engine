package client

import (
	"bufio"
	"encoding/binary"
	"fmt"
	"io"
	"net"
)

/* CLIENT SOCKET STRUCT:
bytes [0:4]   = data length
bytes [5:8]   = data tag
bytes [9:len] = data
*/

// MsgQueue to handle client sockets
type MsgQueue struct {
	Len  int
	head *Message
	tail *Message
}

// Client struct
type Client struct {
	ID     string
	Reader *bufio.Reader
	Conn   net.Conn
	Queue  MsgQueue
}

// Message recived from client [ len | tag | data... ]
type Message struct {
	Len     int
	Tag     uint32
	Data    string
	RawData []byte
	next    *Message
}

// Send data to client
func (client *Client) Send(tag uint32, data []byte) error {
	dataLen := len(data)
	packet := make([]byte, 8, 8+dataLen)

	packet[0] = byte(dataLen)
	packet[1] = byte(dataLen >> 8)
	packet[2] = byte(dataLen >> 16)
	packet[3] = byte(dataLen >> 24)

	packet[0] = byte(tag)
	packet[1] = byte(tag >> 8)
	packet[2] = byte(tag >> 16)
	packet[3] = byte(tag >> 24)

	packet = append(packet, data...)

	fmt.Println(packet)

	_, err := client.Conn.Write(packet)

	return err
}

// Recv data from client
func (client *Client) Recv(buffer *Message) error {
	var msgLen [4]byte
	var msgTag [4]byte
	var len uint32
	var tag uint32
	var err error

	_, err = io.ReadFull(client.Reader, msgLen[:])
	if err != nil {
		return err
	}

	_, err = io.ReadFull(client.Reader, msgTag[:])
	if err != nil {
		return err
	}

	len = binary.LittleEndian.Uint32(msgLen[:])
	tag = binary.LittleEndian.Uint32(msgTag[:])

	data := make([]byte, len)
	_, err = io.ReadFull(client.Reader, data)

	buffer.RawData = data
	buffer.Data = string(data)
	buffer.Len = int(len)
	buffer.Tag = tag

	return err
}

// Close connection with client
func (client *Client) Close() {
	client.Conn.Close()
}

/////////////////////////////////

// Push Message to queue
func (q *MsgQueue) Push(msg *Message) {
	if q.head == nil {
		q.head = msg
		q.tail = msg
		return
	}

	q.tail.next = msg
	q.tail = msg

	q.Len++
}

// Pop Message to queue and get value
func (q *MsgQueue) Pop() *Message {
	if q.head == nil {
		return nil
	}

	msg := &Message{}

	msg.Tag = q.head.Tag
	msg.Len = q.head.Len
	msg.Data = q.head.Data
	msg.RawData = q.head.RawData

	q.head = q.head.next

	q.Len--

	return msg
}

/*
m := &client.Message{}
m2 := &client.Message{}

m.Tag = SET_ID
m.Data = "hello"
m.Len = len(m.Data)
m.RawData = []byte(m.Data)

m2.Tag = NOP
m2.Data = "bye"
m2.Len = len(m2.Data)
m2.RawData = []byte(m2.Data)

queue := client.MsgQueue{}
queue.Push(m)
queue.Push(m2)

fmt.Println(queue.Pop())
fmt.Println(queue.Pop())
fmt.Println(queue.Pop())
*/
