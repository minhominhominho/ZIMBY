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
            obj.GetComponent<FurnitureController>().SetLocation(location);
            obj.GetComponent<FurnitureController>().SetDirection(location.Direction);
            

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

        if (HasToOpenPanel(tag))
        {
            uiManager.OpenPanel(tag);

        }
        else if (tag == "Garden")
        {
            selectedGarden = furniture;
            int plantAge = furniture.GetComponent<GardenController>().GetPlantAge();
            Debug.Log(plantAge);
            if (plantAge == -1)
            {
                uiManager.OpenPanel(tag);
            } else if(plantAge < 3)
            {
                uiManager.OpenPanel("ResetGarden");
            } else if(plantAge == 3)
            {
                furniture.GetComponent<GardenController>().Harvest();
            }
        }
    }

    private bool HasToOpenPanel(string tag)
    {
        return tag == "CraftingTable" || tag == "Storage" || tag == "Door" || tag == "Stove";
    }

    public void ResetGarden()
    {
        selectedGarden.GetComponent<GardenController>().ResetGarden();
    }

    public void PlantSeed(int itemId)
    {
        Debug.Log(selectedGarden);
        selectedGarden.GetComponent<GardenController>().PlantSeed(itemId);
    }
}
