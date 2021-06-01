using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearAfterUnlock : MonoBehaviour
{
    [SerializeField]
    PlayerCharacter unlocksAfter;

    private void Start()
    {        
        if (PlayerPrefs.HasKey(MainMenu.PlayerPrefCharacterUnlockState))
        {
            if (PlayerPrefs.GetInt(MainMenu.PlayerPrefCharacterUnlockState) >= (int)unlocksAfter)
            {
                Destroy(gameObject);
            }
        }
        
    }
}
