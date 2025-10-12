using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Utils
{
    public static void DebugEveryXFPS(string message, int fps = 60)
    {
        if (Time.frameCount % fps == 0)
        {
            Debug.Log(message);
        }
    }
    
    public static void SwitchToScene(Scene scene)
    {
        string sceneName = Enum.GetName(typeof(Scene), scene);
        SceneManager.LoadScene(sceneName);
    }
    
    public static AudioType GetAudioType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        switch (extension)
        {
            case ".mp3":
                return AudioType.MPEG;
            case ".wav":
                return AudioType.WAV;
            case ".ogg":
                return AudioType.OGGVORBIS;
            case ".aiff":
                return AudioType.AIFF;
            default:
                Debug.LogWarning("Unsupported audio type. Defaulting to WAV.");
                return AudioType.WAV;
        }
    }
}