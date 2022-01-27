using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour {

    public static PlayerStats playerStatsInstance;

    // References
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject recentlyLostHealthBar;

    [SerializeField] private GameObject staminaBar;
    [SerializeField] private GameObject recentlyLostStaminaBar;

    private Rigidbody rigidbodyReference;


    // Statistic Bar Variables
    private float startingStatBarXScale;

    private bool healthAnimationRunning = false;
    private bool staminaAnimationRunning = false;
    private bool sprintingAnimationRunning = false;

    private float recentlyLostHealthXScale;
    private float recentlyUsedStaminaXScale;

    [SerializeField] private float delayBeforeAnimation = 0.25f;
    [SerializeField] private float sameTimeNoMatterAmountLostAnimationTime = 0.75f;
    [SerializeField] private float fasterIfLessLostAnimationTime = 2.0f;

    // Health
    private float timeSinceLastTookDamage = 0;
    private bool healing = false;

    private int maxHealth = 100;
    public int MaxHealth { 
        get {
            return maxHealth;
        }
        set {
            if (value > 0) {
                maxHealth = value;
                UpdateHealthBar();
            }
        }
    }

    private int health = 100;
    public int Health { 
        get {
            return health;
        }
        set {
            if (value > health) {
                if (value <= maxHealth) {
                    AddHealth(value - health);
                }
                else AddHealth(maxHealth - health);
            }
            if (value < health) {
                RemoveHealth(health - value);
            }
        }
    }

    private float secondsPerHealthToHeal = 3;
    public float SecondsPerHealthToHeal {
        get {
            return secondsPerHealthToHeal;
        }
        set {
            if (value >= 0) {
                secondsPerHealthToHeal = value;
            }
        }
    }

    private float healingHealthCooldownLength = 20;
    public float HealingHealthCooldownLength {
        get {
            return healingHealthCooldownLength;
        }
        set {
            if (value >= 0) {
                healingHealthCooldownLength = value;
            }
        }
    }

    // Stamina & Sprinting
    private float timeSinceLastUsedStamina = 0;

    private float maxStamina = 100;
    public float MaxStamina {
        get {
            return maxStamina;
        }
        set {
            if (value > 0) {
                maxStamina = value;
                UpdateStaminaBar();
            }
        }
    }

    private float stamina = 100;
    public float Stamina { 
        get {
            return stamina;
        }
        set {
            if (value > stamina) {
                AddStamina(value - stamina);
            }
            if (value < stamina) {
                RemoveStamina(stamina - value);
            }
        }
    }

    private float minimumStamina = 20;
    public float MinimumStamina {
        get {
            return minimumStamina;
        }
        set {
            if (value >= 0) {
                minimumStamina = value;
            }
        }
    }


    private float staminaUsedPerSecondSprinting = 20;
    public float StaminaUsedPerSecond {
        get {
            return staminaUsedPerSecondSprinting;
        }
        set {
            if (value >= 0) {
                staminaUsedPerSecondSprinting = value;
            }
        }
    }

    private float staminaGainedPerSecond = 15;
    public float StaminaGainedPerSecond {
        get {
            return staminaGainedPerSecond;
        }
        set {
            if (value >= 0) {
                staminaGainedPerSecond = value;
            }
        }
    }

    private float regeneratingStaminaCooldownLength = 2;
    public float RegeneratingStaminaCooldownLength {
        get {
            return regeneratingStaminaCooldownLength;
        }
        set {
            if (value >= 0) {
                regeneratingStaminaCooldownLength = value;
            }
        }
    }

    private bool sprinting;
    public bool Sprinting {
        get {
            return sprinting;
        }

        set {
            if (sprinting != value) {
                if (value && stamina >= minimumStamina) {
                    sprinting = true;
                    PlayerMovement.playerMovementInstance.Sprinting = true;
                    StartCoroutine(DelayedStartSprintingAnimation());
                    timeSinceLastUsedStamina = 0;
                }
                if (!value) {
                    sprinting = false;
                    PlayerMovement.playerMovementInstance.Sprinting = false;
                }
            }
        }
    }

    // Defence
    private float staminaUsedPerSecondBlocking = 20;
    public int Defence { get; set; }

    private bool blocking = false;
    public bool Blocking {
        get {
            return blocking;
        }
        set {
            if (value && stamina >= minimumStamina) {
                blocking = true;
                PlayerMovement.playerMovementInstance.Blocking = true;
            }
            if (!value) {
                blocking = false;
                PlayerMovement.playerMovementInstance.Blocking = false;
            }
        }
    }
    
    private int maxDefence = 75;
    public int MaxDefence {
        get {
            return maxDefence;
        }
    }


    // Settings
    public enum AnimationType {
        sameTimeNoMatterAmountLost,
        fasterIfLessLost
    }

    public AnimationType StatBarAnimationType { get; set; }

    // Awake is called before update
    private void Awake() {
        playerStatsInstance = this;

        if (!TryGetComponent<Rigidbody>(out rigidbodyReference)) Debug.LogError("The player has no rigid body, or it was not possible to find");

        startingStatBarXScale = healthBar.transform.localScale.x;
        if (startingStatBarXScale != staminaBar.transform.localScale.x) {
            Debug.LogError("The stamina bar and health bar did not start at the same scale");
        }
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        AnimateHealthBar();
        AnimateStaminaBar();

        RemoveStaminaWhenSprinting();
        RemoveStaminaWhenBlocking();

        RegenerateHealth();
        RegenerateStamina();
    }


    // UpdateHealthBar can be called in order to make the health displayed on the health bar match the current health
    private void UpdateHealthBar() {
        if (health > 0) {
            healthBar.transform.localScale = new Vector3(((float)health / maxHealth) * startingStatBarXScale, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
            return;
        }
        else {
            healthBar.transform.localScale = new Vector3(0, healthBar.transform.localScale.y, transform.localScale.z);

            if (health < 0) {
                health = 0;
                Debug.LogWarning("Health was less than 0");
            }
        }
    }

    // UpdateStaminaBar can be called in order to make the stamina displayed on the stamina bar match the current stamina
    private void UpdateStaminaBar() {
        if (Stamina > 0) {
            staminaBar.transform.localScale = new Vector3(((float)Stamina / maxStamina) * startingStatBarXScale, staminaBar.transform.localScale.y, staminaBar.transform.localScale.z);
        }
        else {
            staminaBar.transform.localScale = new Vector3(0, staminaBar.transform.localScale.y, staminaBar.transform.localScale.z);

            if (Stamina < 0) {
                Stamina = 0;
                Debug.LogWarning("Stamina was less than 0");
            }
        }
    }

    // CaluclateDamage is called whenever the player takes damamge in order to account for their defence
    private int CalculateDamage(int damage) {
        if (Defence < 0) {
            Debug.LogError("Defence was negative (" + Defence + ")");
            return damage;
        }

        if (Defence > MaxDefence) {
            Debug.LogError("Defence was larger than the maximum defence (defence: " + Defence + ", maxDefence: " + MaxDefence);
            return Convert.ToInt32(damage * (100.0f - MaxDefence) / 100.0f);
        }
        else return Convert.ToInt32(damage * (100.0f - Defence) / 100.0f);
    }

    private void PlayDeathAnimation() {
        rigidbodyReference.constraints &= RigidbodyConstraints.FreezeRotationY;

        rigidbodyReference.AddTorque(Vector3.right * 100);
    }

    // RemoveHealth can be called with an amount of damage in order to remove the provided amount of health, update the health bar, and start the health bar animation
    private void RemoveHealth(int damage) {

        damage = CalculateDamage(damage);
        health -= damage;

        if (health <= 0) {
            health = 0;
            PlayDeathAnimation();
        }
        else timeSinceLastTookDamage = 0;

        UpdateHealthBar();

        if (!healthAnimationRunning) {
            StartCoroutine(DelayedStartHealthBarAnimation());
        }
        else {
            recentlyLostHealthXScale = recentlyLostHealthBar.transform.localScale.x;
            healthAnimationRunning = true;
        }
    }

    // AddHealth can be called with an amount of health in order to add the provided amount of health, and update the health bar and recently lost health bar
    private void AddHealth(int healthToAdd) {
        if (healthToAdd > 0) {
            if (health + healthToAdd > maxHealth) {
                health = maxHealth;
            }
            else health += healthToAdd;

            UpdateHealthBar();

            if (recentlyLostHealthBar.transform.localScale.x < healthBar.transform.localScale.x) {
                recentlyLostHealthBar.transform.localScale = new Vector3(healthBar.transform.localScale.x, recentlyLostHealthBar.transform.localScale.y, recentlyLostHealthBar.transform.localScale.z);
            }
        }
        else if (healthToAdd == 0) {
            Debug.LogWarning("The AddHealth function was called with a value of 0");
        }
        else Debug.LogWarning("The AddHealth function was called with a negative value. If the intention was to damage the player, use the RemoveHealth function");
    }

    // RemoveStamina can be called with an amount of stamina in order to remove the provided amount of stamina, update the stamina bar, and start the stamina bar animations
    private void RemoveStamina(float staminaLost) {
        if (staminaLost > 0) {
            if (stamina - staminaLost > 0) {
                stamina -= staminaLost;
            }
            else {
                stamina = 0;
                // Player Is Out of Stamina
            }
            UpdateStaminaBar();

            timeSinceLastUsedStamina = 0;

            if (!staminaAnimationRunning) {
                StartCoroutine(DelayedStartStaminaBarAnimation());
            }
            else {
                recentlyUsedStaminaXScale = recentlyLostStaminaBar.transform.localScale.x;
                staminaAnimationRunning = true;
            }
        }
        else if (staminaLost == 0) {
            Debug.LogWarning("The RemoveStamina function was called with a value of 0");
        }
        else Debug.LogWarning("The RemoveStamina function was called a negative value. If the intention was to add stamina, use the AddStamina function instead");
    }

    // AddStamina can be called with an amount of stamina in order to add the provided amount of stamina and update the stamina bar and recently lost stamina bar
    private void AddStamina(float staminaGained) {
        if (staminaGained > 0) {
            if (stamina + staminaGained < maxStamina) {
                stamina += staminaGained;
            }
            else {
                stamina = maxStamina;
            }
            UpdateStaminaBar();

            if (recentlyLostStaminaBar.transform.localScale.x < staminaBar.transform.localScale.x) {
                recentlyLostStaminaBar.transform.localScale = new Vector3(staminaBar.transform.localScale.x, recentlyLostStaminaBar.transform.localScale.y, recentlyLostStaminaBar.transform.localScale.z);
            }
        }
        else if (staminaGained == 0) {
            Debug.LogWarning("The AddStamina function was called with a value of 0");
        }
        else Debug.LogWarning("The AddStamina function was called with a negative value. If the intention was to remove stamina, use the RemoveStamina function instead");
    }

    // AnimateHealthBar is called on Update in order to animate the recently lost health bar
    private void AnimateHealthBar() {
        if (!healthAnimationRunning) return;

        if (StatBarAnimationType == AnimationType.sameTimeNoMatterAmountLost) {
            recentlyLostHealthBar.transform.localScale -= Vector3.right * ((recentlyLostHealthXScale - healthBar.transform.localScale.x) * Time.deltaTime / sameTimeNoMatterAmountLostAnimationTime);
        }
        if (StatBarAnimationType == AnimationType.fasterIfLessLost) {
            recentlyLostHealthBar.transform.localScale -= Vector3.right * startingStatBarXScale * Time.deltaTime / fasterIfLessLostAnimationTime;
        }

        // If the animation should be over, make the recently lost health bar the exact same size as the health bar and stop the animation
        if (recentlyLostHealthBar.transform.localScale.x <= healthBar.transform.localScale.x) {
            recentlyLostHealthBar.transform.localScale = new Vector3(healthBar.transform.localScale.x, recentlyLostHealthBar.transform.localScale.y, recentlyLostHealthBar.transform.localScale.z);
            healthAnimationRunning = false;
        }
    }

    // AnimateStaminaBar is called on Update in order to update the recently lost stamina bar
    private void AnimateStaminaBar() {
        if (sprintingAnimationRunning) {
            recentlyLostStaminaBar.transform.localScale -= Vector3.right * startingStatBarXScale * staminaUsedPerSecondSprinting * Time.deltaTime / maxStamina;

            // If the animation should be over, make the recently lost health bar the exact same size as the stamina bar and stop the animation
            if (recentlyLostStaminaBar.transform.localScale.x <= staminaBar.transform.localScale.x) {
                recentlyLostStaminaBar.transform.localScale = new Vector3(staminaBar.transform.localScale.x, recentlyLostStaminaBar.transform.localScale.y, recentlyLostStaminaBar.transform.localScale.z);
                sprintingAnimationRunning = false;
            }
            return;
        }

        if (Sprinting) return;
        if (!staminaAnimationRunning) return;

        if (StatBarAnimationType == AnimationType.sameTimeNoMatterAmountLost) {
            recentlyLostStaminaBar.transform.localScale -= Vector3.right * ((recentlyUsedStaminaXScale - staminaBar.transform.localScale.x) * Time.deltaTime / sameTimeNoMatterAmountLostAnimationTime);
        }
        if (StatBarAnimationType == AnimationType.fasterIfLessLost) {
            recentlyLostStaminaBar.transform.localScale -= Vector3.right * startingStatBarXScale * Time.deltaTime / fasterIfLessLostAnimationTime;
        }

        // If the animation should be over, make the recently lost health bar the exact same size as the stamina bar and stop the animation
        if (recentlyLostStaminaBar.transform.localScale.x <= staminaBar.transform.localScale.x) {
            recentlyLostStaminaBar.transform.localScale = new Vector3(staminaBar.transform.localScale.x, recentlyLostStaminaBar.transform.localScale.y, recentlyLostStaminaBar.transform.localScale.z);
            staminaAnimationRunning = false;
        }
    }

    // RemoveStaminaWhenSprinting is called on Update in order to remove stamina if the player is sprinting
    private void RemoveStaminaWhenSprinting() {
        if (!Sprinting) return;

        Stamina -= staminaUsedPerSecondSprinting * Time.deltaTime;
        if (Stamina <= 0) {
            Sprinting = false;
        }
    }

    // RemoveStaminaWhenBlocking is called on Update in order to remove stamina if the player is blocking
    private void RemoveStaminaWhenBlocking() {
        if (!Blocking) return;

        Stamina -= staminaUsedPerSecondBlocking * Time.deltaTime;
        if (Stamina <= 0) {
            Blocking = false;
        }
    }

    // RegenerateHealthPeriodically is started a single time when the player starts healing and stops itself once the player takes damage or reaches full health
    private IEnumerator RegenerateHealthPeriodically() {
        yield return new WaitForSeconds(SecondsPerHealthToHeal);

        if (health >= maxHealth || timeSinceLastTookDamage < healingHealthCooldownLength) {
            healing = false;
        }
        else if (healing) {
            AddHealth(1);
            StartCoroutine(RegenerateHealthPeriodically());
        }
    }

    // RegenerateHealth is called on Update in order to heal the player
    private void RegenerateHealth() {
        if (health >= maxHealth) return;
        if (healing) return;

        if (timeSinceLastTookDamage >= healingHealthCooldownLength) {
            AddHealth(1);
            StartCoroutine(RegenerateHealthPeriodically());
            healing = true;
        }
        else {
            timeSinceLastTookDamage += Time.deltaTime;
        }
    }

    // RegenerateStamina is called on Update in order to regenerate stamina
    private void RegenerateStamina() {
        if (Stamina >= maxStamina) return;
        if (Sprinting) return;

        if (timeSinceLastUsedStamina >= regeneratingStaminaCooldownLength) {
            AddStamina(Time.deltaTime * staminaGainedPerSecond);
        }
        else {
            timeSinceLastUsedStamina += Time.deltaTime;
        }
    }

    // DelayedStartHealthBarAnimation is called whenever the player takes damage and starts the health bar animation shortly after the player takes damage
    private IEnumerator DelayedStartHealthBarAnimation() {
        yield return new WaitForSeconds(delayBeforeAnimation);

        recentlyLostHealthXScale = recentlyLostHealthBar.transform.localScale.x;
        healthAnimationRunning = true;
    }

    // DelayedStartStaminaBarAnimation is called whenever the player loses stamina and starts the stamina bar animation shortly after the player loses stamina
    private IEnumerator DelayedStartStaminaBarAnimation() {
        yield return new WaitForSeconds(delayBeforeAnimation);

        recentlyUsedStaminaXScale = recentlyLostStaminaBar.transform.localScale.x;
        staminaAnimationRunning = true;
    }

    // DelayedStartSprintingAnimation is called whenever the player starts sprinting and starts the sprinting stamina bar animation shortly after the player starts sprinting
    private IEnumerator DelayedStartSprintingAnimation() {
        yield return new WaitForSeconds(delayBeforeAnimation);

        sprintingAnimationRunning = true;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Death")) {
            Health = 0;
        }
    }
}