using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public SpriteRenderer birdRenderer;
    private Rigidbody2D rb;
    Material materialInstance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        GameController gc = FindObjectOfType<GameController>();
        if (gc != null && !gc.IsGameStarted())
        {
            rb.simulated = false;
        }

        if (!photonView.IsMine)
        {
            rb.simulated = false;
        }
    }

    private void Start()
    {
        materialInstance = birdRenderer.material;
        ApplyColors(photonView.Owner);
    }

    private void ApplyColors(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.TryGetValue("colors", out object obj))
        {
            byte[] data = (byte[])obj;

            Color body = new Color32(data[0], data[1], data[2], 255);
            Color wing = new Color32(data[3], data[4], data[5], 255);
            Color feather = new Color32(data[6], data[7], data[8], 255);

            materialInstance.SetColor("_RedColor", body);
            materialInstance.SetColor("_GreenColor", wing);
            materialInstance.SetColor("_BlueColor", feather);
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("colors"))
        {
            ApplyColors(targetPlayer);
        }
    }

    public void EnablePhysics()
    {
        if (rb != null)
            rb.simulated = true;
    }
}