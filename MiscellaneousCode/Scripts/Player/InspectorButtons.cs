using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectorButtons : MonoBehaviour
{

    // FOR TESTING
    public bool sprintingToggle = false;

    public float amountOfHealthOrStaminaToAdd = 0;
    public bool changeHealthButton = false;
    public bool changeStaminaButton = false;

    // Update is called once per frame
    void Update()
    {
        if (sprintingToggle) {
            PlayerStats.playerStatsInstance.Sprinting = !PlayerStats.playerStatsInstance.Sprinting;
            sprintingToggle = false;
        }

        if (changeHealthButton) {
            PlayerStats.playerStatsInstance.Health += Mathf.RoundToInt(amountOfHealthOrStaminaToAdd);
            changeHealthButton = false;
        }

        if (changeStaminaButton) {
            PlayerStats.playerStatsInstance.Stamina += amountOfHealthOrStaminaToAdd;
            changeStaminaButton = false;
        }
    }
}
