﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BoxSpawner : MonoBehaviour
{
    public static BoxSpawner instance;

    [SerializeField] private GameObject elevatorBoxPrefab;
    [SerializeField] private GameObject elevatorGhostBoxPrefab;
    [SerializeField] private int spawnOffsetX = 1;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private int maxBoxes = 3;
    [SerializeField] private float stepSize = 1;
    [SerializeField] private GameObject player;
    [SerializeField, Tooltip("Quick Spawn özelliğinin kutuyu oyuncuya göre ne kadar uzakta yaratacağı")] private Vector3 quickSpawnOffset;

    private Queue<GameObject> activeBoxes = new Queue<GameObject>();

    private GameObject currentBox;
    private GameObject currentGhostBox;
    private Vector3 ghostSpawnPos;
    private Vector2 moveValue;

    private LayerMask mask;
    private Vector3 currentMoveDirection = Vector3.zero;
    private bool hasGivenDirection = false;
    private bool cancelled = false;

    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }

        mask = ~(1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Elevator")) | (1 << LayerMask.NameToLayer("IgnorePlayer"));
    }

    void Update()
    {


        /*if (InputManager.WasQuickSpawnActionPressed && currentGhostBox == null)
        {
            Vector3 spawnPos = transform.position + quickSpawnOffset;
            TrySpawnBox(spawnPos); // Senin ayrı fonksiyonun varsa burayı kullan
        }*/

        if (InputManager.WasSpawnActionPressed && currentGhostBox == null)
        {

            PlaceTheGhostBox();
        }
        else if (InputManager.WasSpawnActionPressed && currentGhostBox != null && currentMoveDirection != null)
        {
            ghostSpawnPos = currentGhostBox.transform.position;
            StartCoroutine(TrySpawnBox(ghostSpawnPos));
        }


        if (Input.GetKeyDown(KeyCode.Escape) && currentGhostBox != null)
        {
            Destroy(currentGhostBox);
            currentGhostBox = null;
        }






        if (InputManager.WasBoostBoxActionPressed)
        {
            BoostClosestBoxToPlayer();
        }
    }




    public void PlaceTheGhostBox()
    {
        if (currentGhostBox != null) return;

        Vector3 spawnPos = player.transform.position + new Vector3(3, 1, 0);
        GameObject ghostBox = Instantiate(elevatorGhostBoxPrefab, spawnPos, Quaternion.identity);

        currentGhostBox = ghostBox;

    }



    public GameObject ReturnNewestBox()
    {
        if (activeBoxes.Count > 0)
        {
            GameObject[] boxArray = activeBoxes.ToArray();
            GameObject newestBox = boxArray[boxArray.Length - 1];
            return newestBox;
        }
        else
        {
            print("kutu yok");
            return null;
        }
    }

    public IEnumerator TrySpawnBox(Vector3 spawnPos)
    {
        yield return StartCoroutine(GiveDirectionBeforeSpawn());

        if (cancelled)
        {
            Debug.Log("Spawn işlemi iptal edildi.");
            Destroy(currentGhostBox);
            currentGhostBox = null;
            yield break;
        }

        BoxCollider boxCollider = elevatorBoxPrefab.GetComponent<BoxCollider>();
        Vector3 updatedPos = new Vector3(0, boxCollider.bounds.extents.y + 0.05f, 0) + spawnPos;

        GameObject newBox = Instantiate(elevatorBoxPrefab, updatedPos, Quaternion.identity);
        activeBoxes.Enqueue(newBox);

        if (activeBoxes.Count > maxBoxes)
        {
            GameObject oldestBox = activeBoxes.Dequeue();
            if (oldestBox != null)
            {
                Destroy(oldestBox);
            }
        }

        ElevatorBox elevatorBox = newBox.GetComponent<ElevatorBox>();
        elevatorBox.StartMoving(currentMoveDirection);

        currentBox = newBox;
        currentMoveDirection = Vector3.zero;
        Destroy(currentGhostBox);
        currentGhostBox = null;
        currentBox = null;
    }

    private IEnumerator GiveDirectionBeforeSpawn()
    {
        hasGivenDirection = false;
        cancelled = false;

        while (!hasGivenDirection && !cancelled)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentMoveDirection = Vector3.up;
                hasGivenDirection = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentMoveDirection = Vector3.down;
                hasGivenDirection = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentMoveDirection = Vector3.left;
                hasGivenDirection = true;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentMoveDirection = Vector3.right;
                hasGivenDirection = true;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                cancelled = true;
            }

            yield return null; // Bir sonraki frame'e kadar bekle
        }
    }


    private void BoostClosestBoxToPlayer()
    {
        if (activeBoxes.Count == 0) return;

        GameObject closestBox = null;
        float minDistance = Mathf.Infinity;
        Vector3 playerPos = player.transform.position;

        foreach (GameObject box in activeBoxes)
        {
            if (box == null) continue;

            float distance = Vector3.Distance(playerPos, box.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestBox = box;
            }
        }

        if (closestBox != null)
        {
            ElevatorBox elevatorBox = closestBox.GetComponent<ElevatorBox>();
            if (elevatorBox != null)
            {
                elevatorBox.SetState(new BoxSpeedingState());
                Debug.Log("Boosted box: " + closestBox.name);
            }
        }
    }













}
