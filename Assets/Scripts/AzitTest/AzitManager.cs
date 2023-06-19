using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AzitManager : MonoBehaviour
{
    public UIManager uiManager;
    public GameObject character;
    public Camera mainCamera;

    private GameData gameData = GameData.GetInstance();
    private ResourceManager rm = ResourceManager.GetInstance(); 

    private void Awake()
    {
        rm.Load();

        List<FurnitureLocation> interior = gameData.GetAzitInterior();
        foreach(FurnitureLocation location in interior)
        {
            GameObject obj = Instantiate<GameObject>(
                rm.GetPrefab(location.id.ToString()),
                location.pos,
                Quaternion.identity
            );
            obj.name = location.id.ToString();
            obj.GetComponent<FurnitureController>().SetDirection(location.direction);
        }
    }

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
        return tag == "CraftingTable" || tag == "Storage" || tag == "Door" || tag == "Stove";
    }
}
