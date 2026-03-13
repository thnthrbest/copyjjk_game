using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class GestureReceiver : MonoBehaviour
{
    [Header("Socket Settings")]
    public string host = "localhost";
    public int port = 5005;

    [Header("Gesture Prefabs")]
    // public GameObject rabbitPrefab;
    // public GameObject birdPrefab;
    // public GameObject frogPrefab;

    [Header("skill Settings")]
    // public Vector3 spawnPosition = Vector3.zero;  // ตำแหน่ง spawn
    // public bool destroyPrevious = true;            // ลบ obj เก่าก่อน spawn ใหม่
    public GameObject player;  // อ้างอิงถึงสกิลโล่กระต่าย

    [Header("Debug")]
    public string latestGesture = "";
    public bool isConnected = false;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = false;

    // ───── Timer ─────
    private float gestureTimer = 0f;
    private bool  isWaiting    = false;
    private float WAIT_SECONDS = 5f;

    // ───── Spawn ─────
    //  private GameObject currentObj = null;  // เก็บ obj ที่ spawn ล่าสุด

    // ───── Queue ─────
    private readonly Queue<string> gestureQueue = new Queue<string>();
    private readonly object queueLock = new object();

    void Start()
    {
        ConnectToPython();
    }

    void ConnectToPython()
    {
        try
        {
            client      = new TcpClient(host, port);
            stream      = client.GetStream();
            isConnected = true;
            isRunning   = true;

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

                    foreach (string msg in raw.Split('\n'))
                    {
                        string m = msg.Trim();
                        if (!string.IsNullOrEmpty(m))
                        {
                            lock (queueLock)
                            {
                                gestureQueue.Enqueue(m);
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
        // ─── ดึงข้อมูลจาก Queue ───
        lock (queueLock)
        {
            while (gestureQueue.Count > 0)
            {
                OnGestureReceived(gestureQueue.Dequeue());
            }
        }

        // ─── นับ Timer ───
        if (isWaiting)
        {
            gestureTimer += Time.deltaTime;

            float remaining = WAIT_SECONDS - gestureTimer;
            Debug.Log($"[Gesture] รออีก {remaining:F1} วิ... (ท่าล่าสุด: {latestGesture})");

            if (gestureTimer >= WAIT_SECONDS)
            {
                isWaiting    = false;
                gestureTimer = 0f;
                ProcessGesture(latestGesture);
            }
        }
    }

    void OnGestureReceived(string raw)
    {
        string[] parts = raw.Split(':');
        if (parts.Length != 2) return;

        string code    = parts[0];
        string gesture = parts[1];

        switch (code)
        {
            case "1":
                if (isWaiting)
                {
                    Debug.Log("[Gesture] มือหายไประหว่างรอ — ยกเลิก timer");
                    isWaiting     = false;
                    gestureTimer  = 0f;
                    latestGesture = "";
                }
                break;

            case "2":
                latestGesture = gesture;
                gestureTimer  = 0f;
                isWaiting     = true;
                Debug.Log($"[Gesture] ได้ท่า '{gesture}' — เริ่มนับ {WAIT_SECONDS} วิ");
                break;
        }
    }

    void ProcessGesture(string gesture)
    {
        Debug.Log($"[Gesture] ✅ ครบ {WAIT_SECONDS} วิ — spawn: {gesture}");
        GetPrefabByGesture(gesture);

        // if (prefab == null)
        // {
        //     Debug.LogWarning($"[Gesture] ไม่พบ Prefab สำหรับท่า: {gesture}");
        //     return;
        // }

        // ─── ลบ obj เก่าถ้าเปิด destroyPrevious ───
        // if (destroyPrevious && currentObj != null)
        // {
        //     Destroy(currentObj);
        //     currentObj = null;
        //     Debug.Log("[Gesture] ลบ obj เก่าแล้ว");
        // }

        // ─── Spawn obj ใหม่ ───
        // currentObj = Instantiate(prefab, spawnPosition, Quaternion.identity);
        // currentObj.name = gesture + "_spawned";
    
       // Debug.Log($"[Gesture] Spawn {gesture} ที่ {spawnPosition}");
    }

    void GetPrefabByGesture(string gesture)
    {
        switch (gesture)
        {
            case "rabbit":player.GetComponent<rabbitskill>().StartSkill();
            break;
                
            // case "bird":   
            // case "frog":
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