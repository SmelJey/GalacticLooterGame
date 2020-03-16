using System;
using System.IO;
using UnityEngine;

public static class GameLogger {
    public static bool isActive = false;
    private static StreamWriter writer;

    public static void LoggerInit() {
        writer = new StreamWriter("LastLog.txt");
        writer.WriteLine($"[MSG][{DateTime.Now.ToString()}][GameLogger]: Logger initialized");

        Application.logMessageReceived += HandleLog;
        isActive = true;
    }

    public static void LogMessage(string message, string prefix = "") {
        if (!isActive) {
            return;
        }

        if (prefix == string.Empty) {
            prefix = GameManager.instance == null ? "Null Game Manager" : GameManager.instance.gameState.ToString();
        }

        writer.WriteLine($"[MSG][{DateTime.Now.ToString()}][{prefix}]: {message}");
    }

    public static void LogError(string message, string prefix = "") {
        if (!isActive) {
            return;
        }

        if (prefix == string.Empty) {
            prefix = GameManager.instance == null ? "Null Game Manager" : GameManager.instance.gameState.ToString();
        }

        writer.WriteLine($"[ERR][{DateTime.Now.ToString()}][{prefix}]: {message}");
    }

    public static void LogException(string message, string prefix = "") {
        if (prefix == string.Empty) {
            prefix = GameManager.instance == null ? "Null Game Manager" : GameManager.instance.gameState.ToString();
        }

        writer.WriteLine($"[EXCPT][{DateTime.Now.ToString()}][{prefix}]: {message}");
    }

    public static void LogDebug(string message, string prefix = "") {
        if (!isActive) {
            return;
        }

        if (prefix == string.Empty) {
            prefix = GameManager.instance == null ? "Null Game Manager" : GameManager.instance.gameState.ToString();
        }

        writer.WriteLine($"[DBG][{DateTime.Now.ToString()}][{prefix}]: {message}");
    }

    public static void LoggerClose() {
        if (!isActive) {
            return;
        }

        writer.WriteLine($"[MSG][{DateTime.Now.ToString()}][GameLogger]: Logger closed");
        writer.Close();
        isActive = false;
    }

    private static void HandleLog(string logString, string stackTrace, LogType type) {
        if (!isActive) {
            return;
        }

        switch (type) {
            case LogType.Error: {
                    LogError(logString);
                    LogError(stackTrace, "StackTrace");
                    break;
                }
            case LogType.Exception: {
                    LogError(logString);
                    LogError(stackTrace, "StackTrace");
                    break;
                }
            default: {
                    LogMessage(logString);
                    break;
                }
        }
    }
}

