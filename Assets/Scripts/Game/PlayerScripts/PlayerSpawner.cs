using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private BirdAppearanceData data;
    private SpriteRenderer playerRenderer;
    private BirdAppearance playerAppearance;

    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            playerRenderer = playerPrefab.GetComponent<SpriteRenderer>();
            playerAppearance = playerPrefab.GetComponent<BirdAppearance>();

            playerRenderer.sprite = data.skins[data.currentSkinIndex].GetComponent<SpriteRenderer>().sprite;
            playerPrefab.GetComponent<Animator>().runtimeAnimatorController = data.skins[data.currentSkinIndex].GetComponent<Animator>().runtimeAnimatorController;
            playerAppearance.materialInstance = new Material(data.skins[data.currentSkinIndex].GetComponent<SpriteRenderer>().sharedMaterial);
            playerRenderer.sharedMaterial = playerAppearance.materialInstance;

            if (playerAppearance.maskTexture != null)
                playerAppearance.materialInstance.SetTexture("_MaskTex", playerAppearance.maskTexture);

            SpawnPlayer();
        }
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Brak prefab gracza w PlayerSpawner!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Brak spawn pointów w PlayerSpawner!");
            return;
        }

        int index = (PhotonNetwork.CurrentRoom.PlayerCount - 1) % spawnPoints.Length;
        Vector3 spawnPos = spawnPoints[index].position;
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
    }
}
