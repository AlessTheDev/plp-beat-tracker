using System;
using System.Collections;
using System.IO;
using BeatTracking;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Looks at the <see cref="GameInfo"/> class and prepares the audiosource
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioSourceSetup : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        if(!GameInfo.HasBeenSet) return; // Debug mode
        
        _audioSource = GetComponent<AudioSource>();
        
        // Prevent Delay
        Application.runInBackground = GameInfo.UseMicrophone;

        if (GameInfo.UseMicrophone)
        {
            SetupMicrophone();
        }
        else
        {
            LoadAudioToSource();
        }
    }

    private void LoadAudioToSource()
    {
        FileVisual selectedFile = FileExplorerManager.Instance.selectedFile;
        string filePath = selectedFile.filePath + Path.DirectorySeparatorChar + selectedFile.fileName;
        Debug.Log("Loading: " + filePath);
        StartCoroutine(LoadAudioToSource(filePath));
    }

    private IEnumerator LoadAudioToSource(string filePath)
    {
        // Detect the audio type based on the file extension
        AudioType audioType = Utils.GetAudioType(filePath);

        string formattedPath = "file://" + filePath;

        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(formattedPath, audioType);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

            _audioSource.clip = audioClip;
            _audioSource.Play();
        }
    }

    private void SetupMicrophone()
    {
        string micDevice = Microphone.devices[0];
        
        _audioSource.loop = true;
        _audioSource.mute = false;
        _audioSource.clip = Microphone.Start(micDevice, true, 2, AudioSettings.outputSampleRate);
        
        // Wait until microphone starts recording
        while (!(Microphone.GetPosition(micDevice) > 0)) { }
        
        _audioSource.Play();
    }
}
