using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private GameObject OwningPlayer;
    private GameObject[] HealthPoints = new GameObject[3];

    void Start()
    {
        HealthManager hm = OwningPlayer.GetComponent<HealthManager>(); 
        if (hm != null)
        {
            hm.OnHealthChanged += HandleHealthChaged;
        }
        HealthPoints[0] = transform.Find("HealthBase/Health/HP1").gameObject;
        HealthPoints[1] = transform.Find("HealthBase/Health/HP2").gameObject;
        HealthPoints[2] = transform.Find("HealthBase/Health/HP3").gameObject;

    }

    private void HandleHealthChaged(int health)
    {
        if (health > HealthPoints.Length || health < 0) return;
        for (int i = 0; i < health; i++)
            HealthPoints[i].SetActive(true);

        if (health < HealthPoints.Length)
            HealthPoints[health].SetActive(false);
    }

    public void ResetHealth()
    {
        foreach(var hp in HealthPoints)
        {
            hp.SetActive(true);
        }
    }
}
