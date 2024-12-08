using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerCarEffects : MonoBehaviour
{
    [SerializeField] private AudioSource EngineWorkingSound;
    [SerializeField] private AudioSource EngineStartSound;
    [SerializeField] private AudioSource EngineStopSound;
    [SerializeField] private AudioSource SteamSound;
    [SerializeField] private AudioSource FireSound;
    [SerializeField] private GameObject BreakingEffect;
    [SerializeField] private GameObject Lights;
    [SerializeField] private GameObject FireParticlesPrefab;

    private GameObject FireParticles;

    private Coroutine TurnOffCoroutine;

    public void StartEngine()
    {
        TurnLights(true);
        if (TurnOffCoroutine != null) { StopCoroutine(TurnOffCoroutine); TurnOffCoroutine = null; } // Cancel engine turn off
        if (EngineWorkingSound.isPlaying) return; // Don't start engine if already working
        EngineStartSound.Play();
        EngineWorkingSound.PlayDelayed(EngineStartSound.clip.length - 0.8f);
        EngineStopSound.Stop(); // Stop turning off sound if already started
    }

    public void StopEngine()
    {   
        EngineWorkingSound.Stop();
        TurnLights(false);
    }
    public void TurnOffEngine()
    {   
        EngineWorkingSound.Stop();
        EngineStopSound.Play();
        TurnLights(false);
    }
    public IEnumerator TurnOffEngineDelayedCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        TurnOffEngine();
        TurnOffCoroutine = null;
    }
    public void TurnOffEngineDelayed(float stopDelay)
    {
       TurnOffCoroutine = StartCoroutine(TurnOffEngineDelayedCoroutine(stopDelay));
    }
    private void TurnLights(bool status)
    {
        Lights.SetActive(status);
    }

    public void BreakCar()
    {
        StopEngine();
        SteamSound.Play();
        FireSound.Play();

        FireParticles = Instantiate(FireParticlesPrefab, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
        FireParticles.transform.SetParent(transform);
        FireParticles.transform.localPosition = new UnityEngine.Vector3(0, 0.01f, 0);

        var breakEffectParticles = Instantiate(BreakingEffect, transform.position, UnityEngine.Quaternion.identity);
        ParticleSystem ps = breakEffectParticles.GetComponent<ParticleSystem>();
        Destroy(ps.gameObject, ps.main.duration);
    }

    public void RepairCar()
    {
        SteamSound.Stop();
        FireSound.Stop(); 
        Destroy(FireParticles);
    }

}
