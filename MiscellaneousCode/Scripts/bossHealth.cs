using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class bossHealth : MonoBehaviour
{
    private GameObject obyject;
    private GameObject obyjectDrag;
    
    private Slider slider;
    private Slider sliderDrag;

    private float spiderHealth;
    private float maxSpiderHealth;

    private float dragonHealth;
    private float maxDragonHealth;

    public GameObject spider;
    public GameObject dragon;

    // Start is called before the first frame update
    void Start()
    {
        spider = GameObject.Find("Spider Venom-Purple");
        spider = GameObject.Find("Dragon");
        spiderHealth = spider.gameObject.GetComponent<EnemyBaseScript>().health;
        maxSpiderHealth = spider.gameObject.GetComponent<EnemyBaseScript>().health;

        dragonHealth = dragon.gameObject.GetComponent<EnemyBaseScript>().health;
        maxDragonHealth = dragon.gameObject.GetComponent<EnemyBaseScript>().health;

        obyject = GameObject.FindGameObjectWithTag("Spider_bar");
        obyjectDrag = GameObject.FindGameObjectWithTag("Dragon_bar");        

        slider = GameObject.Find("spiderSlider").GetComponent<Slider>();
        sliderDrag = GameObject.Find("dragonSlider").GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(Player_pos.player.transform.position, spider.transform.position) <= 75)
        {
            obyject.SetActive(true);
        }
        else
        {
            obyject.SetActive(false);
        }

        if (Vector3.Distance(Player_pos.player.transform.position, dragon.transform.position) <= 250)
        {
            obyjectDrag.SetActive(true);
        }
        else
        {
            obyjectDrag.SetActive(false);
        }

        spiderHealth = spider.gameObject.GetComponent<EnemyBaseScript>().health;
        
        slider.value = spiderHealth / maxSpiderHealth;

        dragonHealth = dragon.gameObject.GetComponent<EnemyBaseScript>().health;

        sliderDrag.value = dragonHealth / maxDragonHealth;
    }
}
