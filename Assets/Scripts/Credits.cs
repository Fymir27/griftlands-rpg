using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    public static Credits Instance;

    [SerializeField]
    TextAsset creditFile;

    [SerializeField]
    Text columnLeft;
    
    [SerializeField]
    Text columnRight;    

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        if (creditFile == null)
            return;

        var reader = new StringReader(creditFile.text);
        var lines = new List<string>();        
        while (true)
        {
            var line = reader.ReadLine();
            if (line is null)
                break;
            lines.Add(line);
        }

        Debug.Log("Credits file successfully parsed. Lines: " + lines.Count);

        // split into halves
        var stringBuilders = new[]
        {
            new StringBuilder(),
            new StringBuilder()
        };

        int half = lines.Count / 2 + 1;

        for (int i = 0; i < lines.Count; i++)
        {
            stringBuilders[i / half].AppendLine(lines[i]);
        }

        columnLeft.text = stringBuilders[0].ToString();
        columnRight.text = stringBuilders[1].ToString();
    }
}
