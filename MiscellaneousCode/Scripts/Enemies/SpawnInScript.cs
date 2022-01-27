using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInScript : MonoBehaviour
{
    [System.Serializable]
    public class EnemyWorth
    {        
        public int weight;
        public GameObject self;

        public EnemyWorth(int weight, GameObject self)
        {            
            this.weight = weight;
            this.self = self;            
        }
    }

    
    private GameObject player;

    public GameObject[] monsters;

    public EnemyWorth[] enemies = new EnemyWorth[6];

    public int maxAreaWorth;

    public float dimX;
    public float dimY;
    public float dimZ;

    public List<EnemyWorth> spawnedInEnemies = new List<EnemyWorth>();

    [SerializeField]
    private int areaWorth;

    private int rand;

    private float randX;
    private float randY;
    private float randZ;

    private GameObject tempEnem;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < monsters.Length; i++)
        {
            enemies[i] = new EnemyWorth(monsters[i].GetComponent<EnemyBaseScript>().worth, monsters[i]);
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (areaWorth < maxAreaWorth)
        {
            rand = Random.Range(0, 3);
            randX = Random.Range(-dimX, dimX);
            randY = Random.Range(-dimY, dimY);
            randZ = Random.Range(-dimZ, dimZ);

            tempEnem = Instantiate(enemies[rand].self, new Vector3(this.transform.position.x - randX, this.transform.position.y - randY, this.transform.position.z - randZ), Quaternion.identity);

            spawnedInEnemies.Add( new EnemyWorth(enemies[rand].weight, tempEnem));
            areaWorth += enemies[rand].weight;
        }

        foreach (EnemyWorth enem in spawnedInEnemies)
        {
            if (enem == null)
            {
                areaWorth -= enem.weight;
            }
        }
    }


    private void OnTriggerEnter(Collider col)
    {
        foreach (EnemyWorth enem in enemies)
        {
            if (col.gameObject == enem.self)
            {
                areaWorth += enem.weight;
            }
        }
        if (col.gameObject == player)
        {
            areaWorth += 150;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        foreach (EnemyWorth enem in enemies)
        {
            if (col.gameObject == enem.self)
            {
                areaWorth -= enem.weight;
            }
        }
        if (col.gameObject == player)
        {
            areaWorth -= 150;
        }
    }
}
