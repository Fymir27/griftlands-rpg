using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
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

    private void Start()
    {
        if (PlayerPrefs.HasKey(PlayerPrefVolume))
        {
            volumeSlider.value = PlayerPrefs.GetFloat(PlayerPrefVolume); //.Invoke(PlayerPrefs.GetFloat(PlayerPrefVolume));
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
        
        SceneManager.LoadScene(firstScene);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(PlayerPrefCurrentScene))
        {
            SceneManager.LoadScene(PlayerPrefs.GetString(PlayerPrefCurrentScene));
        }        
        else
        {
            Debug.LogError("Saved game not found");
        }
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
}
