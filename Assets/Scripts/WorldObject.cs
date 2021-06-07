using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public string Name;
    public bool Solid;
    public bool Interactable;

    [SerializeField]
    // TODO: different reaction from different characters
    string[] dialogue;

    // Start is called before the first frame update
    void Start()
    {
        var world = World.Instance;
        var gridPos = world.WorldToGridPos(transform.position);
        // snap to grid
        transform.position = world.GridToWorldPos(gridPos);

        world.SetObject(this, gridPos);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        string message = "...";
        if(Interactable || dialogue.Length > 0)
        {
            int randomIndex = Random.Range(0, dialogue.Length);
            message = dialogue[randomIndex];
        }
        
        Textbox.Instance.Display(this, message);
    }
}
