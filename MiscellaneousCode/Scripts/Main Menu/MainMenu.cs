using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button Play, Quit, Settings, Back;

    public GameObject Setting;
    public GameObject NotSetting;

    public Dropdown MaxFrames, Vsync, Graphics;
    

    // Start is called before the first frame update
    void Start()
    {
        Play.onClick.AddListener(NextScene);
        Quit.onClick.AddListener(Application.Quit);
        Settings.onClick.AddListener(SettingsScene);
        Back.onClick.AddListener(OutOfSettings);
    }

    private void NextScene()
    {
        SceneManager.LoadScene("DifficultyChoosing", LoadSceneMode.Additive);
    }

    private void SettingsScene()
    {        
        NotSetting.SetActive(false);     
        Setting.SetActive(true);        
    }

    private void OutOfSettings()
    {
        NotSetting.SetActive(true);
        Setting.SetActive(false);
    }

    private void Update()
    {
        if (SceneManager.sceneCount >= 2)
        {
            SceneManager.UnloadSceneAsync("DifficultyChoosing");
            SceneManager.UnloadSceneAsync("Spawn");
            SceneManager.UnloadSceneAsync("Forest");
            SceneManager.UnloadSceneAsync("Monster");
            
        }

        switch (MaxFrames.value)
        {
            case 0:
                Application.targetFrameRate = 60;
                break;
            case 1:
                Application.targetFrameRate = 15;
                break;
            case 2:
                Application.targetFrameRate = 30;
                break;
            case 3:
                Application.targetFrameRate = 45;
                break;
            case 4:
                Application.targetFrameRate = 80;
                break;
            case 5:
                Application.targetFrameRate = 100;
                break;
            case 6:
                Application.targetFrameRate = 150;
                break;
            case 7:
                Application.targetFrameRate = 240;
                break;
            case 8:
                Application.targetFrameRate = -1;
                break;
            
        }

        switch (Vsync.value)
        {
            case 0:
                QualitySettings.vSyncCount = 1;
                break;
            case 1:
                QualitySettings.vSyncCount = 0;
                break;            
        }

        switch (Graphics.value)
        {
            case 0:
                QualitySettings.SetQualityLevel(5, true);
                break;
            case 1:
                QualitySettings.SetQualityLevel(0, true);
                break;
            case 2:
                QualitySettings.SetQualityLevel(1, true);
                break;
            case 3:
                QualitySettings.SetQualityLevel(2, true);
                break;
            case 4:
                QualitySettings.SetQualityLevel(3, true);
                break;
            case 5:
                QualitySettings.SetQualityLevel(4, true);
                break;           
        }

    }
   
}
