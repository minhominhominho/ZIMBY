using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemFrame : MonoBehaviour
{
    private ResourceManager rm = ResourceManager.GetInstance();
    private StoragePanel panel;
    private int itemId;

    public void SetItem(StoragePanel panel, int itemId, int count)
    {
        this.panel = panel;
        this.itemId = itemId;
        name = "Item_" + itemId;
        transform.GetChild(0).GetComponent<Image>().sprite = rm.GetIcon(itemId.ToString());
        transform.GetChild(1).GetComponent<TMP_Text>().text = "" + count;
    }

    private void OnMouseEnter()
    {
        Debug.Log("Mouse Enter");
        panel.OnMouseEnterItem(itemId);
    }

    private void OnMouseExit()
    {
        panel.OnMouseExitItem();        
    }
}
