using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Actor
{
    [SerializeField]
    Text hpText = null;

    // Start is called before the first frame update
    void Start()
    {
        InitActor();
    }

    // Update is called once per frame
    void Update()
    {
        if(hpText != null)
            hpText.text = $"HP: {CurHealth}/{MaxHealth}";
    }
}
