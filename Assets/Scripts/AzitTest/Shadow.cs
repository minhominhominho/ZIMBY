using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    private bool isValid = true;

    public bool IsValid() { return isValid; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isValid = false;
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isValid = true;
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
