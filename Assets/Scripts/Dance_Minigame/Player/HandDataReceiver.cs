using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// ค่าการงอของนิ้วแต่ละนิ้ว (0 = เหยียดตรง, 1 = งอสุด)
/// ชื่อ field ต้องตรงกับ key ใน JSON ที่ฝั่ง Python ส่งมา
/// </summary>
[Serializable]
public class FingerData
{
    public float thumb;
    public float index;
    public float middle;
    public float ring;
    public float pinky;
}

/// <summary>โครงสร้าง packet ที่รับจาก Python ผ่าน UDP</summary>
[Serializable]
public class HandPacket
{
    public FingerData left;
    public FingerData right;
}

/// <summary>
/// รับข้อมูลนิ้วมือจาก hand_tracker.py ผ่าน UDP
/// เข้าถึงค่าล่าสุดได้ผ่าน HandDataReceiver.LeftHand / RightHand (static)
/// </summary>
public class HandDataReceiver : MonoBehaviour
{
    [Header("Network")]
    [Tooltip("ต้องตรงกับ PORT ในไฟล์ hand_tracker.py")]
    public int port = 5052;

    [Header("Debug")]
    public bool logRawJson = false;

    /// <summary>ค่านิ้วของมือซ้ายล่าสุด (คงค่าเดิมไว้ถ้ามือหายไปจากกล้อง)</summary>
    public static FingerData LeftHand { get; private set; } = new FingerData();

    /// <summary>ค่านิ้วของมือขวาล่าสุด</summary>
    public static FingerData RightHand { get; private set; } = new FingerData();

    /// <summary>true ถ้าพบมือซ้ายในเฟรมล่าสุด</summary>
    public static bool LeftHandDetected { get; private set; }

    /// <summary>true ถ้าพบมือขวาในเฟรมล่าสุด</summary>
    public static bool RightHandDetected { get; private set; }

    UdpClient _client;
    Thread _thread;
    volatile bool _running;
    readonly object _lock = new object();
    string _latestJson;
    bool _hasNewData;

    void Start()
    {
        try
        {
            _client = new UdpClient(port);
            _running = true;
            _thread = new Thread(ReceiveLoop) { IsBackground = true };
            _thread.Start();
            Debug.Log($"[HandDataReceiver] กำลังฟังที่ UDP port {port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[HandDataReceiver] เปิด port {port} ไม่ได้: {e.Message}");
        }
    }

    void ReceiveLoop()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        while (_running)
        {
            try
            {
                byte[] data = _client.Receive(ref anyIP);
                string json = Encoding.UTF8.GetString(data);

                lock (_lock)
                {
                    _latestJson = json;
                    _hasNewData = true;
                }
            }
            catch (SocketException)
            {
                // เกิดตอนปิดโปรแกรม (socket ถูกปิด) - ไม่ต้องแจ้งเตือน
            }
            catch (Exception e)
            {
                if (_running)
                    Debug.LogWarning($"[HandDataReceiver] รับข้อมูลผิดพลาด: {e.Message}");
            }
        }
    }

    void Update()
    {
        string json = null;
        lock (_lock)
        {
            if (_hasNewData)
            {
                json = _latestJson;
                _hasNewData = false;
            }
        }

        if (json == null) return;
        if (logRawJson) Debug.Log($"[HandDataReceiver] {json}");

        // JsonUtility ไม่รองรับ "null" เป็นค่า field
        // จึงแทนที่ด้วย object sentinel ที่ทุกนิ้ว = -1 (ค่าปกติอยู่ระหว่าง 0-1)
        json = json.Replace("null", "{\"thumb\":-1,\"index\":-1,\"middle\":-1,\"ring\":-1,\"pinky\":-1}");

        try
        {
            HandPacket packet = JsonUtility.FromJson<HandPacket>(json);

            LeftHandDetected  = packet.left  != null && packet.left.index  >= 0f;
            RightHandDetected = packet.right != null && packet.right.index >= 0f;

            // คงค่าล่าสุดไว้ถ้ามือหายไปชั่วคราว (กันท่ากระตุกกลับ rest position)
            if (LeftHandDetected)  LeftHand  = packet.left;
            if (RightHandDetected) RightHand = packet.right;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[HandDataReceiver] JSON parse error: {e.Message}\n{json}");
        }
    }

    void OnDestroy()         => Close();
    void OnApplicationQuit() => Close();

    void Close()
    {
        _running = false;
        _client?.Close();
        _thread?.Join(200);
    }
}