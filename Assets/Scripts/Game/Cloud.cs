using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Cloud : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private GameController gameController;
    private SpriteRenderer spriteRenderer;

    public enum Distance
    {
        Close = 2, Medium = 3, Far = 4
    }
    private Distance distance;
    private float lenght;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameController = FindAnyObjectByType<GameController>();

        object[] data = info.photonView.InstantiationData;
        distance = (Distance)data[0];
        lenght = (float)data[1];

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
        spriteRenderer.size = new Vector2(lenght, 1f);
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