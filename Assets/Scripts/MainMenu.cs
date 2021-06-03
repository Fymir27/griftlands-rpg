using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour, IPointerEnterHandler
{
    public const string PlayerPrefVolume = "volume";
    public const string PlayerPrefCharacterUnlockState = "characterUnlockState";
    public const string PlayerPrefCurrentScene = "currentScene";

    [SerializeField]
    string firstScene;
    [SerializeField]
    Button loadButton;
    [SerializeField]
    Slider volumeSlider;
    [SerializeField]
    MenuCursor cursor;

    private void Start()
    {
        if (PlayerPrefs.HasKey(PlayerPrefVolume))
        {
            volumeSlider.value = PlayerPrefs.GetFloat(PlayerPrefVolume);
        }
        if (!PlayerPrefs.HasKey(PlayerPrefCurrentScene))
        {
            loadButton.interactable = false;
        }              
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteKey(PlayerPrefCharacterUnlockState);
        PlayerPrefs.DeleteKey(PlayerPrefCurrentScene);

        int fallbackSceneIndex = 0;
        var sceneNames = new List<string>();
        for (int i = SceneManager.sceneCountInBuildSettings - 1; i >= 0; i--)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            sceneNames.Add(sceneName);

            if (i != SceneManager.GetActiveScene().buildIndex)
            {
                fallbackSceneIndex = i;
            }
        }

        if (firstScene == "")
        {
            Debug.LogError("No first scene defined!");
            SceneManager.LoadScene(fallbackSceneIndex);
            return;
        }

        if (firstScene == "MainMenu")
        {
            Debug.LogError("First scene can't be menu scene!");
            SceneManager.LoadScene(fallbackSceneIndex);
            return;
        }

        if (!sceneNames.Contains(firstScene))
        {
            Debug.LogError("Invalid first scene!");
            SceneManager.LoadScene(fallbackSceneIndex);
            return;
        }

        Destroy(AudioController.Instance.gameObject);  // might be in DontDestroyOnLoad still
        SceneManager.LoadScene(firstScene);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(PlayerPrefCurrentScene))
        {
            Destroy(AudioController.Instance.gameObject);  // might be in DontDestroyOnLoad still
            SceneManager.LoadScene(PlayerPrefs.GetString(PlayerPrefCurrentScene));
        }        
        else
        {
            Debug.LogError("Saved game not found");
        }
    }

    public void LoadSceneKeepSound(string sceneName)
    {
        if (AudioController.Instance != null)
        {
            DontDestroyOnLoad(AudioController.Instance.gameObject);
        }
     
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
}
