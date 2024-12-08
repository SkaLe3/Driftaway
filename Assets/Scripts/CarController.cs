using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] public float AccelerationForce = 15f;
    [SerializeField] private float MaxSpeed = 15f;
    [SerializeField] private float MaxTraction = 1f;
    [SerializeField] private float IdleTraction = 2f;
    [SerializeField] private float TurnSpeed = 50f;
    [SerializeField] private float DriftTreshold = 0.45f;
    [SerializeField] private float ForwardTiltAmount = 5f;
    [SerializeField] private float SpringStiffness = 100f;
    [SerializeField] private float DamperCoefficient = 20f;
    [SerializeField] private float SidewaysTiltAmount = 5f;
    [SerializeField] private float MinSideSkidVelocity = 5f;



    [Header("References")] 
    [SerializeField] private Rigidbody Rb;
    [SerializeField] private AudioSource EngineSoundSource;
    [SerializeField] private AudioSource SkidClip;
    [SerializeField] private AudioClip[] ImpactSounds;
    [SerializeField] private float ImpactVolume = 1.0f;
    [SerializeField] private GameObject ImpactParticlesPrefab;
    [SerializeField] private GameObject CarBody;
    [SerializeField] private TrailRenderer[] SkidMarks = new TrailRenderer[4];
    [SerializeField] private ParticleSystem[] SkidSmokes = new ParticleSystem[4];



    public bool HasPlayedImpact { get; set; } = false; // Marks that car crashed and is responsible for impact sounds and visuals, the other car won't play impacts then
    private Vector3 Acceleration;
    private Vector3 VelocityDelta;
    private Vector3 PreviousVelocity;
    private float Traction;
    private float AccelerationInput;
    private float SteeringInput;
    private float ForwardTilt;
    private float SidewaysTilt;
    private float ForwardTiltVelocity;
    private float SidewaysTiltVelocity;
    private bool TireEffectsFlag = false;


    public event Action OnCarCrash;

    // Start is called before the first frame update
    void Start()
    {
        if (Rb == null)
            Rb = GetComponent<Rigidbody>(); 
        if (EngineSoundSource == null)
            EngineSoundSource = GetComponent<AudioSource>();

        PreviousVelocity = Rb.velocity;
    }

    public void SetInputs(float accelerationInput, float steeringInput)
    {
        AccelerationInput = Mathf.Clamp(accelerationInput, -1f, 1f);
        SteeringInput = Mathf.Clamp(steeringInput, -1f, 1f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Car"))
        {
            OnCarCrash?.Invoke();
            if (!other.gameObject.GetComponent<CarController>().HasPlayedImpact) // Don't play impact FX if other car did
            {
                AudioManager.Instance.PlaySoundOneShot(ImpactSounds[Random.Range(0, ImpactSounds.Length - 1)], ImpactVolume, Random.Range(0.6f, 1.4f));
                var part = Instantiate(ImpactParticlesPrefab, other.contacts[0].point, Quaternion.identity);
                ParticleSystem ps = part.GetComponent<ParticleSystem>();
                Destroy(ps.gameObject, ps.main.duration);
                HasPlayedImpact = true;
            }
            else
            {
               other.gameObject.GetComponent<CarController>().HasPlayedImpact = false; 
            }
        

            other.rigidbody.AddForce(Rb.velocity * 0.5f, ForceMode.Impulse);
            Rb.AddForce(-other.relativeVelocity * 0.5f, ForceMode.Impulse);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEngineSound();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        // Velocity delta for tilt
        VelocityDelta = Rb.velocity - PreviousVelocity;

        // Traction interpolation
        float t = Rb.velocity.magnitude / MaxSpeed;
        Traction = Mathf.Lerp(IdleTraction, MaxTraction, t * t);

        // Do not slide when no input
        if (AccelerationInput == 0)
            Traction = IdleTraction;
        // Accelerate
        Acceleration = AccelerationForce * AccelerationInput * transform.forward;
        Rb.AddForce(Acceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
        float projectedSpeed = Mathf.Abs(Vector3.Dot(Rb.velocity, transform.forward));
        float driftCoefficient = Vector3.Dot(Rb.velocity.normalized , transform.forward);
        float directionSign = Mathf.Sign(driftCoefficient +  AccelerationInput);
        float steerAngle = SteeringInput  * TurnSpeed;
        float driftMultiplier = 1.0f;

        // Check for drifting
        if (driftCoefficient <DriftTreshold)
        {
            driftMultiplier = Mathf.Lerp(1.5f, 1f, Mathf.Abs(driftCoefficient) / DriftTreshold); // Higher multiplier for sharper drifts
        }
        // Steer
        transform.Rotate(steerAngle  * driftMultiplier* projectedSpeed * directionSign * Vector3.up * Time.fixedDeltaTime);
        Rb.velocity = Vector3.ClampMagnitude(Rb.velocity, MaxSpeed);
        Rb.velocity = Vector3.Lerp(Rb.velocity.normalized, Vector3.Project(Rb.velocity.normalized, transform.forward).normalized,  Traction * Time.fixedDeltaTime) * Rb.velocity.magnitude;
         
        PreviousVelocity = Rb.velocity;
        
    }

    private void UpdateEngineSound()
    {   
        if (EngineSoundSource != null)
        {
            float t = Rb.velocity.magnitude / MaxSpeed;
            EngineSoundSource.pitch = 1 +  2* Mathf.Sqrt(t);
        }
    }



    private void UpdateTilt()
    {
            
        float centerOfGravityHeight = 0.1f;

        // Calculate the torque caused by acceleration
        float forwardForce = Vector3.Dot(VelocityDelta * 2f /Time.deltaTime, transform.forward) * Rb.mass;
        float tiltTorque = forwardForce * centerOfGravityHeight;

        // Convert torque to angular acceleration
        float momentOfInertia = Rb.mass * Mathf.Pow(centerOfGravityHeight, 2); 
        float angularAcceleration = tiltTorque / momentOfInertia;

        // Apply the spring-damper system
        float springForce = -SpringStiffness * ForwardTilt; 
        float dampingForce = -DamperCoefficient * ForwardTiltVelocity;
        float netAngularAcceleration = angularAcceleration + springForce + dampingForce;

        ForwardTiltVelocity += netAngularAcceleration * Time.deltaTime;
        ForwardTilt += ForwardTiltVelocity * Time.deltaTime;

        ForwardTilt = Mathf.Clamp(ForwardTilt, -ForwardTiltAmount, ForwardTiltAmount);

        // Calculate the torque caused by lateral acceleration
        float lateralForce = Vector3.Dot(VelocityDelta * 15f / Time.deltaTime, transform.right) * Rb.mass;
        float sidewaysTiltTorque = lateralForce * centerOfGravityHeight;

        // Convert torque to angular acceleration
        float sidewaysAngularAcceleration = sidewaysTiltTorque / momentOfInertia;

        // Apply the spring-damper system
        float sidewaysSpringForce = -SpringStiffness * SidewaysTilt;
        float sidewaysDampingForce = -DamperCoefficient * SidewaysTiltVelocity;
        float sidewaysNetAngularAcceleration = sidewaysAngularAcceleration + sidewaysSpringForce + sidewaysDampingForce;

        SidewaysTiltVelocity += sidewaysNetAngularAcceleration * Time.deltaTime;
        SidewaysTilt += SidewaysTiltVelocity * Time.deltaTime;

        SidewaysTilt = Mathf.Clamp(SidewaysTilt, -SidewaysTiltAmount, SidewaysTiltAmount);

        // Apply all
        Vector3 localRotation = CarBody.transform.localEulerAngles;
        localRotation.x = ForwardTilt;
        localRotation.z = SidewaysTilt;
        CarBody.transform.localEulerAngles = localRotation;
    }

    private void UpdateVisuals()
    {
        UpdateTilt();
        VFX();
    }


    private void VFX()
    {
        if ( Mathf.Abs(Vector3.Dot(Rb.velocity, transform.right)) > MinSideSkidVelocity)
            StartSkidEffects();
        else
            StopSkidEffects();
    }

    private void StartSkidEffects()
    {
        if (TireEffectsFlag) return;
        foreach (var skidMark in SkidMarks)
        {
            skidMark.emitting = true; 
        }
        
        foreach (var smoke in SkidSmokes)
        {
            smoke.Play();
        }
        if (SkidClip != null)
        {
            SkidClip.Play();
        }
        TireEffectsFlag = true;
    }

    private void StopSkidEffects()
    {
        if (!TireEffectsFlag) return;

        foreach (var skidMark in SkidMarks)
        {
            skidMark.emitting = false; 
        }
        
        foreach (var smoke in SkidSmokes)
        {
            smoke.Stop();
        }
        if (SkidClip != null)
        {
            SkidClip.Stop();
        }
        TireEffectsFlag = false;
    }

}