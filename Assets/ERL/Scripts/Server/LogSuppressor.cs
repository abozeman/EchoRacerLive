using UnityEngine;

public class LogSuppressor : MonoBehaviour
{
    void Awake()
    {
        Application.logMessageReceived += HandleLog;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString.Contains("Unloading 3 Unused Serialized files"))
        {
            // Suppress the message by doing nothing with it.
            return;
        }

        // For all other messages, log them as usual.
        Debug.LogFormat("{0}\n{1}", logString, stackTrace);
    }
}