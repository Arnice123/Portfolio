using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamage : MonoBehaviour
{    

    public static float TakeDmg(float h, Weapon AttackingItem)
    {
        h = h - AttackingItem.damage;
        return h;
    }
}
