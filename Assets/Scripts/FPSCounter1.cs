using UnityEngine;
using TMPro;

public class ToggleFPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    private bool isVisible = false;

    void Start()
    {
        fpsText.gameObject.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isVisible = !isVisible;
            if (fpsText != null)
                fpsText.gameObject.SetActive(isVisible); 
        }
    }
}
