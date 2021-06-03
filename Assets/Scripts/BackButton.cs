using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour
{
    [SerializeField]
    string previousScene = "MainMenu";
    [SerializeField]
    bool keepSound = true;

    public void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Back);
    }

    public void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Back();
        }
    }

    public void Back()
    {
        if (keepSound)
        {
            DontDestroyOnLoad(AudioController.Instance.gameObject);
        }
        SceneManager.LoadScene(previousScene);
    }
}
