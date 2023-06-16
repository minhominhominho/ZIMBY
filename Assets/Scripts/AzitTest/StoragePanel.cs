using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoragePanel : MonoBehaviour
{
    public GameObject itemFrame;
    public GameObject content;

    private GameData gameData = GameData.GetInstance();
    private List<GameObject> itemObjects = new List<GameObject>();

    private void OnEnable()
    {
        int[] inventory = gameData.GetInventory();

        for(int i=0; i<inventory.Length; i++)
        {
            if (inventory[i] > 0)
            {
                GameObject itemObject = Instantiate<GameObject>(itemFrame, Vector3.zero, Quaternion.identity, content.transform);
                itemObject.GetComponent<ItemFrame>().SetItem(i, inventory[i]);
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
}
