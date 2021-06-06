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
    [SerializeField]
    GameObject overlayMenu;

    private bool openedThisFrame = false;

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
        if (openedThisFrame)
        {
            openedThisFrame = false;
            return;
        }

        if (Input.GetButtonDown("Cancel") || Input.GetButtonDown("Pause"))
        {
            if (controls.activeInHierarchy)
            {
                controls.SetActive(false);
                overlayMenu.SetActive(true);
            } 
            else
            {
                Close();
            }            
        }
    }

    public void OpenControlsOverlay()
    {
        overlayMenu.SetActive(false);
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
        openedThisFrame = true;
        overlayMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Close()
    {
        overlayMenu.SetActive(false);
        if (OnClose != null)
        {
            OnClose.Invoke();
        }
    }    
}
