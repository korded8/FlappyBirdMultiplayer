using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;

public class SegmentController : MonoBehaviourPun
{
    [Header("Segment Settings")]
    [SerializeField] private GameObject[] segmentPrefabs;
    [SerializeField] private int poolSize = 5;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnX = 10f;
    [SerializeField] private float cleanupX = -15f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("GameController Reference")]
    [SerializeField] private GameController gameController;

    private float timer = 0f;
    private List<GameObject> segmentPool = new List<GameObject>();

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
        for (int i = 0; i < segmentPrefabs.Length; i++)
        {
            if (i == 0)
            {
                GameObject firstSegment = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];

                PhotonNetwork.Instantiate(firstSegment.name, new Vector3(spawnX, 0), Quaternion.identity);
                segmentPool.Add(firstSegment);
            }
            else
            {
                float xScale = segmentPool[i - 1].transform.localScale.x / 2;
                float x = segmentPool[i - 1].transform.position.x;
                GameObject segment = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];

                if (segment.transform.position.x - x >= segment.transform.localScale.x / 2 + xScale)
                {
                    PhotonNetwork.Instantiate(segment.name, new Vector3(spawnX, 0), Quaternion.identity);
                    segmentPool.Add(segment);
                }
            }
        }
    }

    private void MoveSegments()
    {
        foreach (GameObject s in segmentPool)
        {
            s.transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }
    }

    private void DestroySegments()
    {
        foreach (GameObject s in segmentPool)
        {
            if (s.transform.position == new Vector3(cleanupX, 0))
            {
                segmentPool.Remove(s);
                Destroy(s);
            }
        }
    }
}