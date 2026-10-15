using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Cloud : MonoBehaviourPun
{
    private GameController gameController;
    private SpriteRenderer spriteRenderer;

    public enum Distance
    {
        Close = 2, Medium = 3, Far = 4
    }
    private Distance distance;

    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameController = FindAnyObjectByType<GameController>();

        float dis = Random.Range(0, 3);
        float length = Random.Range(1, 4);
        if (length != 1)
            length = Random.Range(1, 4);

        distance = Distance.Far;

        switch (dis)
        {
            case 0:
                distance = Distance.Close;
                break;
            case 1:
                distance = Distance.Medium;
                break;
            case 2:
                distance = Distance.Far;
                break;
        }

        float result = 0f;

        switch (distance)
        {
            case Distance.Close:
                result = 100f / 255f;
                break;
            case Distance.Medium:
                result = 50f / 255f;
                break;
            case Distance.Far:
                result = 25f / 255f;
                break;
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, result);
        spriteRenderer.size = new Vector2(length, 1f);
    }

    private void Update()
    {
        transform.position += 
            (Vector3.left * 
            Time.deltaTime * 
            gameController.moveSpeed) / 
            (float)distance;
    }
}