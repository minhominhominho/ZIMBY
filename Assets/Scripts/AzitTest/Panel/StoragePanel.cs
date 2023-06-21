using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StoragePanel : MonoBehaviour
{
    public GameObject itemFrame;
    public GameObject content;
    public GameObject itemInfo;
    public GameObject itemInfoContent;

    protected GameData gameData = GameData.GetInstance();
    protected ResourceManager rm = ResourceManager.GetInstance();
    protected List<GameObject> itemObjects = new List<GameObject>();

    private void OnEnable()
    {
        int[] inventory = gameData.GetInventory();

        for(int i=0; i<inventory.Length; i++)
        {
            int itemId = i;
            if (inventory[i] > 0)
            {
                GameObject itemObject = Instantiate<GameObject>(itemFrame, Vector3.zero, Quaternion.identity, content.transform);
                itemObject.GetComponent<ItemFrame>().SetItem(this, i, inventory[i]);
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

    public void OnMouseEnterItem(int id)
    {
        itemInfoContent.transform.GetChild(0).GetComponent<Image>().sprite = rm.GetIcon(id.ToString());
        itemInfoContent.transform.GetChild(1).GetComponent<TMP_Text>().text = gameData.items[id].GetName();
        itemInfoContent.transform.GetChild(2).GetComponent<TMP_Text>().text = gameData.GetItemType(id);
        itemInfoContent.transform.GetChild(3).GetComponent<TMP_Text>().text = gameData.items[id].description;
        itemInfo.SetActive(true);
    }

    public void OnMouseExitItem()
    {
        itemInfo.SetActive(false);
    }
}
