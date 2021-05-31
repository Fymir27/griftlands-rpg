using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearAfterUnlock : MonoBehaviour
{
    [SerializeField]
    PlayerCharacter unlocksAfter;

    private void Start()
    {        
        if (PlayerPrefs.HasKey(Player.PlayerPrefCharacterUnlockState))
        {
            if (PlayerPrefs.GetInt(Player.PlayerPrefCharacterUnlockState) >= (int)unlocksAfter)
            {
                Destroy(gameObject);
            }
        }
        
    }
}
