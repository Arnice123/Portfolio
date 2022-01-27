using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    public static PlayerMovement playerMovementInstance;

    // References
    private Rigidbody rigidbodyReference;
    private Animator animatorReference;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private GameObject defenceIcon;

    // Minimap Variables
    [SerializeField] private Transform minimapCamera;
    private Vector3 minimapCameraOffset;
    [SerializeField] private Transform playerIcon;
    private Vector3 playerIconEulerAngles;

    // GUIs
    [SerializeField] private GameObject teleportMenu;
    [SerializeField] private GameObject loadingScreen;

    public bool PauseMenuOpen { get; set; }
    public bool TeleportMenuOpen { get; set; }

    // Player Variables

    // Sprinting
    private bool shiftHeld = false;

    private bool sprinting = false;
    public bool Sprinting {
        get {
            return sprinting;
        }
        set {
            sprinting = value;

            if (Walking) Walking = false;
            animatorReference.SetBool("Running", value);
        }
    }

    private bool walking = false;
    private bool Walking {
        get {
            return walking;
        }
        set {
            walking = value;
            animatorReference.SetBool("Walking", value);
        }
    }

    [SerializeField] private float walkSpeed = 3; // Walk speed in m/s
    public float WalkSpeed {
        get {
            return walkSpeed;
        }
        set {
            if (value >= 0 && value <= runSpeed) {
                walkSpeed = value;
            }
        }
    }

    [SerializeField] private float runSpeed = 6; // Run speed in m/s
    public float RunSpeed {
        get {
            return runSpeed;
        }
        set {
            if (value >= 0 && value >= walkSpeed) {
                runSpeed = value;
            }
        }
    }

    // Jumping
    private Vector3 lastUpdateVelocity = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;
    [SerializeField] private float accelerationThreshold = 0.1f; // The amount of acceleration the player can have and still be able to jump
    [SerializeField] private float jumpCooldown = 0.5f;
    private float timeLastJumped = 0;
    [SerializeField] private float backwardsSpeedMultiplier = 0.75f; // The player's speed is multiplied by this amount when moving backwards
    [SerializeField] private float inAirSpeedMultiplier = 0.5f; // The player's speed is multiplied by this amount when in the air
    public bool TouchingGround { get; set; }

    private float jumpVelocity = 5;
    public float JumpVelocity {
        get {
            return jumpVelocity;
        }
        set {
            if (value >= 0) {
                jumpVelocity = value;
            }
        }
    }

    // Attacking
    [SerializeField] private bool attacking = false;
    public bool Attacking {
        get {
            return attacking;
        }
    }

    // Blocking
    private bool blocking = false;
    public bool Blocking {
        get {
            return blocking;
        }
        set {
            blocking = value;
            if (animatorReference.GetCurrentAnimatorStateInfo(0).IsName("Walking") || animatorReference.GetCurrentAnimatorStateInfo(0).IsName("Running")) StopCurrentAnimation();
            animatorReference.SetBool("Blocking", value);
        }
    }

    // Camera
    private float verticalCameraBounds = 89;

    private float mouseSensitivity = 20;
    public float MouseSensitivity {
        get {
            return mouseSensitivity;
        }
        set {
            mouseSensitivity = value;
        }
    }


    // Awake is called when the script is being loaded
    void Awake() {
        playerMovementInstance = this;

        rigidbodyReference = GetComponent<Rigidbody>();
        animatorReference = GetComponent<Animator>();

        minimapCameraOffset = minimapCamera.transform.position - playerTransform.position;
        playerIconEulerAngles = playerIcon.eulerAngles;
    }

    // Start is called before the first frame update
    private void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        OnGround();
        Jump();

        IsSprinting();
        MovePlayer();
        MoveCamera();

        Attack();
        Block();

        MoveMinimap();
    }

    // FixedUpdate is called every fixed framerate update
    private void FixedUpdate() {
        acceleration = (rigidbodyReference.velocity - lastUpdateVelocity) / Time.fixedDeltaTime;
        lastUpdateVelocity = rigidbodyReference.velocity;
    }

    // OnTriggerEnter is called when the collider enters the trigger
    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.CompareTag("PickUp")) {
            PlayerStats.playerStatsInstance.Defence += 1;
            if (defenceIcon) {
                StartCoroutine(DefenceIconAnimation(1));
            }
        }
    }

    // OnGround is called on Update in order to determine whether or not the player is on the ground
    private void OnGround() {
        if (acceleration.y > accelerationThreshold || acceleration.y < -accelerationThreshold) TouchingGround = false;
        else TouchingGround = true;
    }

    // Jump is called on Update and launches the player upwards if they are on solid ground and have not jumped recently
    private void Jump() {
        if (!TouchingGround) return;
        if (Input.GetAxisRaw("Jump") != 1) return;
        if (Time.realtimeSinceStartup < (timeLastJumped + jumpCooldown)) return;

        rigidbodyReference.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
        timeLastJumped = Time.realtimeSinceStartup;
    }

    // IsSprinting is called on Update in order to check whether or not the player is holding shift
    private void IsSprinting() {
        if (Input.GetAxisRaw("Sprint") == 1) {
            shiftHeld = true;
            return;
        }
        else {
            shiftHeld = false;
            if (Sprinting) PlayerStats.playerStatsInstance.Sprinting = false;
        }
    }

    // MovePlayer is called on Update in order to check if the player is trying to move, and if they are, to move them
    private void MovePlayer() {
        if (Input.GetAxisRaw("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0) {
            if (Sprinting) {
                PlayerStats.playerStatsInstance.Sprinting = false;
            }
            if (Walking) Walking = false;
            return;
        }

        if (!Sprinting && shiftHeld) PlayerStats.playerStatsInstance.Sprinting = true;
        if (!Sprinting && !Walking) Walking = true;

        float movementSpeed = walkSpeed;
        if (Sprinting) movementSpeed = runSpeed;
        if (!TouchingGround) movementSpeed *= inAirSpeedMultiplier;

        if (Input.GetAxisRaw("Vertical") == 1) playerTransform.Translate(movementSpeed * Time.deltaTime * Vector3.forward);
        if (Input.GetAxisRaw("Vertical") == -1) playerTransform.Translate(movementSpeed * backwardsSpeedMultiplier * Time.deltaTime * Vector3.back);

        playerTransform.Translate(Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime * Vector3.right);
    }

    // MoveCamera is called on Update in order to move the player and camera
    private void MoveCamera() {
        if (TeleportMenuOpen || PauseMenuOpen) return;

        playerTransform.Rotate(Input.GetAxisRaw("Mouse X") * MouseSensitivity * Vector3.up);

        float degreesToMove = Input.GetAxisRaw("Mouse Y") * MouseSensitivity / 2;
        if (degreesToMove == 0) return;

        if (playerCamera.eulerAngles.x - degreesToMove >= 180) {
            if (playerCamera.eulerAngles.x < (360 - verticalCameraBounds)) playerCamera.eulerAngles = new Vector3((360 - verticalCameraBounds), playerCamera.eulerAngles.y, playerCamera.eulerAngles.z);

            if (playerCamera.eulerAngles.x - degreesToMove < 360 - verticalCameraBounds) return;
        }

        if (playerCamera.eulerAngles.x - degreesToMove < 180) {
            if (playerCamera.eulerAngles.x > verticalCameraBounds) playerCamera.eulerAngles = new Vector3(verticalCameraBounds, playerCamera.eulerAngles.y, playerCamera.eulerAngles.z);

            if ((playerCamera.eulerAngles.x - degreesToMove) > verticalCameraBounds) return;
        }

        playerCamera.Rotate(degreesToMove * Vector3.left);
    }

    // Attack is called on update in order to determine if the player should be attacking
    private void Attack() {
        if (Attacking || Blocking) return;

        if (Input.GetAxisRaw("Fire1") != 0) {
            float attackAnimationIndex = Random.Range(0, 3);
            if (attackAnimationIndex == 0) StartCoroutine(PlayAttackingAnimationUntilOver("Attack2a"));
            else if (attackAnimationIndex == 1) StartCoroutine(PlayAttackingAnimationUntilOver("Attack2b"));
            else if (attackAnimationIndex == 2) StartCoroutine(PlayAttackingAnimationUntilOver("Attack2c"));

            return;
        }

        if (Input.GetAxisRaw("FireE") != 0) {
            float attackAnimationIndex = Random.Range(0, 3);
            if (attackAnimationIndex == 0) StartCoroutine(PlayAttackingAnimationUntilOver("Attack1a"));
            else if (attackAnimationIndex == 1) StartCoroutine(PlayAttackingAnimationUntilOver("Attack1b"));
            else if (attackAnimationIndex == 2) StartCoroutine(PlayAttackingAnimationUntilOver("Attack1c"));

            return;
        }

        if (Input.GetAxisRaw("FireQ") != 0) {
            float attackAnimationIndex = Random.Range(0, 3);
            if (attackAnimationIndex == 0) StartCoroutine(PlayAttackingAnimationUntilOver("Attack3a"));
            else if (attackAnimationIndex == 1) StartCoroutine(PlayAttackingAnimationUntilOver("Attack3b"));
            else if (attackAnimationIndex == 2) StartCoroutine(PlayAttackingAnimationUntilOver("Attack3c"));

            return;
        }
    }

    // Block is called on update in order to check if the player should be blocking
    private void Block() {
        if (Attacking) return;

        if (Blocking) {
            if (Input.GetAxisRaw("Fire2") == 0) PlayerStats.playerStatsInstance.Blocking = false;

            return;
        }
        else if (Input.GetAxis("Fire2") != 0) {
            PlayerStats.playerStatsInstance.Blocking = true;
        }
    }

    // MoveMinimap is called on Update in order to move the minimap camera and the player icon
    private void MoveMinimap() {
        minimapCamera.position = new Vector3(playerTransform.position.x, playerTransform.position.y + minimapCameraOffset.y, playerTransform.position.z);
        playerIcon.eulerAngles = new Vector3(playerIconEulerAngles.x, playerIconEulerAngles.y, -playerTransform.eulerAngles.y);
    }

    // StopCurrentAnimation can be called to stop the execution of the current animation
    private void StopCurrentAnimation() {
        animatorReference.Rebind();
        Walking = false;
    }

    // DefenceIconAnimation is called whenever the player picks up more armour in order to let them know that they gained more defence
    IEnumerator DefenceIconAnimation(float delay) {
        defenceIcon.SetActive(true);

        yield return new WaitForSeconds(delay);

        defenceIcon.SetActive(false);
    }

    // PlayAnimationUntilOver can be called to play an animation and then stop it once it is over
    IEnumerator PlayAttackingAnimationUntilOver(string attackAnimation) {
        if (animatorReference.GetCurrentAnimatorStateInfo(0).IsName("Walking") || animatorReference.GetCurrentAnimatorStateInfo(0).IsName("Running")) StopCurrentAnimation();
        attacking = true;
        animatorReference.SetBool(attackAnimation, true);
        yield return new WaitUntil(() => animatorReference.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation));
        animatorReference.SetBool(attackAnimation, false);
        yield return new WaitWhile(() => animatorReference.GetCurrentAnimatorStateInfo(0).IsName(attackAnimation));
        attacking = false;
    }
}