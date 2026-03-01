using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SegmentSpawner : MonoBehaviourPun
{
    [Header("Segment Settings")]
    [SerializeField] private GameObject[] segmentPrefabs;  // prefaby segmentów
    [SerializeField] private int poolSize = 5;             // ile segmentów tworzymy w puli
    [SerializeField] private float spawnInterval = 3f;     // odstêp miêdzy segmentami
    [SerializeField] private float spawnX = 10f;           // startowa pozycja X
    [SerializeField] private float cleanupX = -15f;        // pozycja X, przy której segmenty wracaj¹ do puli
    [SerializeField] private float moveSpeed = 2f;         // prêdkoœæ przesuwania segmentów

    [Header("GameController Reference")]
    [SerializeField] private GameController gameController; // przeci¹gnij w Inspectorze

    private float timer = 0f;
    private List<GameObject> segmentPool = new List<GameObject>();
    private int nextSegmentIndex = 0;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Tworzymy pulê segmentów
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = segmentPrefabs[i % segmentPrefabs.Length];
            GameObject segment = PhotonNetwork.Instantiate(prefab.name, new Vector3(-1000, 0, 0), Quaternion.identity);
            segment.SetActive(false);
            segmentPool.Add(segment);
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Sprawdzamy, czy gra wystartowa³a
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;
        }
        if (!gameController.IsGameStarted()) return;

        // Obs³uga spawnowania segmentów
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnSegmentFromPool();
            timer = 0f;
        }

        // Przesuwanie aktywnych segmentów
        MoveSegments();
    }

    private void SpawnSegmentFromPool()
    {
        if (segmentPool.Count == 0) return;

        // Pobieramy nastêpny segment z puli
        GameObject segment = segmentPool[nextSegmentIndex];
        nextSegmentIndex = (nextSegmentIndex + 1) % segmentPool.Count;

        segment.transform.position = new Vector3(spawnX, 0f, 0f);
        segment.SetActive(true);
    }

    private void MoveSegments()
    {
        foreach (GameObject segment in segmentPool)
        {
            if (!segment.activeInHierarchy) continue;

            segment.transform.position += Vector3.left * moveSpeed * Time.deltaTime;

            if (segment.transform.position.x < cleanupX)
            {
                segment.SetActive(false); // zwracamy do puli
            }
        }
    }
}
