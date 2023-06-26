using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeavePanel : MonoBehaviour
{
    public void OnClickLeaveButton()
    {
        SceneManager.LoadScene("Tmp Map");
    }
}
