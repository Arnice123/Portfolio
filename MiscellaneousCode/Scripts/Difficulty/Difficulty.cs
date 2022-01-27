using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Difficulty : MonoBehaviour
{
    public float easyDif = 0.5f;
    public float normalDif = 1f;
    public float hardDif = 2f;
    public float hellDif = 500f;
    public float difficulty = 0;

    public float bob;

    public Text selectDifficultyError;

    public Button Easy, Normal, Hard, Hell, StartGame;

    void Start()
    {
        Easy.onClick.AddListener(SetDifficultyEasy);
        Normal.onClick.AddListener(SetDifficultyNormal);
        Hard.onClick.AddListener(SetDifficultyHard);
        Hell.onClick.AddListener(SetDifficultyHell);
        StartGame.onClick.AddListener(NextScene);

        if (SceneManager.sceneCount >= 2)
        {            
            SceneManager.UnloadSceneAsync("Main_Menu");
        }
    }

    

    public void SetDifficultyEasy()
    {
        difficulty = easyDif;
    }

    public void SetDifficultyNormal()
    {
        difficulty = normalDif;
    }

    public void SetDifficultyHard()
    {
        difficulty = hardDif;
    }

    public void SetDifficultyHell()
    {
        difficulty = hellDif;
    }

    public void NextScene()
    {
        if (difficulty >= 0.49f)
            SceneManager.LoadScene("Spawn", LoadSceneMode.Additive);
        else
        {
            selectDifficultyError.text = "Please select a difficulty";
        }
    }

}
