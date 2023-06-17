using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DevMapManager : MonoBehaviour
{
    public GameObject itemPrefab;
    public GameObject itemPanelContent;

    private GameData gameData = GameData.GetInstance();
    private int[] gains = new int[10000];

    private void Start()
    {
        for(int i=0; i<gameData.items.Length; i++)
        {
            Item item = gameData.items[i];
            if (item == null) continue;

            GameObject itemObject = Instantiate<GameObject>(itemPrefab, Vector3.zero, Quaternion.identity, itemPanelContent.transform);
            int id = i;
            itemObject.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_item_" + id);
            itemObject.transform.GetChild(1).GetComponent<TMP_InputField>().onValueChanged.AddListener((val) => OnChange(id, val));
        }
        
    }

    private void OnChange(int id, string value)
    {
        gains[id] = int.Parse(value);
    }

    public void OnClickFinish()
    {
        for(int i=0; i<gameData.items.Length; i++)
        {
            if (gains[i] > 0)
            {
                gameData.AddItem(i, gains[i]);
            }
        }

        SceneManager.LoadScene("Azit");
    }
}
