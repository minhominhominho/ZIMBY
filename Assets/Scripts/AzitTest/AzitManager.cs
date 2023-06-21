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
    private GameObject selectedGarden = null;

    private void Awake()
    {
        rm.Load();

        // 시간 처리
        gameData.GoodMorning();

        gameData.DoAllFurnitureLocation((y, x, location) =>
        {
            GameObject obj = Instantiate<GameObject>(
                rm.GetPrefab(location.itemId.ToString()),
                new Vector2((float)y / 2, (float)x / 2),
                Quaternion.identity);
            obj.name = location.itemId.ToString();
            obj.GetComponent<FurnitureController>().SetDirection(location.direction);
            obj.GetComponent<FurnitureController>().SetLocation(location);

            if(location.IsGarden())
            {
                GardenLocation gLocation = location as GardenLocation;
                obj.GetComponent<GardenController>().SetGarden(gLocation);
            }
        });
    }

    void Update()
    {
        // camera
        mainCamera.transform.position = new(character.transform.position.x, character.transform.position.y, -10f);
        
    }

    public void UseFurniture(GameObject furniture)
    {
        string tag = furniture.tag;

        if(HasToOpenPanel(tag))
        {
            uiManager.OpenPanel(tag);
            if(tag == "Garden")
            {
                selectedGarden = furniture;
            }
        }
    }

    private bool HasToOpenPanel(string tag)
    {
        return tag == "CraftingTable" || tag == "Storage" || tag == "Door" || tag == "Stove" || tag == "Garden";
    }

    public void PlantSeed(int itemId)
    {
        selectedGarden.GetComponent<GardenController>().PlantSeed(itemId);
    }
}
