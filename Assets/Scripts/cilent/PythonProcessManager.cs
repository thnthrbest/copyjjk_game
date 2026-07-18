using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// จัดการเปิด-ปิดตัว Python (main.py) อัตโนมัติเมื่อเข้า/ออกจากเกม
/// - โหมด Editor: ใช้ python จาก PATH ของเครื่อง + path แบบ hardcode (สำหรับ dev เท่านั้น)
/// - โหมด Build: ใช้ python.exe จาก venv ที่อยู่ข้างๆ ตัวเกม (PythonApp/env/Scripts/python.exe)
/// วิธีใช้: สร้าง Empty GameObject ชื่อ "PythonManager" แล้วลากสคริปต์นี้ใส่ วางไว้ใน Scene แรกของเกม
/// </summary>
public class PythonProcessManager : MonoBehaviour
{
    public static PythonProcessManager Instance { get; private set; }

    [Header("Editor (Dev) Settings")]
    [Tooltip("Path เต็มไปยังโฟลเดอร์ที่มี main.py (เฉพาะตอนรันใน Unity Editor)")]
    [SerializeField] private string devPythonFolderPath = @"C:\Users\comsc\Documents\GitHub\GestureController";

    [Tooltip("คำสั่ง python ที่ใช้ตอน Editor (ปกติคือ 'python' ถ้าอยู่ใน PATH แล้ว)")]
    [SerializeField] private string devPythonCommand = "python";

    [Header("Build Settings")]
    [Tooltip("ชื่อโฟลเดอร์ที่เก็บไฟล์ Python ทั้งหมด (วางข้างๆ ตัว .exe ของเกมตอน build)")]
    [SerializeField] private string buildPythonFolderName = "PythonApp";

    [Header("Options")]
    [Tooltip("แสดง log จาก Python ใน Unity Console หรือไม่")]
    [SerializeField] private bool logPythonOutput = true;

    [Tooltip("เปิด Python อัตโนมัติตอน Start() หรือไม่ (ถ้าปิด ต้องเรียก StartPython() เอง)")]
    [SerializeField] private bool autoStartOnAwake = true;

    private Process pythonProcess;
    private bool isPythonRunning = false;

    public bool IsPythonRunning => isPythonRunning;

    void Awake()
    {
        // ป้องกันมี PythonProcessManager ซ้อนกันหลายตัวข้าม Scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (autoStartOnAwake)
        {
            StartPython();
        }
    }

    /// <summary>
    /// สั่งเปิด main.py เป็น background process
    /// </summary>
    public void StartPython()
    {
        if (isPythonRunning)
        {
            Debug.LogWarning("[Python] Process กำลังทำงานอยู่แล้ว ไม่เปิดซ้ำ");
            return;
        }

        string pythonExePath;
        string workingDir;
        string arguments;

        if (Application.isEditor)
        {
            // ----- โหมดพัฒนา (กด Play ใน Unity Editor) -----
            workingDir = devPythonFolderPath;
            pythonExePath = devPythonCommand;
            arguments = "main.py";

            if (!Directory.Exists(workingDir))
            {
                Debug.LogError($"[Python] ไม่พบโฟลเดอร์ dev path: {workingDir}\nกรุณาแก้ตัวแปร devPythonFolderPath ใน Inspector");
                return;
            }
        }
        else
        {
            // ----- โหมด Build จริง -----
            string appRoot = Path.GetDirectoryName(Path.GetDirectoryName(Application.dataPath));
            // Application.dataPath ตอน build คือ .../GameName_Data
            // ถอยขึ้นมา 1 ชั้นให้ถึงโฟลเดอร์ที่มีตัว .exe เกม แล้วไปหา PythonApp ที่วางข้างๆ
            string exeFolder = Path.GetDirectoryName(Application.dataPath);
            workingDir = Path.Combine(exeFolder, buildPythonFolderName);
            pythonExePath = Path.Combine(workingDir, "env", "Scripts", "python.exe");
            arguments = "\"main.py\"";

            if (!File.Exists(pythonExePath))
            {
                Debug.LogError($"[Python] ไม่พบ python.exe ที่: {pythonExePath}\nตรวจสอบว่าติดตั้ง Python venv ไว้ถูกตำแหน่งหรือยัง");
                return;
            }
        }

        var psi = new ProcessStartInfo
        {
            FileName = pythonExePath,
            Arguments = arguments,
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        try
        {
            pythonProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };

            if (logPythonOutput)
            {
                pythonProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.Log($"[Python] {e.Data}");
                };
                pythonProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.LogWarning($"[Python-ERR] {e.Data}");
                };
            }

            pythonProcess.Exited += (s, e) =>
            {
                isPythonRunning = false;
                Debug.LogWarning("[Python] Process ปิดตัวเอง (ไม่ได้สั่งปิดจาก Unity)");
            };

            pythonProcess.Start();

            if (logPythonOutput)
            {
                pythonProcess.BeginOutputReadLine();
                pythonProcess.BeginErrorReadLine();
            }

            isPythonRunning = true;
            Debug.Log($"[Python] เปิดสำเร็จ | Working Dir: {workingDir}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Python] เปิดไม่สำเร็จ: {ex.Message}");
            isPythonRunning = false;
        }
    }

    /// <summary>
    /// สั่งปิด process ของ Python
    /// </summary>
    public void StopPython()
    {
        if (pythonProcess == null || pythonProcess.HasExited)
        {
            isPythonRunning = false;
            return;
        }

        try
        {
            pythonProcess.Kill();
            pythonProcess.WaitForExit(3000);
            Debug.Log("[Python] ปิด process สำเร็จ");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Python] ปิด process ไม่สำเร็จ: {ex.Message}");
        }
        finally
        {
            isPythonRunning = false;
            pythonProcess?.Dispose();
            pythonProcess = null;
        }
    }

    /// <summary>
    /// เผื่อกรณี Python process ค้าง/ไม่ตอบสนอง สั่งปิดแล้วเปิดใหม่
    /// </summary>
    public void RestartPython()
    {
        StopPython();
        StartPython();
    }

    void OnApplicationQuit()
    {
        StopPython();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            StopPython();
        }
    }
}