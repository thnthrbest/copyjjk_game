using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class HandInputReceiver : MonoBehaviour
{
    // ───── UDP (CONTROL) ─────
    UdpClient udpClient;
    public int udpPort = 5006;
    IPEndPoint udpEndPoint;

    // ───── TCP (GESTURE) ─────
    public string tcpHost = "127.0.0.1"; // 🔥 connect ไป Python
    public int tcpPort = 5005;
    TcpClient tcpClient;
    NetworkStream stream;
    Thread tcpThread;

    // ───── STATE ─────
    public static string LeftHand = "IDLE";
    public static string RightHand = "NONE";
    public static string Gesture = "dont";

    private bool isRunning = true;

    void Start()
    {
        StartUDP();
        StartTCP();
    }

    // =========================
    // UDP RECEIVER
    // =========================
    void StartUDP()
    {
        try
        {
            udpClient = new UdpClient(udpPort);
            udpEndPoint = new IPEndPoint(IPAddress.Any, udpPort);
            udpClient.BeginReceive(ReceiveUDP, null);

            Debug.Log("UDP Started on port " + udpPort);
        }
        catch (Exception e)
        {
            Debug.LogError("UDP Error: " + e);
        }
    }

    void ReceiveUDP(IAsyncResult result)
    {
        if (!isRunning) return;

        try
        {
            byte[] data = udpClient.EndReceive(result, ref udpEndPoint);
            string msg = Encoding.UTF8.GetString(data);

            ParseUDP(msg);

            if (isRunning)
                udpClient.BeginReceive(ReceiveUDP, null);
        }
        catch (ObjectDisposedException) { }
        catch (Exception e)
        {
            if (isRunning)
                Debug.LogError("UDP Error: " + e);
        }
    }

    void ParseUDP(string msg)
    {
        string[] lines = msg.Split('\n');

        foreach (string line in lines)
        {
            if (line.StartsWith("L:"))
                LeftHand = line.Replace("L:", "").Trim();
            else if (line.StartsWith("R:"))
                RightHand = line.Replace("R:", "").Trim();
        }
    }

    // =========================
    // TCP CLIENT (สำคัญ)
    // =========================
    void StartTCP()
    {
        tcpThread = new Thread(TCPClientLoop);
        tcpThread.IsBackground = true;
        tcpThread.Start();
    }

    void TCPClientLoop()
    {
        while (isRunning)
        {
            try
            {
                Debug.Log("Connecting to Python TCP...");

                tcpClient = new TcpClient();
                tcpClient.Connect(tcpHost, tcpPort); // 🔥 connect ไป Python

                stream = tcpClient.GetStream();
                Debug.Log("TCP Connected!");

                byte[] buffer = new byte[1024];

                while (tcpClient.Connected && isRunning)
                {
                    int length = stream.Read(buffer, 0, buffer.Length);
                    if (length == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, length);
                    ParseTCP(msg);
                }

                tcpClient.Close();
                Debug.Log("TCP Disconnected");
            }
            catch (Exception e)
            {
                if (isRunning)
                {
                    Debug.LogWarning("Retry TCP in 1 sec... " + e.Message);
                    Thread.Sleep(1000); // 🔁 retry
                }
            }
        }
    }

    void ParseTCP(string msg)
    {
        string[] lines = msg.Split('\n');

        foreach (string line in lines)
        {
            if (line.StartsWith("1:"))
                Gesture = line.Replace("1:", "").Trim();
            else if (line.StartsWith("2:"))
                Gesture = line.Replace("2:", "").Trim();
        }

        Debug.Log("[TCP] Gesture = " + Gesture);
    }

    // =========================
    // CLEANUP
    // =========================
    void OnApplicationQuit()
    {
        isRunning = false;

        try
        {
            udpClient?.Close();
            tcpClient?.Close();
        }
        catch { }

        if (tcpThread != null && tcpThread.IsAlive)
        {
            tcpThread.Join(500);
        }
    }
}