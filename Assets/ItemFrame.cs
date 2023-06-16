using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemFrame : MonoBehaviour
{
    public void SetItem(int itemId, int count)
    {
        name = "Item_" + itemId;
        transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_item_" + itemId);
        transform.GetChild(1).GetComponent<TMP_Text>().text = "" + count;
    }
}
