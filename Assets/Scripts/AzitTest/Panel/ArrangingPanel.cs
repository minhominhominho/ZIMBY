using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.FilePathAttribute;

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
    private bool isRearrangeMode = false;
    private Vector3 beforeRearrangePos = Vector3.zero;
    private GameObject shadow = null;

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
                    arrangableItemObject.GetComponent<Button>().onClick.AddListener(() => OnClickNewFurniture(itemId));
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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetArrangeMode(false);
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
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(shadowPos, Vector2.zero, .1f, furnitureLayer);
            if (hit.collider != null && !hit.collider.CompareTag("Door"))
            {
                if (hoveredFurniture == null)
                {
                    hoveredFurniture = hit.collider;
                    hoveredFurniture.GetComponent<FurnitureController>().OnHoverEnter();
                }
                else if (hoveredFurniture != hit.collider)
                {
                    FurnitureController hfc = hoveredFurniture.GetComponent<FurnitureController>();

                    hfc.OnHoverExit();
                    hoveredFurniture = hit.collider;
                    hfc.OnHoverEnter();
                }
            }
            else
            {
                if (hoveredFurniture != null)
                {
                    hoveredFurniture.GetComponent<FurnitureController>().OnHoverExit();
                    hoveredFurniture = null;
                }
            }

            // 가구 재배치를 위해 클릭했을 때
            if (Input.GetMouseButtonDown(0) && hoveredFurniture != null)
            {
                // Set Arrange Mode
                OnClickArrangedFurniture();
            }
            // 가구 철거를 위해 오른쪽 클릭했을 때
            else if(Input.GetMouseButtonDown(1) && hoveredFurniture != null)
            {
                FurnitureLocation location = hoveredFurniture.GetComponent<FurnitureController>().GetLocation();

                Debug.Log("가구 " + location.itemId + " 삭제 시도");

                // if garden, must reset
                if(location.IsGarden())
                {
                    GardenLocation gl = location as GardenLocation;
                    if(gl.age != 0)
                    {
                        // TODO: can not remove garden with seeds
                        return;
                    }
                }

                // Game Data
                gameData.removeFurniture(location.locationId);
                gameData.AddItem(location.itemId, 1);

                // Destroy Game Object
                Destroy(hoveredFurniture.gameObject);
                hoveredFurniture = null;
            }
        }
    }

    private void OnClickNewFurniture(int itemId)
    {
        GameObject shadowResource = rm.GetPrefab(itemId.ToString());
        shadow = Instantiate<GameObject>(shadowResource, Vector3.zero, Quaternion.identity);
        shadow.GetComponent<FurnitureController>().SetLocation(gameData.CreateFurnitureLocation(itemId));
        SetArrangeMode(true);
    }

    private void OnClickArrangedFurniture()
    {
        this.isRearrangeMode = true;
        this.beforeRearrangePos = hoveredFurniture.transform.position;
        shadow = hoveredFurniture.gameObject;
        SetArrangeMode(true);
    }

    /// <summary>
    /// 가구 배치 모드 설정해주는 함수
    /// </summary>
    /// <param name="itemId">표시하려는 아이템의 id, -1이면 shadow 삭제</param>
    private void SetArrangeMode(bool isArrangeMode)
    {
        // EXIT ARRANGE MODE
        if (!isArrangeMode)
        {
            if (isRearrangeMode)
            {
                shadow.transform.position = beforeRearrangePos;
                shadow.GetComponent<BoxCollider2D>().isTrigger = false;
                Destroy(shadow.GetComponent<Shadow>());

                isRearrangeMode = false;
                beforeRearrangePos = Vector3.zero;
            }

            // make shadow null
            shadow = null;

            // unlock GetKey
            uiManager.LockGetKey(false);

            // panel reload
            resetMyFurnitures();
            loadMyFurnitures();

            // show panel again
            arrangablePanel.SetActive(true);
        }
        else
        {
            Debug.Assert(shadow != null, "Enter Arrange Mode but shadow is null");

            // set Shadow
            shadow.GetComponent<BoxCollider2D>().isTrigger = true;
            shadow.AddComponent<Shadow>();

            // lock uiManager GetKey
            uiManager.LockGetKey(true);

            // hide panel
            arrangablePanel.SetActive(false);
        }
    }

    private void Arrange(Vector3 shadowPos)
    {
        FurnitureController controller = shadow.GetComponent<FurnitureController>();
        FurnitureLocation location = controller.GetLocation();
        int direction = location.Direction;

        // modify Game Data
        if (isRearrangeMode)
        {
            beforeRearrangePos = shadowPos;
            int locationId = controller.GetLocation().locationId;
            location = gameData.RelocateFurniture(locationId, shadowPos);
            // TODO: No More Furniture Alarm
            if (location == null) return;
        }
        else
        {
            gameData.AddItem(location.itemId, -1);
            location = gameData.LocateFurniture(shadowPos, location);
            // TODO: No More Furniture Alarm
            if (location == null) return;
        }


        // make furniture
        shadow.name = location.itemId.ToString();
        shadow.GetComponent<BoxCollider2D>().isTrigger = false;
        Destroy(shadow.GetComponent<Shadow>());
        if (!isRearrangeMode)
        {
            controller.SetLocation(location);
            if (location.IsGarden()) shadow.GetComponent<GardenController>().SetGarden(location as GardenLocation);
        }

        // arrange mode off
        SetArrangeMode(false);
    }
}
