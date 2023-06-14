using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    // Raycast 에 감지될 레이어 마스크
    public LayerMask layerMask;
    public GameObject checkButton;
    public UIManager uiManager;

    private Collider2D hitCollider = null;

    private float speed = 5f;
    private GameObject currentCheckButton = null;
    private GameData gameData = GameData.GetInstance();

    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(uiManager.IsPanelOpen())
        {
            // nothing
            animator.Play("Idle");
        } else
        {
            Vector3 direction = SetDirection();
            Move();

            // RayCast
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 2f, layerMask);
            hitCollider = hit.collider;
            if (hitCollider != null)
            {
                if (currentCheckButton == null)
                {
                    currentCheckButton = Instantiate(checkButton, hitCollider.transform.position, Quaternion.identity);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    // TODO: Panel이 뜨지 않는 Object
                    if(hitCollider.name.Contains("Dev_ItemGiver"))
                    {
                        int itemId = int.Parse(hitCollider.name.Substring(14));
                        gameData.addItem(itemId, 1);
                    } else
                    {
                        uiManager.OpenPanel(hitCollider.name);
                    }

                    
                }
            }
            else
            {
                if (currentCheckButton != null)
                {
                    Destroy(currentCheckButton);
                    currentCheckButton = null;
                }
            }
        }

        
    }

    Vector3 SetDirection()
    {
        Vector3 cursorDirection = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f));
        Vector3 direction = cursorDirection - transform.position;

        if (Mathf.Approximately(direction.x, 0f) && Mathf.Approximately(direction.y, 0))
        { }
        else
        {
            animator.SetFloat("x", direction.x);
            animator.SetFloat("y", direction.y);
        }

        return direction;
    }

    void Move()
    {
        float x = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;

        float y = 0f;
        if (Input.GetKey(KeyCode.S)) y -= 1f;
        if (Input.GetKey(KeyCode.W)) y += 1f;

        if (Mathf.Approximately(x, 0f) && Mathf.Approximately(y, 0))
        {
            animator.Play("Idle");
        }
        else
        {
            animator.Play("Run");

        }
        transform.position += new Vector3(x, y, 0).normalized * speed * Time.deltaTime;
    }
}


