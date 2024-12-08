using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(PlayerCarEffects))]
public class PlayerController : MonoBehaviour
{
    private CarController CarController;
    private PlayerCarEffects CarEffects;
    private Rigidbody Rb;
    private HealthManager Hm;

    private bool IsDead = false;
    private bool IsRoundStarted = false;
    private bool IsMovingToStart = false;
    private UnityEngine.Vector3 StartPosition = new UnityEngine.Vector3(0, 1, 0);

    [SerializeField] private float StoppingDistanceInMenu = 0.1f;
    [SerializeField] private float StartMovementInMenuDelay = 2f;
    [SerializeField] private float StopEngineInMenuDelay = 2f;
    [SerializeField] private float AccelerationInMenu = 0.5f;
    [SerializeField] private float DecelerationDistanceInMenu = 2f;

    public event Action OnPlayerReady;

    private void Start()
    {
        CarController = GetComponent<CarController>();
        CarEffects = GetComponent<PlayerCarEffects>();
        if (CarEffects == null)
        {
            Debug.LogError("Faild to get Car Effects");
        }
        Rb = GetComponent<Rigidbody>();
   
        Hm = gameObject.GetComponent<HealthManager>();
        if (Hm != null)
        {
            Hm.OnDeath += HandleDeath;
        }
        GameObject GM = GameObject.FindGameObjectWithTag("GameMode");
        GM.GetComponent<GameMode>().OnRoundStarted += HandleStartRound;
    }

    private void Update()
    {   
        CarController.SetInputs(0, 0);
        if (IsMovingToStart)
        {
            ProcessMovementToStart();
        }
        if (IsDead) return;
        if (!IsRoundStarted) return;
        float acceleration = Input.GetAxis("Vertical"); 
        float steering = Input.GetAxis("Horizontal");  
        CarController.SetInputs(acceleration, steering);
    }

    public void Revive()
    {
        CarEffects.RepairCar();
        GetComponent<HealthManager>().Revive();
        IsDead = false;
        StartCoroutine(DelayedMoveToStart());
    }

    private void HandleDeath()
    {
        if (IsDead) return;
        IsDead = true;
        IsRoundStarted = false;
        CarEffects.BreakCar();
        Debug.Log("Player Died");
    }
    private void HandleStartRound()
    {
        IsRoundStarted = true;
        CarEffects.StartEngine();
        Debug.Log("Round Started");
    }

    private IEnumerator DelayedMoveToStart()
    {
        CarEffects.StartEngine();
        UnityEngine.Vector3 initialPosition = StartPosition;
        initialPosition.z = -5f;
        Rb.velocity = new UnityEngine.Vector3(0, 0, 0);
        Rb.position = initialPosition;
        transform.rotation = UnityEngine.Quaternion.Euler(0, 0, 0);
        yield return new WaitForSeconds(StartMovementInMenuDelay);
        MoveToStart();
    }
    public void MoveToStart()
    {
        IsMovingToStart = true;  
    }
    private void ProcessMovementToStart()
    {
        if (Rb == null) return;

        float distanceToTarget = UnityEngine.Vector3.Distance(transform.position, StartPosition);
        if (distanceToTarget > DecelerationDistanceInMenu)
        {   
            CarController.SetInputs(AccelerationInMenu, 0);
        }
        else
        {
            if (distanceToTarget > StoppingDistanceInMenu)
            {   
                float currentSpeed = Rb.velocity.magnitude;
                float requiredAcceleration = -(currentSpeed * currentSpeed) / (2 * distanceToTarget+0.5f);
                float input = Mathf.Clamp(requiredAcceleration / CarController.AccelerationForce, -1f, 1f);
                CarController.SetInputs(input, 0);
            }
            else
            {
                CarController.SetInputs(0, 0);
                Rb.velocity = UnityEngine.Vector3.zero;
                CarEffects.TurnOffEngineDelayed(StopEngineInMenuDelay);
                OnPlayerReady?.Invoke();
                IsMovingToStart = false;
            }
        }      
    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Boost"))
        {
            if (other.gameObject.CompareTag("HealthBoost"))
            {
                other.gameObject.GetComponent<BoostPickup>().PickUp();
                Hm.ChangeHealth(1);
            }
        }
    }
}
