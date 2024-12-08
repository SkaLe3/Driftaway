using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CarController))]
public class HealthManager : MonoBehaviour
{
    [SerializeField] private int MaxHealth = 3;

    private CarController Controller;
    private int Health;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;
    


    void Start()
    {
        Health = MaxHealth; 
        Controller  = gameObject.GetComponent<CarController>();
        Controller.OnCarCrash += HandleCrash;
    }

    public void HandleCrash()
    {
        ChangeHealth(-1);
    }

    public void ChangeHealth(int amount)
    {
        Health += amount;
        Health = Mathf.Clamp(Health, 0, MaxHealth);

        OnHealthChanged?.Invoke(Health);
        Debug.Log( gameObject.tag + "Health changed to " + Health);

        if (Health <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Revive()
    {
        Health = MaxHealth;
        OnHealthChanged?.Invoke(Health);
    }

}
