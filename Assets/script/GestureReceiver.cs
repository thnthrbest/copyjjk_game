using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class GestureReceiver : MonoBehaviour
{
    [Header("Socket Settings")]
    public string host = "localhost";
    public int port = 5005;

    [Header("Debug")]
    public string latestGesture = "";
    public bool isConnected = false;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = false;

    // Queue สำหรับส่งข้อมูลจาก Thread มายัง Main Thread
    private readonly System.Collections.Generic.Queue<string> gestureQueue
        = new System.Collections.Generic.Queue<string>();
    private readonly object queueLock = new object();

    void Start()
    {
        ConnectToPython();
    }

    void ConnectToPython()
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            isConnected = true;
            isRunning = true;

            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("[Gesture] เชื่อมต่อกับ Python สำเร็จ!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Gesture] เชื่อมต่อไม่ได้: {e.Message}");
            isConnected = false;
        }
    }

    void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        while (isRunning)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string raw = Encoding.UTF8
                        .GetString(buffer, 0, bytesRead)
                        .Trim();

                    // อาจมีหลาย message ในครั้งเดียว แยกด้วย newline
                    foreach (string msg in raw.Split('\n'))
                    {
                        string gesture = msg.Trim();
                        if (!string.IsNullOrEmpty(gesture))
                        {
                            lock (queueLock)
                            {
                                gestureQueue.Enqueue(gesture);
                            }
                        }
                    }
                }
            }
            catch
            {
                isConnected = false;
                break;
            }
        }
    }

    void Update()
    {
        // ดึงข้อมูลจาก Queue มาใช้ใน Main Thread
        lock (queueLock)
        {
            while (gestureQueue.Count > 0)
            {
                string gesture = gestureQueue.Dequeue();
                latestGesture = gesture;
                OnGestureReceived(gesture);
            }
        }
    }

    void OnGestureReceived(string gesture)
    {
        Debug.Log($"[Gesture] ได้รับท่า: {gesture}");

        // ─── เพิ่ม Logic ของเกมตรงนี้ ───
        switch (gesture)
        {
            case "rabbit":
                Debug.Log("ท่ากระต่าย!");
                // GetComponent<Animator>().SetTrigger("Rabbit");
                break;

            case "bird":
                Debug.Log("ท่านก!");
                // GetComponent<Animator>().SetTrigger("Bird");
                break;

            case "frog":
                Debug.Log("ท่ากบ!");
                // GetComponent<Animator>().SetTrigger("Frog");
                break;

            default:
                Debug.Log($"ท่าที่ไม่รู้จัก: {gesture}");
                break;
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        receiveThread?.Abort();
        stream?.Close();
        client?.Close();
    }
}