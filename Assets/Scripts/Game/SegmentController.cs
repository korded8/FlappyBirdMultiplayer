using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SegmentController : MonoBehaviourPun
{
    [Header("Segment Settings")]
    private GameObject[] segmentPrefabs;

    public float moveSpeed = 2f;
    private readonly float spawnX = 20f;
    private readonly float cleanupX = -20f;

    [Header("GameController Reference")]
    [SerializeField] private GameController gameController;

    private bool isFirstSegment;
    private readonly List<GameObject> segmentPool = new();

    private void Start()
    {
        segmentPrefabs = Resources.LoadAll<GameObject>("Segments");
        isFirstSegment = true;
    }

    [System.Obsolete]
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;
        }
        if (!gameController.IsGameStarted()) return;

        SpawnSegments();
        MoveSegments();
        DestroySegments();
    }

    private void SpawnSegments()
    {
        GameObject segmentToSpawn;

        if (isFirstSegment)
        {
            segmentToSpawn = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];
            isFirstSegment = false;
        }
        else
        {
            float xLast = segmentPool[^1].transform.position.x;
            float scaleLast = segmentPool[^1].transform.localScale.x / 2;

            segmentToSpawn = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];

            float distance = spawnX - xLast;
            if (distance <= scaleLast + segmentToSpawn.transform.localScale.x / 2)
                return;
        }

        GameObject segmentInstance = PhotonNetwork.Instantiate($"Segments/{segmentToSpawn.name}", new Vector3(spawnX, 0, 0), Quaternion.identity);
        segmentPool.Add(segmentInstance);

        GhostObstacle[] obstacles = segmentInstance.GetComponentsInChildren<GhostObstacle>();
        foreach (GhostObstacle obstacle in obstacles)
        {
            if (obstacle != null)
            {
                gameController.AssignPlayerToGhostObstacle(obstacle);
            }
        }
    }

    private void MoveSegments()
    {
        foreach (GameObject s in segmentPool)
            s.transform.position += moveSpeed * Time.deltaTime * Vector3.left;
    }

    private void DestroySegments()
    {
        for (int i = segmentPool.Count - 1; i >= 0; i--)
        {
            GameObject s = segmentPool[i];

            if (s.transform.position.x <= cleanupX)
            {
                PhotonNetwork.Destroy(s);
                segmentPool.RemoveAt(i);
            }
        }
    }
}
