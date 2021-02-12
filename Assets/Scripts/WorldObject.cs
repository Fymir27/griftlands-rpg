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
        if(!Interactable || dialogue.Length == 0)
        {
            Debug.LogWarning("WorldObject.Interact() called but not interactable OR no dialogue");
            return;
        }

        int randomIndex = Random.Range(0, dialogue.Length - 1);
        Textbox.Instance.Display(this, dialogue[randomIndex]);
    }
}
