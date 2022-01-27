using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickUps : MonoBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            Destroy(this.gameObject);
        }

    }
}
