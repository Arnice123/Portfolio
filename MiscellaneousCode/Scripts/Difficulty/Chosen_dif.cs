using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Chosen_dif : MonoBehaviour
{
    
    public static float SDif = 1.0f;

    private void Update()
    {
        if (EnemyBaseScript.SelectedDifficulty != 0)
        {
            SDif = EnemyBaseScript.SelectedDifficulty;
        }
    }
}
