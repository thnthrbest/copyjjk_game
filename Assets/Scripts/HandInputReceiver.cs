using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class HandInputReceiver : MonoBehaviour
{

    UdpClient udpClient;

    public int port = 5055;

    IPEndPoint endPoint;

    public static string LeftHand = "IDLE";
    public static string RightHand = "NONE";

    void Start()
    {

        try
        {

            udpClient = new UdpClient(port);

            endPoint = new IPEndPoint(IPAddress.Any, port);

            udpClient.BeginReceive(ReceiveData, null);

            Debug.Log("UDP Receiver Started on port " + port);

        }
        catch (Exception e)
        {

            Debug.LogError("UDP Start Error: " + e);

        }

    }

    void ReceiveData(IAsyncResult result)
    {

        try
        {

            byte[] data = udpClient.EndReceive(result, ref endPoint);

            string message = Encoding.UTF8.GetString(data);

            Debug.Log("Packet Received:\n" + message);

            ParsePacket(message);

            udpClient.BeginReceive(ReceiveData, null);

        }
        catch (Exception e)
        {

            Debug.LogError("UDP Receive Error: " + e);

        }

    }

    void ParsePacket(string msg)
    {

        string[] lines = msg.Split('\n');

        foreach (string line in lines)
        {

            if (line.StartsWith("L:"))
            {

                LeftHand = line.Replace("L:", "").Trim();

                Debug.Log("Left Hand = " + LeftHand);

            }

            if (line.StartsWith("R:"))
            {

                RightHand = line.Replace("R:", "").Trim();

                Debug.Log("Right Hand = " + RightHand);

            }

        }

    }

}