using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public GameObject item;
    public float damage;

    public Weapon(GameObject objectGameObject, float damageToDeal)
    {
        item = objectGameObject;
        damage = damageToDeal;
    }
}

public class WeaponBaseScript : MonoBehaviour
{
    public GameObject self;
    public float damage;
    private Weapon weapon;


    // Start is called before the first frame update
    void Start()
    {
        weapon = new Weapon(self, damage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
