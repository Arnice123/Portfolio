using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;


public class TimeToFight : MonoBehaviour
{

    // the different states

    public enum State
    {
        Chase,
        Attack,
        Idle,
        Wander
    }

    // default state is idle

    public static State state = State.Idle;

    // the chosen destination for the wander state

    public Vector3 chosenDest;

    // helping pathfinding

    public GameObject invis;
    private GameObject checkedheight;
    
    // raycasts for helping them see if the possition is on the ground

    RaycastHit hit;
    Ray downRay;
    Ray upRay;
    
    // instance of the script

    public TimeToFight instance;

    private void Awake()
    {
        instance = this;
    }

    // creating a special alk object

    Alk alk;

    private void Start()
    {
        // if this object is an alk then iniztialize it

        if (gameObject.GetComponent<Alk>() != null)
        {
            alk = gameObject.GetComponent<Alk>();
        }
    }

    public void ChoseChasingWhatStateToAttackPlayer(NavMeshAgent agent, GameObject playerPos, GameObject Gself, Animator anim, bool MultiAttack, float inRange, float inView, float AttackRange, bool HasSeenPLayer)
    {
        // passing in all of the required variables

        switch (state)
        {
            case State.Chase:
                ChasePlayer(playerPos, Gself, anim, agent, AttackRange, inRange, inView, HasSeenPLayer);
                break;
            case State.Attack:
                AttackPlayer(playerPos, anim, agent, Gself, MultiAttack, inRange, inView, AttackRange, HasSeenPLayer);
                break;
            case State.Idle:
                IdlePlayer(playerPos, Gself, inRange, inView, HasSeenPLayer);
                break;
            case State.Wander:
                WanderPlayer(playerPos, anim, agent, Gself, HasSeenPLayer);
                break;
        }        
    }

    // the distance differance between player and enemy

    private float distDif;

    // random Attacks

    public int random = 0; 
    
    public void AttackPlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, bool MultiAttack, float inRange, float inView, float AttackRange, bool HasSeenPLayer)
    {
        // if multiattack is true and it isnt playing any animations

        if (MultiAttack && !AnimatorIsPlaying("Attack", anim) && !AnimatorIsPlaying("Attack2", anim) && !AnimatorIsPlaying("Attack3", anim) && !AnimatorIsPlaying("Block", anim))
        {          
            // the multiAttackfunction

            MultAttack(playerPos, anim, agent, self, inRange, inView);                    
        }
        else if (!MultiAttack && !AnimatorIsPlaying("Attack", anim))
        {
            // if it isnt playing the attack animation and only has one attack then play the attack animation

            anim.SetBool("Attack2", false);
            anim.SetBool("Attack3", false);
            StartCoroutine(PlayAnim(anim, "Attack"));
        }

        // if the player is not in attack range of the player
        
        if (!PlayerPos.FindPlayerPos(AttackRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            anim.SetBool("Attack", false);
            anim.SetBool("Attack2", false);
            anim.SetBool("Attack3", false);
            state = State.Chase;
        }
    }

    public void MultAttack(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, float inRange, float inView)
    {     
        // get the random number 1-4

        if (random == 0) random = Random.Range(1, 5);

        // if random is equal to 1 then play the first animation

        if (random == 1)
        {            
            // play the animation and wait for it to be over before replaying it

            StartCoroutine(PlayAnim(anim, "Attack"));

            // reset the random variable

            random = 0;
                        
            return;
        }

        if (random == 2)
        {
            // making sure that the animator has the animaton
            if (HasParameter("Attack2", anim))
            {                
                StartCoroutine(PlayAnim(anim, "Attack2"));
                random = 0;
                                
                return;
            }

            // if it doesnt  play the normal animation
            
            StartCoroutine(PlayAnim(anim, "Attack"));
            random = 0;

            return;
        }

        if (random == 3)
        {            
            // play a special partical effect if this is the 3 anim and the alk

            if (alk != null)
            {                  
                alk.particle.gameObject.SetActive(true);
                alk.particle.Play();                
            }
                        
            StartCoroutine(AlkAnim(anim, "Attack3"));
            random = 0;
           
            return;
        }

        if (random == 4)
        {            
            // play the blocking animation

            BlockPlayer(anim);
            random = 0;
            
            return;
        }

        
        StartCoroutine(PlayAnim(anim, "Attack"));
        random = 0;
        
        return;

    }

    IEnumerator AlkAnim(Animator anim, string booleanName)
    {
        // start anim

        anim.SetBool(booleanName, true);

        // wait for it to be over

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + anim.GetCurrentAnimatorStateInfo(0).normalizedTime);

        // set it false

        anim.SetBool(booleanName, false);

        // if this is the alk stop the particles and reset random

        if (alk != null)
        {
            alk.particle.Stop();
            alk.particle.gameObject.SetActive(false);
            random = 0;
        }
    }

    // returns if the animator is playing
    bool AnimatorIsPlaying(Animator anim)
    {
        return anim.GetCurrentAnimatorStateInfo(0).length >
               anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    // returns if the animator is playing

    bool AnimatorIsPlaying(string stateName, Animator anim)
    {
        return AnimatorIsPlaying(anim) && anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    // checks to see if the have a certain parameter

    public static bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }    

    // chasing state

    public void ChasePlayer(GameObject playerPos, GameObject self, Animator anim, NavMeshAgent agent, float AttackRange, float inRange, float inView, bool HasSeenPLayer)
    {
        // set the destination to the players position

        agent.SetDestination(playerPos.transform.position);        

        // play the walk animation

        anim.SetBool("walk", true);

        // distance between player and enemy

        distDif = Vector3.Distance(playerPos.transform.position, self.transform.position);

        // if they get within attack range switch to attack

        if (PlayerPos.FindPlayerPos(AttackRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            anim.SetBool("walk", false);
            state = State.Attack;
        }

        // if the player gets out of range just go to idle

        if (!PlayerPos.FindPlayerPos(inRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            state = State.Idle;
        }
    }

    // blocking sequence
    public void BlockPlayer(Animator anim)
    {
        // block, transition to block attack then do an attack
        if (HasParameter("Block", anim))
            StartCoroutine(PlayAnim(anim, "Block"));

        if (HasParameter("BlockAttack", anim))
            StartCoroutine(PlayAnim(anim, "BlockAttack"));

        if (HasParameter("Attack", anim))
            StartCoroutine(PlayAnim(anim, "Attack"));

        state = State.Attack;
    }

    // to prevent function running multiple times

    private Coroutine coroutine;
    private Coroutine wand;

    // the idle state

    public void IdlePlayer(GameObject playerPos, GameObject self, float inRange, float inView, bool HasSeenPLayer)
    {
        // if the player getts in range switch to the chase state

        if (PlayerPos.FindPlayerPos(inRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            state = State.Chase;
        }

        // if this isnt running have a chance of looking around

        if (coroutine == null)
        {
            
            coroutine = StartCoroutine(LookAroundArea(self));
        }

        // if this isnt running have a chance of wandering around

        if (wand == null)
        {
            
            wand = StartCoroutine(RandomlyWander());
        }
    }

    // 1 in 25 chance of wandering

    IEnumerator RandomlyWander()
    {
        // wait 1 second the check

        yield return new WaitForSeconds(1.0f);

        // if this is true wander

        if (10 >= Random.Range(1, 250)) 
        {
            state = State.Wander;
        }

        // script can play this again
        wand = null;
    }

    IEnumerator LookAroundArea(GameObject self)
    {
        if (1 >= Random.Range(1, 1000))
        {
            // Cache the start, left, and right extremes of our rotation.
            Quaternion start = self.transform.rotation;
            Quaternion left = start * Quaternion.Euler(0, -45, 0);
            Quaternion right = start * Quaternion.Euler(0, 45, 0);

            // Yield control to the Rotate coroutine to execute
            // each turn in sequence, and resume here after each
            // invocation of Rotate finishes its work.

            yield return Rotate(self.transform, start, left, 1.0f);
            yield return Rotate(self.transform, left, right, 2.0f);
            yield return Rotate(self.transform, right, start, 1.0f);
        }
        coroutine = null;
    }

    // help prevent rerunning

    private bool coru = false;
    Coroutine cro = null;

    // wandering state

    public void WanderPlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, bool HasSeenPLayer)
    {
        // makes it so the agent can move

        agent.isStopped = false;

        // if it doesnt have a location get a destination

        if (coru == false)
        {
            coru = true;
            agent.SetDestination(GetDest(self));            
            
        }

        // set walk anim true

        anim.SetBool("walk", true);

        // if you get close to the possition go idle and finish the walk anim

        if ((Vector3.Distance(agent.transform.position, agent.destination) <= 0.5f || Vector3.Distance(agent.transform.position, agent.destination) <= -0.5f)
                || (Mathf.Approximately(self.transform.position.x, agent.destination.x) && Mathf.Approximately(self.transform.position.z, agent.destination.z)))
        {
            state = State.Idle;
            if (cro == null)
                cro = StartCoroutine(PlayAnim(anim, "walk"));
            agent.ResetPath();
            coru = false;
        }
    }    

    IEnumerator PlayAnim(Animator anim, string booleanName) //sets chosen anim bool true, waits length of the anim, sets bool false
    {
        anim.SetBool(booleanName, true);
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        anim.SetBool(booleanName, false);
        cro = null;
    }

    // enemies in the scene

    private GameObject[] enemies;

    // closest enemy

    private GameObject closestEnemy;

    private Vector3 GetDest(GameObject self)
    {
        // finding all enemies

        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // distance difference

        float distDif = 0;
        float curDistdif = 0;

        // if there are enemies use the flocking wander

        if (enemies != null)
            chosenDest = CloseToEnemy(distDif, curDistdif, self);
        else
            chosenDest = DefaultFind(self);
               
        return chosenDest;
    }

    public Vector3 CloseToEnemy(float distDif, float curDistdif, GameObject self)
    {
        // first in the array is chosen as closest enemy

        closestEnemy = enemies[0];

        // filters through and actually gets the closest enemy

        foreach (GameObject g in enemies)
        {
            distDif = Vector3.Distance(this.transform.position, g.transform.position);
            curDistdif = Vector3.Distance(this.transform.position, closestEnemy.transform.position);
            if (distDif < curDistdif)
            {
                closestEnemy = g;
            }
        }

        // distance between closest enemy and this enemy

        curDistdif = Vector3.Distance(this.transform.position, closestEnemy.transform.position);

        // if it is further than 60 meters away use the normal wander method

        if (curDistdif >= 60)
        {
            chosenDest = DefaultFind(self);
            return chosenDest;
        }

        // angle distance between this and the closest enemy

        float AngleDif = Vector3.Angle(self.transform.position, closestEnemy.transform.position);

        // setting up angles

        Quaternion start = self.transform.rotation;
        Quaternion turnAmount = start * Quaternion.Euler(0, AngleDif, 0);

        // rotate towards enemy slowly

        Rotate(self.transform, start, turnAmount, 1.0f);

        // chosen pos is 30 meters away from the closest enemy

        chosenDest = new Vector3(closestEnemy.transform.position.x + Random.Range(-30, 30), closestEnemy.transform.position.y, closestEnemy.transform.position.z + Random.Range(-30, 30));

        // if the position is within 5m of that enemy find a new one

        while (Vector3.Distance(chosenDest, closestEnemy.transform.position) >= 5.0f && Vector3.Distance(chosenDest, closestEnemy.transform.position) <= -5)
        {
            chosenDest = new Vector3(closestEnemy.transform.position.x + Random.Range(-30, 30), closestEnemy.transform.position.y, closestEnemy.transform.position.z + Random.Range(-30, 30));
        }

        // return the chosen possition

        return chosenDest;
    }

    public Vector3 DefaultFind(GameObject self)
    {
        // chosen position is somewhere in 30 meters raidus

        chosenDest = new Vector3(self.transform.position.x + Random.Range(-30, 30), self.transform.position.y, self.transform.position.z + Random.Range(-30, 30));

        // create a invis object to check around for areas

        checkedheight = Instantiate(invis, chosenDest, Quaternion.identity);

        // these rays go up and down and if they hit that means that the chosen pos is not on the terrain and it will make better position

        downRay = new Ray(checkedheight.transform.position, -transform.up);
        upRay = new Ray(checkedheight.transform.position, transform.up);

        if (Physics.Raycast(upRay, out hit) || Physics.Raycast(downRay, out hit))
        {
            if (hit.collider != null && hit.collider == CompareTag("Terrain"))
            {
                chosenDest.y = hit.point.y;
                
            }
        }

        return chosenDest;
    }

    IEnumerator Rotate(Transform self, Quaternion from, Quaternion to, float duration)
    {

        for (float t = 0; t < 1f; t += Time.deltaTime / duration)
        {
            // Rotate to match our current progress between from and to.

            self.rotation = Quaternion.Slerp(from, to, t);

            // Wait one frame before looping again.

            yield return null;
        }

        // Ensure we finish exactly at the destination orientation.

        self.rotation = to;
    }

}
