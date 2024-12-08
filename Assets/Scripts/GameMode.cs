using System;
using System.Collections;
using TMPro; 
using UnityEngine.UI;
using UnityEngine;
using System.Numerics;

public class GameMode : MonoBehaviour
{
    private static GameMode s_Instance;
    public static GameMode Instance => s_Instance;

    [SerializeField] private EGameState InitialGameState;
    public EGameState CurrentState {get; private set;}

    public float PreviousSurvivalTime {get; private set;}
    public float BestSurvivalTime {get; private set;}
    public float SurvivalTime {get; private set;}


    [Header("References")] 
    [SerializeField] private GameObject GameCamera;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject HUDPanel;

    [SerializeField] private AudioClip[] SoundTracks;
    [SerializeField] private GameObject MusicObjectPrefab;
    private GameObject DeathCameraObject;
    private GameObject MusicObject;

    private TextMeshProUGUI SurvivalTimeText;
    private TextMeshProUGUI PreviousSurvivalTimeText;
    private TextMeshProUGUI BestSurvivalTimeText;
    private TextMeshProUGUI CurrentSurvivalTimerText;



    private EnemySpawner EnemySpawner;
    private BoostSpawner BoostSpawner;
    public GameObject Player;


    public event Action OnRoundStarted;


    private void Awake()
    {
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            s_Instance = this;
        }
    }
    private void Start()
    {
        GameCamera.gameObject.SetActive(false);
        HUDPanel.SetActive(true);
        HUDPanel.GetComponent<UIFade>().Disable();
        GameOverPanel.SetActive(true);
        GameOverPanel.GetComponent<UIFade>().Disable();

        EnemySpawner = gameObject.GetComponent<EnemySpawner>();
        BoostSpawner = gameObject.GetComponent<BoostSpawner>();

        Player = GameObject.FindGameObjectWithTag("Player");
        Player.GetComponent<HealthManager>().OnDeath += EndRound;
        SurvivalTimeText = GameOverPanel.transform.Find("SurvivalTimeValue").GetComponent<TextMeshProUGUI>();
        PreviousSurvivalTimeText = GameOverPanel.transform.Find("PreviousSurvivalTimeValue").GetComponent<TextMeshProUGUI>();
        BestSurvivalTimeText = GameOverPanel.transform.Find("BestSurvivalTimeValue").GetComponent<TextMeshProUGUI>();
        CurrentSurvivalTimerText = HUDPanel.transform.Find("TimerGlow/Timer/Time").GetComponent<TextMeshProUGUI>();

        SurvivalTimeText.text = FormatTime(0);
        PreviousSurvivalTimeText.text = FormatTime(0);
        BestSurvivalTimeText.text = FormatTime(0);
        CurrentSurvivalTimerText.text = FormatTime(0);

        CurrentState = EGameState.MainMenu;
        StartCoroutine(FirstGameStart());
    }
    IEnumerator FirstGameStart()
    {
        yield return new WaitForEndOfFrame();
        RestartGame();
    }

    public void RestartGame()
    {
        DeleteAllEnemies();
        Player.GetComponent<PlayerController>().Revive();
    }

    public void StartRound()
    {  
        CurrentState = EGameState.InProgress;
        EnemySpawner.StartSpawning();
        BoostSpawner.StartSpawning();
        OnRoundStarted?.Invoke();    
        CreateSoundtrackObject();
        StartRoundUI();   
    }
    public void EndRound()
    {
        if (CurrentState == EGameState.GameOver) return;
        Debug.Log("Game Over");
        CurrentState = EGameState.GameOver;
        EnemySpawner.StopSpawning();
        BoostSpawner.StopSpawning();
        DestroySoundtrackObject();
        EndRoundUI();
        ActivateDeathCamera();
    }

    public void ActivateDeathCamera()
    {
        DeathCameraObject = Instantiate(GameCamera);

        UnityEngine.Vector3 targetOffset = new UnityEngine.Vector3(0, 7, -5);
        DeathCameraObject.transform.SetParent(Player.transform);

        StartCoroutine(ZoomCamera(DeathCameraObject.transform.position, targetOffset));
        GameCamera.SetActive(false);
    }

    private IEnumerator ZoomCamera(UnityEngine.Vector3 initialPosition, UnityEngine.Vector3 targetOffeset)
    {     
        float elapsedTime = 0f;

        while (elapsedTime < 1.5f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / 1.5f;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            DeathCameraObject.transform.position = UnityEngine.Vector3.Lerp(initialPosition, Player.transform.position + targetOffeset, easedT);
            yield return null;
        }
        DeathCameraObject.transform.position = Player.transform.position + targetOffeset;
    }

    private void DeleteAllEnemies()
    {
        GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject obj in objectsToDelete)
        {
            Destroy(obj);
        }
    }
    private void StartRoundUI()
    {
       HUDPanel.GetComponent<UIFade>().FadeIn();
    }
    private void CreateSoundtrackObject()
    {
        int soundtrackNumber = UnityEngine.Random.Range(0, SoundTracks.Length);
        MusicObject = Instantiate(MusicObjectPrefab);
        AudioSource asrc = MusicObject.GetComponent<AudioSource>();
        asrc.clip = SoundTracks[soundtrackNumber];
        asrc.Play();
    }
    private void DestroySoundtrackObject()
    {
        AudioSource audioSource = MusicObject.GetComponent<AudioSource>();
        StartCoroutine(FadeOutMusic(audioSource, 1f));
    }
    private IEnumerator FadeOutMusic(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        audioSource.Stop();
        Destroy(audioSource.gameObject);
    }
    private void EndRoundUI()
    {
        HUDPanel.GetComponent<UIFade>().FadeOut();
        GameOverPanel.GetComponent<UIFade>().FadeIn();
        if (BestSurvivalTime < SurvivalTime)
        {
            BestSurvivalTime = SurvivalTime;
        }
        SurvivalTimeText.text = FormatTime(SurvivalTime, "Time");
        PreviousSurvivalTimeText.text = FormatTime(PreviousSurvivalTime, "Previous Time");
        BestSurvivalTimeText.text = FormatTime(BestSurvivalTime, "Best Time");
    }

    private void Update()
    {
        if (CurrentState == EGameState.InProgress)
        {
            SurvivalTime += Time.deltaTime;
            CurrentSurvivalTimerText.text = FormatTime(SurvivalTime);

        }
        if (CurrentState == EGameState.InProgress && Input.GetKeyDown(KeyCode.Escape))
        {
            Player.GetComponent<HealthManager>().ChangeHealth(-10);
        }
    }

    private String FormatTime(float time, String prefix = "")
    {
        if (prefix.Length != 0) prefix += ": ";
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0}{1:00}:{2:00}", prefix, minutes, seconds);
    }

    public void OnContinueButtonClick()
    {
        CurrentState = EGameState.MainMenu;
        PreviousSurvivalTime = SurvivalTime;
        SurvivalTime = 0;
        GameOverPanel.GetComponent<UIFade>().FadeOut();
        GameCamera.SetActive(false);
        Destroy(DeathCameraObject);

        RestartGame();
        MainMenu.Instance.RestartGame();
    }
}
