using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SegmentController : MonoBehaviourPun
{
    [Header("Segment Settings")]
    private GameObject[] segmentPrefabs;

    public float moveSpeed = 2f;
    public float acceleration = 0.07f;
    private readonly float spawnX = 20f;
    private readonly float cleanupX = -20f;
    private readonly int MAX_SEGMENTS = 4;
    private readonly int MAX_SPEED = 4;

    [Header("GameController Reference")]
    [SerializeField] private GameController gameController;

    private readonly List<GameObject> segmentPool = new();
    private double gameStartTime = -1;
    private GameObject lastSegment = null;

    private void Start()
    {
        segmentPrefabs = Resources.LoadAll<GameObject>("Segments");
    }

    [System.Obsolete]
    private void Update()
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;
        }

        if (!gameController.IsGameStarted()) return;

        if (gameStartTime == -1)
            gameStartTime = PhotonNetwork.Time;

        double elapsed = PhotonNetwork.Time - gameStartTime;

        if (elapsed > 30 && moveSpeed <= MAX_SPEED)
            moveSpeed += acceleration * Time.deltaTime;

        if (moveSpeed > MAX_SPEED) moveSpeed = MAX_SPEED;

        if (!PhotonNetwork.IsMasterClient) return;

        SpawnSegments();
        DestroySegments();
    }

    private void SpawnSegments()
    {
        int existingSegments = segmentPool.Count;

        if (existingSegments >= MAX_SEGMENTS)
            return;

        GameObject nextSegment = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];
        float nextSegmentPosition = spawnX;

        if (lastSegment != null)
            nextSegmentPosition = lastSegment.transform.position.x + (lastSegment.transform.localScale.x + nextSegment.transform.localScale.x) / 2;

        double spawnTime = PhotonNetwork.Time;

        GameObject segmentInstance = PhotonNetwork.Instantiate(
            $"Segments/{nextSegment.name}",
            new Vector3(nextSegmentPosition, 0, 0),
            Quaternion.identity,
            0
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