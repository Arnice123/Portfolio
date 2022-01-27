using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_Boss : MonoBehaviour
{
    public float health;
    public float maxhealth = 500;

    public GameObject spiderling;

    private bool halfHealth = false;

    private List<GameObject> spiderlings = new List<GameObject>();
    private GameObject newSpider;

    // Start is called before the first frame update
    void Start()
    {
        health = gameObject.GetComponent<EnemyBaseScript>().health;

        StartCoroutine(SpawnIn());
    }

    // Update is called once per frame
    void Update()
    {
        health = gameObject.GetComponent<EnemyBaseScript>().health;

        if (health <= 250 && halfHealth == false)
        {
            halfHealth = true;
            SpiderlingSpawn();
            
        }
    }

    public void SpiderlingSpawn()
    {
        for (int x = 0; x <= 5; x++)
        {
            Instantiate(spiderling, this.transform.position, Quaternion.identity);
        }
    }

    public IEnumerator SpawnIn()
    {
        int i = 0;
        newSpider = Instantiate(spiderling, this.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(15f);

        foreach (GameObject s in spiderlings)
        {
            i++;
        }

        spiderlings[i+1] = newSpider;
        
        if (spiderlings[10] != null)
        {
            yield return new WaitForSeconds(60f);
            spiderlings.Clear();
        }
        StartCoroutine(SpawnIn());
    }
}
