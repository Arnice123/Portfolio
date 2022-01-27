using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Dragon : MonoBehaviour
{
    public BoxCollider boxColliderDamage;
    public GameObject SpawnInZone;
    private bool isAttacking = false;
    public GameObject rockPrefab;
    public GameObject rock;

    private Animator anim;
    private bool oneYeet = true;
    private int i = 0;

    public enum State
    {
        Chase,
        Attack,
        Idle        
    }

    public static State state = State.Idle;

    public Dragon instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        boxColliderDamage.enabled = false;
        anim = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.GetBool("BreathAttack") && oneYeet)
        {
            oneYeet = false;
            rock = Instantiate(rockPrefab, SpawnInZone.transform.position, Quaternion.identity);
            rock.GetComponent<Rigidbody>().AddForce(transform.forward * 25);
            i++;
            Debug.Log("This runs " + i + " times");
        }
        else if (!anim.GetBool("BreathAttack"))
        {
            oneYeet = true;
        }

        if (anim.GetBool("Attack") || anim.GetBool("Attack2") || anim.GetBool("Attack3"))
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }

        if (isAttacking)
        {
            boxColliderDamage.enabled = true;
        }
        else
        {
            boxColliderDamage.enabled = false;
        }
    }

    public void ChoseState(NavMeshAgent agent, GameObject playerPos, GameObject Gself, Animator anim, bool MultiAttack, float inRange, float inView, float AttackRange, bool isBlocking, bool HasSeenPLayer)
    {

        switch (state)
        {
            case State.Chase:
                ChasePlayer(playerPos, Gself, anim, agent, AttackRange, inRange, inView, HasSeenPLayer);
                break;
            case State.Attack:
                AttackPlayer(playerPos, anim, agent, Gself, MultiAttack, inRange, inView, AttackRange, HasSeenPLayer);
                break;
            case State.Idle:
                IdlePlayer(playerPos, anim, agent, Gself, inRange, inView, HasSeenPLayer);
                break;            
        }
    }

    private float distDif;
    private int random = 0; // random Attacks

    public void AttackPlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, bool MultiAttack, float inRange, float inView, float AttackRange, bool HasSeenPLayer)
    {       

        if (!AnimatorIsPlaying(anim) && !AnimatorIsPlaying("Attack", anim) && !AnimatorIsPlaying("Attack2", anim) && !AnimatorIsPlaying("Attack3", anim) && !AnimatorIsPlaying("Block", anim) && !AnimatorIsPlaying("BreathAttack", anim))
        {
            MultAttack(playerPos, anim, agent, self, inRange, inView);
        }       

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
        if (random == 0) random = Random.Range(1, 6);

        if (random == 1)
        {
            StartCoroutine(PlayAnim(anim, "Attack"));
            random = 0;

            return;
        }

        if (random == 2)
        {
            if (HasParameter("Attack2", anim))
            {
                StartCoroutine(PlayAnim(anim, "Attack2"));
                random = 0;

                return;
            }

            StartCoroutine(PlayAnim(anim, "Attack"));
            random = 0;

            return;
        }

        if (random == 3)
        {          

            StartCoroutine(PlayAnim(anim, "Attack3"));
            random = 0;

            return;
        }

        if (random == 4)
        {
            BlockPlayer(playerPos, anim, agent, inRange, inView, self);
            random = 0;

            return;
        }

        if (random == 5)
        {
            StartCoroutine(PlayAnim(anim, "BreathAttack"));
            random = 0;
        }


        StartCoroutine(PlayAnim(anim, "Attack"));
        random = 0;

        return;

    }   

    bool AnimatorIsPlaying(Animator anim)
    {
        return anim.GetCurrentAnimatorStateInfo(0).length >
               anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }


    bool AnimatorIsPlaying(string stateName, Animator anim)
    {
        return AnimatorIsPlaying(anim) && anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    public static bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    public void ChasePlayer(GameObject playerPos, GameObject self, Animator anim, NavMeshAgent agent, float AttackRange, float inRange, float inView, bool HasSeenPLayer)
    {
        agent.SetDestination(playerPos.transform.position);
        anim.SetBool("walk", true);
        

        distDif = Vector3.Distance(playerPos.transform.position, self.transform.position);
        if (PlayerPos.FindPlayerPos(AttackRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            anim.SetBool("walk", false);
            state = State.Attack;
        }

        if (!PlayerPos.FindPlayerPos(inRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            state = State.Idle;
        }
    }


    public void BlockPlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, float inRange, float inView, GameObject self)
    {
        if (HasParameter("Block", anim))
            StartCoroutine(PlayAnim(anim, "Block"));

        if (HasParameter("BlockAttack", anim))
            StartCoroutine(PlayAnim(anim, "BlockAttack"));

        if (HasParameter("Attack", anim))
            StartCoroutine(PlayAnim(anim, "Attack"));

        state = State.Attack;
    }

    private Coroutine coroutine;
    
    public void IdlePlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, float inRange, float inView, bool HasSeenPLayer)
    {
        if (PlayerPos.FindPlayerPos(inRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            state = State.Chase;
        }

        SetAllAnimsFalse(anim);

        if (coroutine == null)
        {            
            coroutine = StartCoroutine(LookAroundArea(self));
        }
        
    }    

    private void SetAllAnimsFalse(Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            animator.SetBool(param.name, false);
        }
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

    
    
    IEnumerator PlayAnim(Animator anim, string booleanName) //sets chosen anim bool true, waits length of the anim, sets bool false
    {
        anim.SetBool(booleanName, true);
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        anim.SetBool(booleanName, false);
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
