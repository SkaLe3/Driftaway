using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class EnemyAI : MonoBehaviour
{

    [SerializeField] private Transform Target; // The player or waypoint to follow
    [SerializeField] private float StuckTimeThreshold = 2f; // Time in seconds to consider stuck
    private CarController CarController;
    private BoxCollider CarCollider;

      private float TimeStuck = 0f; // Timer to track if stuck

    private void Start()
    {
        CarController = GetComponent<CarController>();
        if (CarController == null)
        {
            Debug.LogError("CarController is missing on the Enemy!");
        }
        Target = GameObject.FindGameObjectWithTag("Player").transform;
        CarCollider = GetComponent<BoxCollider>();

        HealthManager hm = gameObject.GetComponent<HealthManager>();
        if (hm != null)
        {
            hm.OnDeath += HandleDeath;
        }
    }

    private void Update()
    {
        if (Target == null) return; 

        Vector3 targetDirection = Target.position - transform.position;
        float turnDistance = Vector3.Dot(targetDirection.normalized, transform.right);

        var crashPotential = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hit in crashPotential)
        {
            if (hit.CompareTag("Enemy") && hit != CarCollider)
            {
                float distance = Vector3.Distance(hit.transform.position, transform.position);

                turnDistance += Vector3.Dot((transform.position - hit.transform.position).normalized, transform.right);
                turnDistance = Mathf.Clamp(turnDistance, -1, 1);
                break;
            }
        }

        if (Vector3.Magnitude(GetComponent<Rigidbody>().velocity) < 5f)
        {
            TimeStuck += Time.deltaTime;
            if (TimeStuck >= StuckTimeThreshold)
            {
                float randomTurn = Random.Range(-1f, 1f);
                CarController.SetInputs(-0.5f, randomTurn);
                return;
            }
        }
        else
        {
            TimeStuck = 0f;
        }

        float acceleration = 1f;
        float steering = turnDistance;

        CarController.SetInputs(acceleration, steering);
    }

    private void HandleDeath()
    {
        Destroy(gameObject);
    }
}
