using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoostPickup : MonoBehaviour
{
    [SerializeField] private GameObject PickupEffect;
    private AudioSource PickupSound;

    private void Start()
    {
        PickupSound = GetComponent<AudioSource>();
    }

    public void PickUp()
    {
        PickupSound.PlayOneShot(PickupSound.clip);
        if (PickupEffect != null)
        {
            var part = Instantiate(PickupEffect, transform.position, Quaternion.identity);
            ParticleSystem ps = part.GetComponent<ParticleSystem>();
            Destroy(ps.gameObject, ps.main.duration);
        }


        SetObjectInvisible(true);
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 1f);
    }

    public void Expire()
    {
        Destroy(gameObject);
    }

    private void SetObjectInvisible(bool invisible)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(!invisible);
        }
    }
}
