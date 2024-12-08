using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI FpsText;
    private int Frames = 0;
    private float ElapsedTime = 0.0f;

    void Start()
    {
        FpsText = gameObject.GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        Frames++;
        ElapsedTime += Time.unscaledDeltaTime;

        if (ElapsedTime >= 0.5f)
        {
            FpsText.text = $"{Mathf.Ceil(Frames / ElapsedTime)}";
            Frames = 0;
            ElapsedTime = 0.0f;
        }
    }
}