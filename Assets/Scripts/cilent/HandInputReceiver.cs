using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[Serializable]
public class FingerCurlData
{
    public float thumb = -1f;
    public float index = -1f;
    public float middle = -1f;
    public float ring = -1f;
    public float pinky = -1f;
}

[Serializable]
public class MinigameHandPacket
{
    public FingerCurlData left;
    public FingerCurlData right;
}

public class HandInputReceiver : MonoBehaviour
{
    // ───── UDP (CONTROL) ─────
    UdpClient  udpClient;
    public int udpPort = 5006;
    IPEndPoint udpEndPoint;

    // ───── Minigame UDP (Port 5052) ─────
    private UdpClient  minigameUdpClient;
    public int         minigameUdpPort = 5052;
    private Thread     minigameUdpThread;
    private readonly object minigameLock = new object();
    private string     latestMinigameJson;
    private bool       hasNewMinigameData = false;

    // ───── TCP (GESTURE) ─────
    public string tcpHost = "127.0.0.1";
    public int    tcpPort = 5005;
    TcpClient     tcpClient;
    NetworkStream stream;
    Thread        tcpThread;

    public bool IsConnected { get; private set; } = false;

    public GameObject player;
    public GameObject skill;
    public Sprite[] animalSprite;

    public GameObject ui_skill, ui_attack;

    public GameObject main_camera;

    private DepthOfField depthOfField;
    private SplitToning splitToning;
    // ───── STATE ─────
    public static string LeftHand    = "Idle";
    public static string RightHand   = "NONE";
    public static string Gesture     = "dont";
    public static string CurrentMode = "control";  // ← โหมดปัจจุบัน

    // ───── Finger Curl Data ─────
    public static FingerCurlData LeftHandCurl { get; private set; } = new FingerCurlData();
    public static FingerCurlData RightHandCurl { get; private set; } = new FingerCurlData();
    public static bool IsLeftHandDetected { get; private set; } = false;
    public static bool IsRightHandDetected { get; private set; } = false;

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
        // ─── [DEBUG] Keyboard Shortcuts สำหรับทดสอบสกิล (กด 0-4) ───
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("[DEBUG KEY] 0 → ปิด Gesture Mode (closebullet)");
            closebullet();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[DEBUG KEY] 1 → rabbit skill (รอ bullet time จบก่อน)");
            TriggerSkillAfterBulletTime("rabbit");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("[DEBUG KEY] 2 → dog skill (รอ bullet time จบก่อน)");
            TriggerSkillAfterBulletTime("dog");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("[DEBUG KEY] 3 → cow skill (รอ bullet time จบก่อน)");
            TriggerSkillAfterBulletTime("cow");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("[DEBUG KEY] 4 → deer skill (รอ bullet time จบก่อน)");
            TriggerSkillAfterBulletTime("deer");
        }
#endif

        // ─── ดึงข้อมูลจาก Queue มาใช้ใน Main Thread ───
        lock (queueLock)
        {
            while (tcpQueue.Count > 0)
            {
                string line = tcpQueue.Dequeue();
                ProcessTCPLine(line);
            }
        }

        // ─── ดึงข้อมูล Minigame UDP ───
        string minigameJson = null;
        lock (minigameLock)
        {
            if (hasNewMinigameData)
            {
                minigameJson = latestMinigameJson;
                hasNewMinigameData = false;
            }
        }

        if (minigameJson != null)
        {
            ProcessMinigameJson(minigameJson);
        }
        else if (minigameUdpClient == null)
        {
            // Fallback to HandDataReceiver
            IsLeftHandDetected = HandDataReceiver.LeftHandDetected;
            IsRightHandDetected = HandDataReceiver.RightHandDetected;
            
            if (IsLeftHandDetected && HandDataReceiver.LeftHand != null)
            {
                LeftHandCurl.thumb = HandDataReceiver.LeftHand.thumb;
                LeftHandCurl.index = HandDataReceiver.LeftHand.index;
                LeftHandCurl.middle = HandDataReceiver.LeftHand.middle;
                LeftHandCurl.ring = HandDataReceiver.LeftHand.ring;
                LeftHandCurl.pinky = HandDataReceiver.LeftHand.pinky;
            }
            if (IsRightHandDetected && HandDataReceiver.RightHand != null)
            {
                RightHandCurl.thumb = HandDataReceiver.RightHand.thumb;
                RightHandCurl.index = HandDataReceiver.RightHand.index;
                RightHandCurl.middle = HandDataReceiver.RightHand.middle;
                RightHandCurl.ring = HandDataReceiver.RightHand.ring;
                RightHandCurl.pinky = HandDataReceiver.RightHand.pinky;
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
                IsConnected = true;
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
                IsConnected = false;
                Debug.Log("[TCP] Disconnected");
            }
            catch (Exception e)
            {
                IsConnected = false;
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
                TriggerSkillAfterBulletTime(value);
                break;

            default:
                Debug.LogWarning($"[TCP] Unknown code: {line}");
                break;
        }
    }

    // =========================
    // EVENTS — แก้ตามต้องการ
    // =========================
    public void OnModeChanged(string mode)
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
    public void closebullet()
    {
        if (BulletTime.Instance != null)
        {
            ui_skill.SetActive(false);
            ui_attack.SetActive(true);
            BulletTime.Instance.Exit();
            depthOfField.active = false;
            splitToning.active = false;
        }
    }

    void OnGestureDont()
    {
        // มือหายหรือไม่แน่ใจ
    }

    // ─── เรียกตัวนี้แทน OnGestureDetected ───
    // Flow: เปิด bullet time → รอ bulletTimeDuration → ปิด bullet time → รอ timeScale ปกติ → ยิงสกิล
    [Header("Skill Trigger Settings")]
    public float bulletTimeDuration = 3f;  // รอใน bullet time กี่วินาที (unscaled)

    public void TriggerSkillAfterBulletTime(string gesture)
    {
        StartCoroutine(SkillSequence(gesture));
    }

    private System.Collections.IEnumerator SkillSequence(string gesture)
    {
        // 1) เปิด Bullet Time + UI Skill
        OnModeChanged("gesture");
        Debug.Log($"[Skill] เข้า bullet time → สกิล: {gesture}");

        // 2) รอใน bullet time bulletTimeDuration วินาที (ใช้ unscaledDeltaTime เพราะ timeScale สลอว)
        float elapsed = 0f;
        while (elapsed < bulletTimeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Debug.Log($"[Skill] bullet time {bulletTimeDuration}วิ จบ → ปิด slow motion");

        // 3) ปิด Bullet Time
        closebullet();

        // 4) รอให้ timeScale กลับใกล้ปกติ (>= 0.95)
        float waitTimeout = 3f;
        float waitElapsed = 0f;
        while (!BulletTime.Instance.IsTimeScaleNormal())
        {
            waitElapsed += Time.unscaledDeltaTime;
            if (waitElapsed >= waitTimeout)
            {
                Debug.LogWarning("[Skill] Timeout รอสรุป time scale — ยิงสกิลทันที");
                break;
            }
            yield return null;
        }

        // 5) ยิงสกิล
        Debug.Log($"[Skill] timeScale ปกติแล้ว → FireSkill: {gesture}");
        FireSkill(gesture);
    }

    // ─── Logic จริงของการเรียกสกิล (เดิมคือ OnGestureDetected) ───
    public void OnGestureDetected(string gesture) => TriggerSkillAfterBulletTime(gesture);

    private void FireSkill(string gesture)
    {
        var img = skill.GetComponent<Image>();
        switch (gesture)
        {
            case "rabbit":
                img.sprite = animalSprite[0];
                player.GetComponent<rabbitskill>().StartSkill();
                break;
            case "dog":
                img.sprite = animalSprite[1];
                player.GetComponent<dogskill>().StartSkill();
                break;
            case "cow":
                if (animalSprite != null && animalSprite.Length > 2)
                    img.sprite = animalSprite[2];
                var cowComp = player.GetComponent<cowskill>();
                if (cowComp != null) cowComp.StartSkill();
                break;
            case "deer":
                if (animalSprite != null && animalSprite.Length > 3)
                    img.sprite = animalSprite[3];
                Debug.LogWarning("[Skill] deer skill: ยังไม่มี component deerskิll — เพิ่ม script ได้ภายหลัง");
                // TODO: เพิ่มเมื่อมี deerskิll.cs แล้ว: player.GetComponent<deerskิll>().StartSkill();
                break;
            default:
                Debug.LogWarning($"[Skill] ไม่รู้จักสกิล: {gesture}");
                break;
        }
    }

    // =========================
    // MINIGAME CONTROL & UDP
    // =========================
    public void StartMinigame()
    {
        SendTCP("startminigame\n");
        StartMinigameUDP();
    }

    public void StopMinigame()
    {
        SendTCP("stopminigame\n");
        StopMinigameUDP();
    }

    private void SendTCP(string msg)
    {
        try
        {
            if (tcpClient != null && tcpClient.Connected && stream != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(msg);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
            Debug.Log("[TCP] Sent: " + msg);
        }
        catch (Exception e)
        {
            Debug.LogError("[TCP] Error sending message: " + e.Message);
        }
    }

    private void StartMinigameUDP()
    {
        lock (minigameLock)
        {
            if (minigameUdpThread != null && minigameUdpThread.IsAlive) return;

            try
            {
                minigameUdpClient = new UdpClient();
                minigameUdpClient.Client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReuseAddress, true
                );
                minigameUdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, minigameUdpPort));
                
                minigameUdpThread = new Thread(ReceiveMinigameUDPLoop);
                minigameUdpThread.IsBackground = true;
                minigameUdpThread.Start();
                Debug.Log($"[Minigame UDP] Started on port {minigameUdpPort}");
            }
            catch (SocketException se)
            {
                if (minigameUdpClient != null)
                {
                    minigameUdpClient.Close();
                    minigameUdpClient = null;
                }
                Debug.LogWarning($"[Minigame UDP] Port {minigameUdpPort} already in use or access forbidden. Falling back to HandDataReceiver. Details: {se.Message}");
            }
            catch (Exception e)
            {
                if (minigameUdpClient != null)
                {
                    minigameUdpClient.Close();
                    minigameUdpClient = null;
                }
                Debug.LogError($"[Minigame UDP] Failed to start: {e.Message}");
            }
        }
    }

    public void StopMinigameUDP()
    {
        lock (minigameLock)
        {
            try
            {
                if (minigameUdpClient != null)
                {
                    minigameUdpClient.Close();
                    minigameUdpClient = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Minigame UDP] Error closing: {e.Message}");
            }

            if (minigameUdpThread != null && minigameUdpThread.IsAlive)
            {
                minigameUdpThread.Join(200);
                minigameUdpThread = null;
            }
            Debug.Log("[Minigame UDP] Stopped");
        }
    }

    private void ReceiveMinigameUDPLoop()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        while (isRunning && minigameUdpClient != null)
        {
            try
            {
                byte[] data = minigameUdpClient.Receive(ref anyIP);
                string json = Encoding.UTF8.GetString(data);
                lock (minigameLock)
                {
                    latestMinigameJson = json;
                    hasNewMinigameData = true;
                }
            }
            catch (SocketException)
            {
                // Normal shutdown when closed
                break;
            }
            catch (Exception e)
            {
                if (isRunning)
                {
                    Debug.LogWarning($"[Minigame UDP] Receive error: {e.Message}");
                }
            }
        }
    }

    private void ProcessMinigameJson(string json)
    {
        // JsonUtility doesn't handle "null" fields nicely, so we replace them with sentinel values (e.g., -1 for all curls)
        json = json.Replace("null", "{\"thumb\":-1,\"index\":-1,\"middle\":-1,\"ring\":-1,\"pinky\":-1}");

        try
        {
            MinigameHandPacket packet = JsonUtility.FromJson<MinigameHandPacket>(json);
            
            IsLeftHandDetected = packet.left != null && packet.left.index >= 0f;
            IsRightHandDetected = packet.right != null && packet.right.index >= 0f;

            if (IsLeftHandDetected)
            {
                LeftHandCurl = packet.left;
            }
            if (IsRightHandDetected)
            {
                RightHandCurl = packet.right;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Minigame UDP] JSON parse error: {e.Message}\n{json}");
        }
    }

    // =========================
    // CLEANUP
    // =========================
    void OnApplicationQuit()
    {
        isRunning = false;
        StopMinigameUDP();
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