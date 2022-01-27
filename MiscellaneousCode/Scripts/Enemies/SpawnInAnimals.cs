using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInAnimals : MonoBehaviour {
    [System.Serializable]
    public class AnimalWorth {
        public int weight;
        public GameObject self;

        public AnimalWorth(int weight, GameObject self) {
            this.weight = weight;
            this.self = self;
        }
    }

    public GameObject[] animals;

    public AnimalWorth[] animalList = new AnimalWorth[3];

    public int maxAreaWorth;

    public float dimX;
    public float dimY;
    public float dimZ;


    [SerializeField]
    private int areaWorth;

    private int rand;

    private float randX;
    private float randY;
    private float randZ;

    // Start is called before the first frame update
    void Start() {
        animalList[0] = new AnimalWorth(1, animals[0]);
        animalList[1] = new AnimalWorth(1, animals[1]);
        animalList[2] = new AnimalWorth(1, animals[2]);
    }

    // Update is called once per frame
    void Update() {
        if (areaWorth < maxAreaWorth) {
            rand = Random.Range(0, 3);
            randX = Random.Range(-dimX, dimX);
            randY = Random.Range(-dimY, dimY);
            randZ = Random.Range(-dimZ, dimZ);

            Instantiate(animalList[rand].self, new Vector3(this.transform.position.x - randX, this.transform.position.y - randY, this.transform.position.z - randZ), Quaternion.identity);
            areaWorth += animalList[rand].weight;
        }
    }


    private void OnTriggerEnter(Collider col) {
        if (col.gameObject == animalList[0].self) areaWorth += animalList[0].weight;
        else if (col.gameObject == animalList[1].self) areaWorth += animalList[1].weight;
        else if (col.gameObject == animalList[2].self) areaWorth += animalList[2].weight;
    }

    private void OnTriggerExit(Collider col) {
        if (col.gameObject == animalList[0].self) areaWorth += animalList[0].weight;
        else if (col.gameObject == animalList[1].self) areaWorth += animalList[1].weight;
        else if (col.gameObject == animalList[2].self) areaWorth += animalList[2].weight;
    }
}
