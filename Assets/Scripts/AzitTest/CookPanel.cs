using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookPanel : MonoBehaviour
{
    public GameObject availablePanelContent;
    public GameObject materialPanelContent;
    public GameObject availableItem;
    public GameObject materialItem;

    private GameData gameData = GameData.GetInstance();

    private List<GameObject> availableItems = new List<GameObject>();
    private List<GameObject> materialItems = new List<GameObject>();

    private const int NO_SELECT = 0;
    private int selectedItemId = NO_SELECT;

    private void OnEnable()
    {
        bool[] learnedRecipes = gameData.GetLearnedRecipes();
        for (int i = 3000; i < 4000; i++)
        {
            if (learnedRecipes[i])
            {
                GameObject availableItemObject = Instantiate<GameObject>(availableItem, Vector3.zero, Quaternion.identity, availablePanelContent.transform);
                availableItems.Add(availableItemObject);

                availableItemObject.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_item_" + i);
                availableItemObject.transform.GetChild(1).GetComponent<TMP_Text>().text = gameData.items[i].GetName();
                int itemId = i;
                availableItemObject.GetComponent<Button>().onClick.AddListener(() => OnClickAvailableItem(itemId));
            }
        }
    }

    private void OnDisable()
    {
        foreach (GameObject availableItemObject in availableItems)
        {
            Destroy(availableItemObject);
        }
        availableItems.Clear();

        ClearMaterialItems();
    }

    private void OnClickAvailableItem(int itemId)
    {
        ClearMaterialItems();

        selectedItemId = itemId;
        int[,] recipe = gameData.recipes[itemId].getMaterials();
        for (int i = 0; i < recipe.GetLength(0); i++)
        {
            int id = recipe[i, 0];
            int count = recipe[i, 1];

            GameObject materialItemObject = Instantiate<GameObject>(materialItem, Vector3.zero, Quaternion.identity, materialPanelContent.transform);
            materialItems.Add(materialItemObject);

            materialItemObject.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_item_" + id);
            materialItemObject.transform.GetChild(1).GetComponent<TMP_Text>().text = gameData.items[id].GetName();
            materialItemObject.transform.GetChild(2).GetComponent<TMP_Text>().text = gameData.GetInventory()[id] + " / " + count;
        }
    }

    private void ClearMaterialItems()
    {
        foreach (GameObject materialItemObject in materialItems)
        {
            Destroy(materialItemObject);
        }
        materialItems.Clear();
    }

    public void OnClickCrafting()
    {
        if (selectedItemId == NO_SELECT) return;

        bool canCraft = true;
        int[,] recipe = gameData.recipes[selectedItemId].getMaterials();
        for (int i = 0; i < recipe.GetLength(0); i++)
        {
            int id = recipe[i, 0];
            int count = recipe[i, 1];

            if (gameData.GetInventory()[id] < count)
            {
                canCraft = false;
                break;
            }
        }

        if (canCraft)
        {
            for (int i = 0; i < recipe.GetLength(0); i++)
            {
                int id = recipe[i, 0];
                int count = recipe[i, 1];

                gameData.GetInventory()[id] -= count;
            }

            gameData.GetInventory()[selectedItemId]++;

            OnClickAvailableItem(selectedItemId);
        }
    }
}
