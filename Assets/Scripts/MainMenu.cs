using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private static MainMenu s_Instance;
    public static MainMenu Instance => s_Instance;

    [Header("References")] 
    [SerializeField] private GameObject MenuCamera;
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private Canvas MainCanvas;
    [SerializeField] private GameObject CameraSpline;


    [Header("Settings")]
    [SerializeField] private float TransitionDuration = 1f;

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
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();  
        player.OnPlayerReady += ShowMenu;
        MenuCamera.gameObject.SetActive(true);  
        MainMenuPanel.SetActive(true);
        MainMenuPanel.GetComponent<UIFade>().Disable();
    }

    public void RestartGame()
    {
        MenuCamera.gameObject.SetActive(true);
    }
    public void ShowMenu()
    {
        MainMenuPanel.GetComponent<UIFade>().FadeIn();
    }

    private void StartRound()
    {
        MainMenuPanel.GetComponent<UIFade>().FadeOut();
        CameraSpline.GetComponent<CameraTransition>().StartTransition(TransitionDuration);
        GameMode.Instance.StartRound();
    }
    private IEnumerator StartRoundDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        StartRound();
    }

    public void OnQuitButtonClick()
    {
        Application.Quit();
    }

    public void OnPlayButtonClick()
    {
        StartCoroutine(StartRoundDelayed());
    }
}
