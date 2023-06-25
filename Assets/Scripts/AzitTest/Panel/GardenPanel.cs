using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GardenPanel : StoragePanel
{
    public AzitManager am;
    public UIManager um;

    private void OnEnable()
    {
        int[] inventory = gameData.GetInventory();

        for(int i=4000; i<5000; i++)
        {
            int itemId = i;
            if (inventory[i] > 0)
            {
                GameObject itemObject = Instantiate<GameObject>(itemFrame, Vector3.zero, Quaternion.identity, content.transform);
                itemObject.GetComponent<ItemFrame>().SetItem(this, i, inventory[i]);
                itemObject.GetComponent<Button>().onClick.AddListener(() => OnClickItem(itemId));

                EventTrigger eventTrigger = itemObject.GetComponent<EventTrigger>();

                EventTrigger.Entry pointerEnterEntry = new();
                pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
                pointerEnterEntry.callback.AddListener((data) => OnMouseEnterItem(itemId));
                eventTrigger.triggers.Add(pointerEnterEntry);

                EventTrigger.Entry pointerExitEntry = new();
                pointerExitEntry.eventID = EventTriggerType.PointerExit;
                pointerExitEntry.callback.AddListener((data) => OnMouseExitItem());
                eventTrigger.triggers.Add(pointerExitEntry);

                itemObjects.Add(itemObject);
            }
        }
    }


    private void OnDisable()
    {
        foreach (GameObject itemObject in itemObjects)
        {
            Destroy(itemObject);
        }

        itemObjects.Clear();
    }

    private void OnClickItem(int itemId)
    {
        gameData.AddItem(itemId, -1);
        am.PlantSeed(itemId);
        um.ClosePanel();
    }
}
