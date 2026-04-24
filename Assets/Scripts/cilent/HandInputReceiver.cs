using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class HandInputReceiver : MonoBehaviour
{
    // ───── UDP (CONTROL) ─────
    UdpClient  udpClient;
    public int udpPort = 5006;
    IPEndPoint udpEndPoint;

    // ───── TCP (GESTURE) ─────
    public string tcpHost = "127.0.0.1";
    public int    tcpPort = 5005;
    TcpClient     tcpClient;
    NetworkStream stream;
    Thread        tcpThread;

    public GameObject player;

    public GameObject ui_skill, ui_attack;

    public GameObject main_camera;

    private DepthOfField depthOfField;
    private SplitToning splitToning;
    // ───── STATE ─────
    public static string LeftHand    = "IDLE";
    public static string RightHand   = "NONE";
    public static string Gesture     = "dont";
    public static string CurrentMode = "control";  // ← โหมดปัจจุบัน

    // ───── Queue สำหรับส่งข้อมูลมา Main Thread ─────
    private readonly Queue<string> tcpQueue  = new Queue<string>();
    private readonly object        queueLock = new object();

    private bool isRunning = true;

    void Start()
    {
        StartUDP();
        StartTCP();
        Volume volume = main_camera.GetComponent<Volume>();

        if (volume != null && volume.profile != null)
        {
            // ดึง Depth Of Field
            if (volume.profile.TryGet(out depthOfField))
            {
                ///depthOfField.active = true;
            }

            // ดึง Split Toning
            if (volume.profile.TryGet(out splitToning))
            {
                //splitToning.active = true;
            }
        }
    }

    void Update()
    {
        // ─── ดึงข้อมูลจาก Queue มาใช้ใน Main Thread ───
        lock (queueLock)
        {
            while (tcpQueue.Count > 0)
            {
                string line = tcpQueue.Dequeue();
                ProcessTCPLine(line);
            }
        }
    }

    // =========================
    // UDP
    // =========================
    void StartUDP()
    {
        try
        {
            udpClient   = new UdpClient();
            udpClient.Client.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress, true
            );
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, udpPort));
            udpEndPoint = new IPEndPoint(IPAddress.Any, udpPort);
            udpClient.BeginReceive(ReceiveUDP, null);
            Debug.Log("[UDP] Started on port " + udpPort);
        }
        catch (Exception e)
        {
            Debug.LogError("[UDP] Error: " + e);
        }
    }

    void ReceiveUDP(IAsyncResult result)
    {
        if (!isRunning) return;
        try
        {
            byte[] data = udpClient.EndReceive(result, ref udpEndPoint);
            string msg  = Encoding.UTF8.GetString(data);
            ParseUDP(msg);
            if (isRunning) udpClient.BeginReceive(ReceiveUDP, null);
        }
        catch (ObjectDisposedException) { }
        catch (Exception e)
        {
            if (isRunning) Debug.LogError("[UDP] Error: " + e);
        }
    }

    void ParseUDP(string msg)
    {
        string[] lines = msg.Split('\n');
        foreach (string line in lines)
        {
            if (line.StartsWith("L:"))
                LeftHand  = line.Replace("L:", "").Trim();
            else if (line.StartsWith("R:"))
                RightHand = line.Replace("R:", "").Trim();
        }
    }

    // =========================
    // TCP
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
                Debug.Log("[TCP] Connecting to Python...");
                tcpClient = new TcpClient();
                tcpClient.Connect(tcpHost, tcpPort);
                stream = tcpClient.GetStream();
                Debug.Log("[TCP] Connected!");

                byte[]        buffer  = new byte[1024];
                StringBuilder builder = new StringBuilder();

                while (tcpClient.Connected && isRunning)
                {
                    int length = stream.Read(buffer, 0, buffer.Length);
                    if (length == 0) break;

                    string chunk = Encoding.UTF8.GetString(buffer, 0, length);
                    builder.Append(chunk);

                    // ─── แยกข้อความด้วย newline ───
                    string raw = builder.ToString();
                    int    idx;

                    while ((idx = raw.IndexOf('\n')) >= 0)
                    {
                        string line = raw.Substring(0, idx).Trim();
                        raw         = raw.Substring(idx + 1);

                        if (!string.IsNullOrEmpty(line))
                        {
                            lock (queueLock)
                            {
                                tcpQueue.Enqueue(line);
                            }
                        }
                    }

                    builder.Clear();
                    builder.Append(raw);
                }

                tcpClient.Close();
                Debug.Log("[TCP] Disconnected");
            }
            catch (Exception e)
            {
                if (isRunning)
                {
                    Debug.LogWarning("[TCP] Retry in 1s... " + e.Message);
                    Thread.Sleep(1000);
                }
            }
        }
    }

    void ProcessTCPLine(string line)
    {
        string[] parts = line.Split(':');
        if (parts.Length != 2) return;

        string code  = parts[0].Trim();
        string value = parts[1].Trim();

        switch (code)
        {
            // ─── 0 = เปลี่ยนโหมด ───
            case "0":
                CurrentMode = value;
                Debug.Log($"[TCP] Mode → {value.ToUpper()}");
                OnModeChanged(value);
                break;

            // ─── 1 = dont / ไม่พบมือ ───
            case "1":
                Gesture = "dont";
                Debug.Log("[TCP] Gesture = dont");
                OnGestureDont();
                break;

            // ─── 2 = จำแนกท่าได้ ───
            case "2":
                Gesture = value;
                Debug.Log($"[TCP] Gesture = {value}");
                OnGestureDetected(value);
                break;

            default:
                Debug.LogWarning($"[TCP] Unknown code: {line}");
                break;
        }
    }

    // =========================
    // EVENTS — แก้ตามต้องการ
    // =========================
    void OnModeChanged(string mode)
    {
        if (mode == "gesture")
        {
            Debug.Log("[Mode] GESTURE MODE");

            // ─── เปิด Bullet Time ───
            if (BulletTime.Instance != null)
            {
                ui_skill.SetActive(true);
                ui_attack.SetActive(false);
                BulletTime.Instance.Enter();
                depthOfField.active = true;
                splitToning.active = true;
            }
                
        }
        else if (mode == "control")
        {
            Debug.Log("[Mode] CONTROL MODE");
            Gesture = "dont";

            // ─── ปิด Bullet Time ───
            if (BulletTime.Instance != null)
            {
                ui_skill.SetActive(false);
                ui_attack.SetActive(true);
                BulletTime.Instance.Exit();
                depthOfField.active = false;
                splitToning.active = false;
            }

                
        }
    }

    void OnGestureDont()
    {
        // มือหายหรือไม่แน่ใจ
    }

    void OnGestureDetected(string gesture)
    {
         switch (gesture)
        {
            case "rabbit":player.GetComponent<rabbitskill>().StartSkill();
            break;
        }
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
            tcpThread.Join(500);
    }
}   