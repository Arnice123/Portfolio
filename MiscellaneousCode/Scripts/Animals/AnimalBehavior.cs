using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AnimalBehavior : MonoBehaviour
{
    public float health;
    private float MaxHealth;
    public float speed;    
    public NavMeshAgent agent;
    private Weapon weaponGettingHitBy;

    public GameObject invis;
    private Vector3 chosenDest;
    private GameObject checkedheight;
    private GameObject playerPos;
    private Animator anim;

    RaycastHit hit;
    Ray downRay;
    Ray upRay;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Wainder());
    }

    private IEnumerator Wainder()
    {
        WanderPlayer(playerPos, anim, agent, this.gameObject);
        yield return new WaitForSeconds(10f);
        StartCoroutine(Wainder());
    }

    // Update is called once per frame
    void Update()
    {
        playerPos = Player_pos.GetPlayer();

        if (health < 0)
        {
            health = 0;
            Destroy(this.gameObject);
        }

        if (Vector3.Distance(this.gameObject.transform.position, playerPos.transform.position) <= 10)
        {
            WanderPlayer(playerPos, anim, agent, this.gameObject);
            StartCoroutine(SpeedBoost());
        }   
               
    }

    IEnumerator SpeedBoost()
    {
        speed = 8;
        yield return new WaitForSeconds(2.0f);
        speed = 5;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Weapon")
        {
            weaponGettingHitBy = new Weapon(col.gameObject, col.gameObject.GetComponent<WeaponBaseScript>().damage);
            
            health = TakeDamage.TakeDmg(health, weaponGettingHitBy);
            speed += 1;

            
        }

        if (col.gameObject.tag == "Death")
        {
            Destroy(this.gameObject);
        }
    }

    private bool coru;

    public void WanderPlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self)
    {
        agent.isStopped = false;
        if (coru == false)
        {
            coru = true;
            agent.SetDestination(GetDest(self));

        }
        
    }

    private Vector3 GetDest(GameObject self)
    { 
        chosenDest = DefaultFind(self);

        return chosenDest;
    }

   

    public Vector3 DefaultFind(GameObject self)
    {
        chosenDest = new Vector3(self.transform.position.x + Random.Range(-30, 30), self.transform.position.y, self.transform.position.z + Random.Range(-30, 30));
        checkedheight = Instantiate(invis, chosenDest, Quaternion.identity);

        downRay = new Ray(checkedheight.transform.position, -transform.up);
        upRay = new Ray(checkedheight.transform.position, transform.up);

        if (Physics.Raycast(upRay, out hit) || Physics.Raycast(downRay, out hit))
        {
            if (hit.collider != null && hit.collider == CompareTag("Terrain"))
            {
                chosenDest.y = hit.point.y;
                //chosenDest.y = hit.collider.transform.position.y;
            }
        }

        return chosenDest;
    } 
}
