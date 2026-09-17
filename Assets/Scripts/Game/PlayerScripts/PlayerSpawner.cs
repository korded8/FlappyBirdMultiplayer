using Photon.Pun;
using RematchManaging;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private BirdAppearanceData data;
    private SpriteRenderer playerRenderer;
    private BirdAppearance playerAppearance;

    private bool[] isOccupied = new bool[16];

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
        }

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Brak prefab gracza w PlayerSpawner!");
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Brak spawn pointów w PlayerSpawner!");
        }

        photonView.RPC(nameof(OccupySpawnPoints), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void OccupySpawnPoints(PhotonMessageInfo info)
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (isOccupied[i])
                continue;
            else
            {
                isOccupied[i] = true;
                photonView.RPC(nameof(SetSpawn), info.Sender, i);
                return;
            }
        }
    }

    [PunRPC]
    private void ReleaseSpawnPoints(int i)
    {
        isOccupied[i] = false;
    }

    [PunRPC]
    private void SetSpawn(int i)
    {
        isOccupied[i] = true;
        Vector3 spawnPos = spawnPoints[i].position;
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);

        if (Rematch.PreferMaster)
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);

        if (Rematch.PendingRematch)
            Rematch.Clear();
    }

    public void PlayerLeftRoom()
    {
        int index = -1;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (isOccupied[i])
            {
                index = i;
            }
        }
        photonView.RPC(nameof(ReleaseSpawnPoints), RpcTarget.MasterClient, index);
    }
}