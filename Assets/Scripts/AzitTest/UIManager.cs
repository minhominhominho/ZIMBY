using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject storagePanel;
    public GameObject craftingPanel;
    public GameObject arrangingPanel;
    public GameObject leavePanel;

    // 유저와 상호작용하는 Panel이 열려있는 지 여부 (Storage, Counter, ...)
    private bool isPanelOpen = false;
    private GameObject openedPanel = null;
    private bool lockGetKey = false;

    public bool IsPanelOpen() { return isPanelOpen; }

    public void ClosePanel()
    {
        Debug.Assert(isPanelOpen, "Call closePanel when panel is not open");
        isPanelOpen = false;
        openedPanel.SetActive(false);
    }

    public void OpenPanel(string panelName)
    {
        Debug.Assert(!isPanelOpen, "Call OpenPanel when panel is open");
        isPanelOpen = true;
        if (panelName == "Storage")
        {
            storagePanel.SetActive(true);
            openedPanel = storagePanel;
        }
        else if(panelName == "CraftingTable")
        {
            craftingPanel.SetActive(true);
            openedPanel = craftingPanel;
        }
        else if(panelName == "Arranging")
        {
            arrangingPanel.SetActive(true);
            openedPanel = arrangingPanel;
        } else if(panelName == "Door")
        {
            leavePanel.SetActive(true);
            openedPanel = leavePanel;
        }
    }

    public void LockGetKey(bool lockGetKey)
    {
        this.lockGetKey = lockGetKey;
    }

    private void Update()
    {
        if(isPanelOpen && !lockGetKey)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePanel();
                return;
            }
        }
    }
}
