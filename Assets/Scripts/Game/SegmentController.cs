using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SegmentController : MonoBehaviourPun
{
    [Header("Segment Settings")]
    private GameObject[] easySegmentPrefabs;
    private GameObject[] acceleratedSegmentPrefabs;
    private GameObject[] mediumSegmentPrefabs;
    private GameObject[] hardSegmentPrefabs;

    private readonly float spawnX = 20f;
    private readonly float cleanupX = -20f;
    private readonly int MAX_SEGMENTS = 4;

    [Header("GameController Reference")]
    [SerializeField] private GameController gameController;

    private readonly List<GameObject> segmentPool = new();
    private double gameStartTime = -1;
    private GameObject lastSegment = null;

    private void Start()
    {
        easySegmentPrefabs = Resources.LoadAll<GameObject>("Segments/Easy");
        acceleratedSegmentPrefabs = Resources.LoadAll<GameObject>("Segments/Accelerated");
        mediumSegmentPrefabs = Resources.LoadAll<GameObject>("Segments/Medium");
        hardSegmentPrefabs = Resources.LoadAll<GameObject>("Segments/Hard");
    }

    [System.Obsolete]
    private void Update()
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;
        }

        if (!gameController.IsGameStarted() || !PhotonNetwork.InRoom || gameController.isLeaving) return;

        if (gameStartTime == -1)
            gameStartTime = PhotonNetwork.Time;

        if (!PhotonNetwork.IsMasterClient) return;

        SpawnSegments();
        DestroySegments();
    }

    private void SpawnSegments()
    {
        int existingSegments = segmentPool.Count;

        if (existingSegments >= MAX_SEGMENTS)
            return;

        GameObject nextSegment = null;

        switch (gameController.currDiffLvl)
        {
            case GameController.DifficultyLevel.Easy:
                nextSegment = easySegmentPrefabs[Random.Range(0, easySegmentPrefabs.Length)];
                break;
            case GameController.DifficultyLevel.Accelerated:
                nextSegment = acceleratedSegmentPrefabs[Random.Range(0, acceleratedSegmentPrefabs.Length)];
                break;
            case GameController.DifficultyLevel.Medium:
                nextSegment = mediumSegmentPrefabs[Random.Range(0, mediumSegmentPrefabs.Length)];
                break;
            case GameController.DifficultyLevel.Hard:
                nextSegment = hardSegmentPrefabs[Random.Range(0, hardSegmentPrefabs.Length)];
                break;

        }
        float nextSegmentPosition = spawnX;

        if (lastSegment != null)
            nextSegmentPosition = lastSegment.transform.position.x + (lastSegment.transform.localScale.x + nextSegment.transform.localScale.x) / 2;

        double spawnTime = PhotonNetwork.Time;

        GameObject segmentInstance = PhotonNetwork.Instantiate(
            "Segments/" + gameController.currDiffLvl + "/" + nextSegment.name,
            new Vector3(nextSegmentPosition, 0, 0),
            Quaternion.identity
        );

        segmentPool.Add(segmentInstance);
        lastSegment = segmentInstance;

        GhostObstacle[] obstacles = segmentInstance.GetComponentsInChildren<GhostObstacle>();
        foreach (GhostObstacle obstacle in obstacles)
            if (obstacle != null)
                gameController.AssignPlayerToGhostObstacle(obstacle);
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