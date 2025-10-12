using System;
using BeatTracking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private Scene _sceneToLoad;
    
    private void Start()
    {
        FileExplorerManager.Instance.OnFileSelected.AddListener(OnFileSelected);
        GameInfo.HasBeenSet = true;
    }

    private void OnFileSelected(FileVisual selectedFile)
    {
        GameInfo.SelectedFile = selectedFile;
        Utils.SwitchToScene(_sceneToLoad);
    }

    public void OnPLPVisualizationSelected()
    {
        SelectFileAndSwitchScene(Scene.PLPVisualization);
    }
    
    public void OnPLPMicrophoneModeSelected()
    {
        GameInfo.UseMicrophone = true;
        Utils.SwitchToScene(Scene.PLPVisualization);
    }
    
    public void OnFFTVisualizationSelected()
    {
        SelectFileAndSwitchScene(Scene.FFTAverageGraphs);
    }

    private void SelectFileAndSwitchScene(Scene scene)
    {
        GameInfo.UseMicrophone = false;
        _sceneToLoad = scene;
        FileExplorerManager.Instance.Show();
    }
}
