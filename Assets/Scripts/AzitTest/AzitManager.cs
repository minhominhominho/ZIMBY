using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AzitManager : MonoBehaviour
{
    public UIManager uiManager;
    public GameObject character;
    public Camera mainCamera;

    void Update()
    {
        // camera
        mainCamera.transform.position = new(character.transform.position.x, character.transform.position.y, -10f);
        
    }

    public void UseFurniture(string tag)
    {
        if(HasToOpenPanel(tag))
        {
            uiManager.OpenPanel(tag);
        }
    }

    private bool HasToOpenPanel(string tag)
    {
        return tag == "CraftingTable" || tag == "Storage" || tag == "Door";
    }
}
