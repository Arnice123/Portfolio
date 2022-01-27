using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyBaseScript : MonoBehaviour
{
    // creating all of the varialbes
    // all of these are going to be initialized in the inspector

    // health

    public float health;

    private float MaxHealth;

    //speed of the chosen enemy
    public float speed;

    // what distance before they attack

    public float inRange;

    // the enemies periferal vision

    public float inView;

    // the range in which the enemy can attack

    public float AttackRange;

    // how I made the enemy move

    public NavMeshAgent agent;
    
    // this will be true if there are differnet attack aniamtions

    public bool MultiAttack = false;  
    
    // for the goblin who has a sheild
    public bool isBlockingAndItHitSheild = false;      

    // is true when blocking

    public bool isBlocking = false;

    // the canvas object for the healthbar

    public GameObject HealthBarUI;

    // the actual healthbar

    public Slider slider;

    // all of the possible drops from the enemy

    public GameObject[] pickup;

    // the chance of a pickup happening

    public int dropchance;

    // how much the enemy is worth in the area (health / 10)

    public int worth;

    // to help pathfinding

    private bool hasSeenPlayer = false;

    // what the difficulty will be

    public static float SelectedDifficulty;

    // the animator component
        
    private Animator anim;    

    // the weapon that the enemy gets hit by
    
    private Weapon weaponGettingHitBy;    

    // refrencing the state script

    private TimeToFight TtF;  
    
    // refrence to the possible dragon script

    private Dragon DR;    

    private void Awake()
    {
        // getting the animator 

        anim = gameObject.GetComponent<Animator>();

        // setting the max health

        MaxHealth = health;
    }
            
    void Start()
    {             
        // if this is the dragon it will not be null

        DR = gameObject.GetComponent<Dragon>();

        // getting the navmesh

        agent = gameObject.GetComponent<NavMeshAgent>();

        // making the healthbar show up

        slider.value = CalculateHealth();

        // if there are other scenes deload them

        if (SceneManager.sceneCount == 2)
        {
            // grabbing the difficulty that was selected in the previosu scene  

            SelectedDifficulty = GameObject.FindGameObjectWithTag("ButtonController").GetComponent<Difficulty>().difficulty;

            // unloading all the other scenes

            SceneManager.UnloadSceneAsync("DifficultyChoosing");
            SceneManager.UnloadSceneAsync("Main_Menu");
        }
        else
        {
            SelectedDifficulty = Chosen_dif.SDif;
        }                        

        // changing values based off of the difficulty

        health *= SelectedDifficulty;
        speed *= SelectedDifficulty;
        inRange *= SelectedDifficulty;
        inView *= SelectedDifficulty;

        // if this is the dragon the do the special dragon state otherwise do the normal state
        
        if (gameObject.name == "Unka" && gameObject.GetComponent<Dragon>() != null)
        {
            StartCoroutine(GetDragon());
        }
        else
        {
            StartCoroutine(GetGood());
        }

        // if this is a spiderling start losing hp over time

        if (gameObject.GetComponent<Spiderling>() != null)
        {
            StartCoroutine(LoseHealth());
        }

        // fall down faster when you get spawned in the air

        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.down * 100, ForceMode.VelocityChange);
    }

    // calling the states for the enemy

    IEnumerator GetGood()
    {        
        // giving all the variables to the function

        TtF.ChoseChasingWhatStateToAttackPlayer(agent, Player_pos.player, this.gameObject, anim, MultiAttack, inRange, inView, AttackRange, isBlocking, hasSeenPlayer);

        // call it every half a second
        
        yield return new WaitForSeconds(0.5f); 
        StartCoroutine(GetGood());
    }

    // calling the states for the dragon

    IEnumerator GetDragon()
    {
        DR.ChoseState(agent, Player_pos.player, this.gameObject, anim, MultiAttack, inRange, inView, AttackRange, isBlocking, hasSeenPlayer);

        // call it every half a second

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(GetDragon());
    }    

    // variables for random dropchance

    private int rand;
    private int rand1;

    public void Update()
    {
        // if the health is less that 0

        if (health < 1)
        {
            // fixed some errors by not leting them heal bit

            health = 0;

            // random num between 1 - 100

            rand1 = Random.Range(0, 100);

            // if the random number is less than the chance of dropping it will drop something

            if (rand1 <= dropchance)
            {
                // randomly selecting witch armor drop

                rand = Random.Range(0, 1);

                // spawing it in

                Instantiate(pickup[rand], this.transform.position, Quaternion.identity);
            }          
            
            // death animation 

            StartCoroutine(JustDies());
        }

        // make the healthbar face the player

        slider.transform.LookAt(Player_pos.player.transform);

        // if the enemy is in idle and the health is less than the max heal

        if (TimeToFight.state == TimeToFight.State.Idle && health < MaxHealth)
        {            
            // +1 hp a second
            Invoke("Heal", 1.0f);
        }

       
    }

    // heals for one hp a second

    public float Heal()
    {
        health = health + 1;
        slider.value = CalculateHealth();
        return health;        
    }

    // the value of the slider

    float CalculateHealth()
    {
        return health / MaxHealth;
    }

    // the death animations

    IEnumerator JustDies()
    {
        anim.SetBool("isDead", true);
        
        yield return new WaitForSeconds(4.5f);
        Destroy(this.gameObject);
    }

    // last time the player attacked

    private float latestAttack;

    private void OnTriggerEnter(Collider col)
    {
        // if you get hit by a weapon and the player is attacking 

        if (col.gameObject.tag == "Weapon" && PlayerMovement.playerMovementInstance.Attacking)
        {
            // creating the weapon that the enemy was hit by

            weaponGettingHitBy = new Weapon(col.gameObject, col.gameObject.GetComponent<WeaponBaseScript>().damage);

            // if it is blocking and it hits sheild ignore the damage

            if (isBlockingAndItHitSheild)
            {
                return;
            }

            // adding a 1.4 second attack CD

            if (latestAttack != 0 && latestAttack + 1.4f <= Time.realtimeSinceStartup)
            {
                latestAttack = Time.realtimeSinceStartup;
                health = TakeDamage.TakeDmg(health, weaponGettingHitBy);
                slider.value = CalculateHealth();
            }            
        }

        // if you hit the death barier (fall through the map) die

        if (col.gameObject.tag == "Death")
        {
            Destroy(this.gameObject);
        }
    }

    // this to prevent to many spiderlings

    public IEnumerator LoseHealth()
    {
        health = takeDMGoverTime(health);

        yield return new WaitForSeconds(15f);
        LoseHealth();
    }

    public float takeDMGoverTime(float health)
    {
        return health - 10;
    }
}

