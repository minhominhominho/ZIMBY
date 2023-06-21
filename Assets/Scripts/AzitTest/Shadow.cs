using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    private List<Collider2D> colliders = new List<Collider2D>();

    private bool isValid = true;
    public bool IsValid() { return isValid; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        colliders.Add(collision);
        isValid = false;
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        colliders.Remove(collision);
        if (!HasCollider())
        {
            isValid = true;
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private bool HasCollider()
    {
        return colliders.Count > 0;
    }
}
