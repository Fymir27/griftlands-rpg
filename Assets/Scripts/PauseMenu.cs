using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    public const string PlayerPrefVolume = "volume";

    public event System.Action OnClose;

    [SerializeField]
    Button resumeButton;
    [SerializeField]
    Slider volumeSlider;
    [SerializeField]
    MenuCursor cursor;
    [SerializeField]
    GameObject controls;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(PlayerPrefVolume))
        {
            volumeSlider.value = PlayerPrefs.GetFloat(PlayerPrefVolume);
        }        
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Close();
        }
    }

    public void OpenControlsOverlay()
    {
        controls.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Exit()
    {
        Debug.Log("Application Quit - This won't work in the Unity Editor");
        Application.Quit();
    }

    public void OnVolumeChanged(float sliderValue)
    {
        PlayerPrefs.SetFloat(PlayerPrefVolume, sliderValue);
        AudioListener.volume = sliderValue;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cursor != null)
        {
            EventSystem.current.SetSelectedGameObject(eventData.hovered[0]);
        }
    }

    public void Toggle()
    {        
        if (gameObject.activeInHierarchy)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    public void Open()
    {   
        gameObject.SetActiveRecursively(true);
        controls.SetActive(false);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Close()
    {
        if (controls.activeInHierarchy)
        {
            controls.SetActive(false);
            return;
        }

        gameObject.SetActiveRecursively(false);
        if (OnClose != null)
        {
            OnClose.Invoke();
        }
    }    
}
