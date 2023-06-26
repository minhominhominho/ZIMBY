using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropController : MonoBehaviour
{
    public int maxBounce;

    public float xForce;
    public float yForce;
    public float gravity;

    private Vector2 direction;
    private int currentBounce = 0;
    private bool isGrounded = true;

    private float maxHeight;
    private float currentHeight;

    public Transform sprite;
    public Transform shadow;

    private void Start()
    {
        currentHeight = Random.Range(yForce - 1, yForce);
        maxHeight = currentHeight;
        Initialize(new(Random.Range(-xForce, xForce), Random.Range(-xForce, xForce)));
    }

    void Update()
    {
        if(!isGrounded)
        {
            currentHeight += -gravity * Time.deltaTime;
            sprite.position += new Vector3(0, currentHeight, 0) * Time.deltaTime;
            transform.position += (Vector3)direction * Time.deltaTime;

            float totalVelocity = Mathf.Abs(currentHeight) + Mathf.Abs(maxHeight);
            float scaleXY = Mathf.Abs(currentHeight) / totalVelocity;
            shadow.localScale = Vector2.one * Mathf.Clamp(scaleXY, .5f, 1f);

            CheckGroundHit();
        }
    }

    private void Initialize(Vector2 direction)
    {
        isGrounded = false;
        maxHeight /= 1.5f;
        this.direction = direction;
        currentHeight = maxHeight;
        currentBounce++;
    }

    private void CheckGroundHit()
    {
        if(sprite.position.y < shadow.position.y)
        {
            sprite.position = shadow.position;
            shadow.localScale = Vector2.one;

            if(currentBounce < maxBounce)
            {
                Initialize(direction / 1.5f);
            } else
            {
                Destroy(gameObject);
            }
        }
    }
}
