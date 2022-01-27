using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscapeMenu : MonoBehaviour
{
    public Button Desktop, Resume,  Menu;

    public Text warning;

    [SerializeField]
    private GameObject Setting;
    
    public Dropdown MaxFrames, Vsync, Graphics;
    

    // Start is called before the first frame update
    void Start()
    {        
        Desktop.onClick.AddListener(Application.Quit);
        Menu.onClick.AddListener(MainScene);
        Resume.onClick.AddListener(OutOfSettings);

        
    }

    private void MainScene()
    {
        SceneManager.LoadScene("Main_Menu", LoadSceneMode.Additive);
    }

    private void OutOfSettings()
    {
        Time.timeScale = 1;
        Setting.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0 && Setting.activeInHierarchy == true)
            {
                Time.timeScale = 1;
                Setting.SetActive(false);                
            }
            else
            {
                Time.timeScale = 0;
                Setting.SetActive(true);
            }

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
                warning.text = "WARNING IF V-SYNC IS ENABLED THIS WILL BE IGNORED";
                break;
            case 1:
                QualitySettings.vSyncCount = 0;
                warning.text = " ";
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
