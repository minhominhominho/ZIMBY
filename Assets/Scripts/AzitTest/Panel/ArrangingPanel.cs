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
    public LayerMask furnitureLayer;

    private GameData gameData = GameData.GetInstance();
    private ResourceManager rm = ResourceManager.GetInstance(); 

    private List<GameObject> arrangableItemObjectList = new List<GameObject>();
    private GameObject shadowResource = null;
    private GameObject shadow = null;
    private int shadowItemId = -1;

    private Collider2D hoveredFurniture = null;

    private void OnEnable()
    {
        loadMyFurnitures();
    }

    private void OnDisable()
    {
        if (hoveredFurniture != null) hoveredFurniture.GetComponent<FurnitureController>().OnHoverExit();
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
                    arrangableItemObject.transform.GetChild(0).GetComponent<Image>().sprite = rm.GetIcon(i.ToString());
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
        Vector3 cursorPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f));
        Vector3 shadowPos = new(Mathf.Round(cursorPos.x * 2f) * .5f, Mathf.Round(cursorPos.y * 2f) * .5f, 1f);

        if (shadow != null) // if Arrange Mode
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                SetArrangeMode(-1);
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                shadow.GetComponent<FurnitureController>().Rotate();
                return;
            }
            
            shadow.transform.position = shadowPos;

            if (shadow.GetComponent<Shadow>().IsValid())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Arrange(shadowPos);
                }
            }
        } else
        {
            RaycastHit2D hit = Physics2D.Raycast(shadowPos, Vector2.zero, .1f, furnitureLayer);
            if (hit.collider != null && !hit.collider.CompareTag("Door"))
            {
                if (hoveredFurniture == null)
                {
                    hoveredFurniture = hit.collider;
                    hoveredFurniture.GetComponent<FurnitureController>().OnHoverEnter();
                } else if(hoveredFurniture != hit.collider)
                {
                    hoveredFurniture.GetComponent<FurnitureController>().OnHoverExit();
                    hoveredFurniture = hit.collider;
                    hoveredFurniture.GetComponent<FurnitureController>().OnHoverEnter();
                }
            } else
            {
                if(hoveredFurniture != null)
                {
                    hoveredFurniture.GetComponent<FurnitureController>().OnHoverExit();
                    hoveredFurniture = null;
                }
            }

            if(Input.GetMouseButtonDown(0) && hoveredFurniture != null)
            {
                FurnitureLocation location = hoveredFurniture.GetComponent<FurnitureController>().GetLocation();

                // add furniture
                gameData.AddItem(location.itemId, 1);

                // remove from gameData interior
                gameData.removeFurniture(location.locationId);

                // destroy
                Destroy(hoveredFurniture.gameObject);

                // Set Arrange Mode
                SetArrangeMode(location.itemId);
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

            // panel reload
            resetMyFurnitures();
            loadMyFurnitures();

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
            shadowResource = rm.GetPrefab(itemId.ToString());
            shadow = Instantiate<GameObject>(shadowResource, Vector3.zero, Quaternion.identity);
            shadow.GetComponent<BoxCollider2D>().isTrigger = true;
            shadow.AddComponent<Shadow>();
            shadowItemId = itemId;

            // lock uiManager GetKey
            uiManager.LockGetKey(true);

            // hide panel
            arrangablePanel.SetActive(false);
        }
    }

    private void Arrange(Vector3 shadowPos)
    {
        int direction = shadow.GetComponent<FurnitureController>().GetDirection();

        // modify data
        gameData.AddItem(shadowItemId, -1);
        FurnitureLocation location = gameData.LocateFurniture(shadowPos, shadowItemId, direction);
        // TODO: No More Furniture Alarm
        if (location == null) return;

        // make furniture
        GameObject arranged = Instantiate<GameObject>(shadowResource, shadowPos, Quaternion.identity);
        arranged.name = shadowItemId.ToString();
        arranged.GetComponent<FurnitureController>().SetDirection(direction);
        arranged.GetComponent<FurnitureController>().SetLocation(location);
        if (location.IsGarden()) arranged.GetComponent<GardenController>().SetGarden(location as GardenLocation);

        // arrange mode off
        SetArrangeMode(-1);

    }
}
