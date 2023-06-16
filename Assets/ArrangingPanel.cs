using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArrangingPanel : MonoBehaviour
{
    public GameObject arrangableItem;
    public GameObject arrangablePanel;
    public GameObject arrangablePanelContent;
    public UIManager uiManager;

    private GameData gameData = GameData.GetInstance();

    private List<GameObject> arrangableItemObjectList = new List<GameObject>();
    private GameObject shadowResource = null;
    private GameObject shadow = null;
    private int shadowItemId = -1;

    private void OnEnable()
    {
        loadMyFurnitures();
    }

    private void OnDisable()
    {
        resetMyFurnitures();
    }

    private void loadMyFurnitures()
    {
        int[] inventory = gameData.GetInventory();

        // only Furniture
        for (int i = 1000; i < 2000; i++)
        {
            if (inventory[i] > 0)
            {
                for (int j = 0; j < inventory[i]; j++)
                {
                    GameObject arrangableItemObject = Instantiate<GameObject>(arrangableItem, Vector3.zero, Quaternion.identity, arrangablePanelContent.transform);
                    arrangableItemObjectList.Add(arrangableItemObject);
                    arrangableItemObject.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("icon_item_" + i);
                    arrangableItemObject.transform.GetChild(1).GetComponent<TMP_Text>().text = gameData.items[i].GetName();
                    int itemId = i;
                    arrangableItemObject.GetComponent<Button>().onClick.AddListener(() => SetArrangeMode(itemId));
                }
            }
        }
    }

    private void resetMyFurnitures()
    {
        foreach (GameObject arrangableItemObject in arrangableItemObjectList)
        {
            Destroy(arrangableItemObject);
        }
        arrangableItemObjectList.Clear();
    }

    void Update()
    {
        if (shadow != null) // if Arrange Mode
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                SetArrangeMode(-1);
                return;
            }

            if(Input.GetKeyDown(KeyCode.R))
            {
                shadow.GetComponent<Furniture>().Rotate();
                return;
            }

            Vector3 cursorPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f));
            Vector3 shadowPos = new(Mathf.Round(cursorPos.x), Mathf.Round(cursorPos.y), 1f);
            shadow.transform.position = shadowPos;

            if (shadow.GetComponent<Shadow>().IsValid())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Arrange(shadowPos);
                }
            }
        }
    }

    /// <summary>
    /// 가구 배치 모드 설정해주는 함수
    /// </summary>
    /// <param name="itemId">표시하려는 아이템의 id, -1이면 shadow 삭제</param>
    private void SetArrangeMode(int itemId)
    {
        if (itemId == -1)
        {
            // destroy shadow
            Destroy(shadow);
            shadow = null;

            // unlock GetKey
            uiManager.LockGetKey(false);

            // show panel again
            arrangablePanel.SetActive(true);

            // initialize
            shadowResource = null;
            shadow = null;
            shadowItemId = -1;
        }
        else
        {
            // show Shadow
            shadowResource = Resources.Load<GameObject>("Prefabs/" + itemId);
            shadow = Instantiate<GameObject>(shadowResource, Vector3.zero, Quaternion.identity);
            shadow.GetComponent<BoxCollider2D>().isTrigger = true;
            shadowItemId = itemId;

            // lock uiManager GetKey
            uiManager.LockGetKey(true);

            // hide panel
            arrangablePanel.SetActive(false);
        }
    }

    private void Arrange(Vector3 shadowPos)
    {
        // modify data
        gameData.addItem(shadowItemId, -1);

        // make furniture
        GameObject arranged = Instantiate<GameObject>(shadowResource, shadowPos, Quaternion.identity);
        arranged.name = shadowItemId.ToString();
        arranged.GetComponent<Furniture>().SetDirection(shadow.GetComponent<Furniture>().GetDirection());

        // panel reload
        resetMyFurnitures();
        loadMyFurnitures();

        // arrange mode off
        SetArrangeMode(-1);

    }
}
