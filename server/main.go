package main

import (
	"bufio"
	"fmt"
	"net"
	"os"
	"strconv"
	"sync"
	"time"

	"./client"
)

const (
	CONN_HOST = "localhost"
	CONN_PORT = "3333"
	CONN_TYPE = "tcp"
)

const ( // SOCKET_TAG
	NOP      = 0
	SET_ID   = 1
	SEND_KEY = 2
)

var id int
var mutex sync.Mutex

func main() {
	id = 0

	l, err := net.Listen(CONN_TYPE, CONN_HOST+":"+CONN_PORT)

	if err != nil {
		fmt.Println("Error listening:", err.Error())
		os.Exit(1)
	}

	defer l.Close()

	fmt.Println("Listening on: " + CONN_HOST + ":" + CONN_PORT)

	for {
		conn, err := l.Accept()

		if err != nil {
			fmt.Println("Error accepting:", err.Error())
			continue
		}

		id++
		c := &client.Client{}
		c.Conn = conn
		c.Reader = bufio.NewReader(conn)
		c.ID = strconv.Itoa(id)

		go handleRequest(c)
	}
}

func handleRequest(c *client.Client) {
	defer c.Close()
	go clientInput(c)

	c.Send(SET_ID, []byte(c.ID))

	for {
		//fmt.Println("SENT:", msg.Tag, msg.Data)
		time.Sleep(time.Duration(50) * time.Millisecond)

		for c.Queue.Len > 0 {
			mutex.Lock()
			msg := c.Queue.Pop()
			mutex.Unlock()

			switch msg.Tag {
			case SEND_KEY:
				fmt.Println("KEY ->", msg.Data)
				break
			}
		}
	}
}

func clientInput(c *client.Client) {
	for {
		msg := &client.Message{}

		err := c.Recv(msg)

		if err != nil {
			fmt.Println("Error reading ["+c.ID+"]:", err.Error())
			return
		}

		//fmt.Println("RECIVED ["+c.ID+"] :", msg.Data)

		mutex.Lock()
		c.Queue.Push(msg)
		mutex.Unlock()
	}
}
