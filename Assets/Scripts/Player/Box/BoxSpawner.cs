using System;
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


        if (InputManager.WasQuickSpawnActionPressed && currentGhostBox == null)
        {
            Vector3 spawnPos = transform.position + quickSpawnOffset;
            TrySpawnBox(spawnPos); // Senin ayrı fonksiyonun varsa burayı kullan
        }

        if (InputManager.WasSpawnActionPressed && currentGhostBox == null)
        {

            PlaceTheGhostBox();
        }
        else if (InputManager.WasSpawnActionPressed && currentGhostBox != null)
        {
            ghostSpawnPos = currentGhostBox.transform.position;
            TrySpawnBox(ghostSpawnPos);

            Destroy(currentGhostBox);
            currentGhostBox = null;
        }


        if (currentBox != null)
        {
            ElevatorBox box = currentBox.GetComponent<ElevatorBox>();
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                box.StartMoving(Vector3.up);
                currentBox = null;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                box.StartMoving(Vector3.down);
                currentBox = null;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                box.StartMoving(Vector3.left);
                currentBox = null;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                box.StartMoving(Vector3.right);
                currentBox = null;
            }
        }






        if (ReturnNewestBox() != null)
        {
            GameObject newestBox = ReturnNewestBox();
            if (InputManager.WasBoostBoxActionPressed)
            {
                newestBox.GetComponent<ElevatorBox>().SetState(new BoxSpeedingState());
            }
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

    public void TrySpawnBox(Vector3 spawnPos)
    {
        if (currentBox != null) return;

        BoxCollider boxCollider = elevatorBoxPrefab.GetComponent<BoxCollider>();

        Vector3 updatedPos = new Vector3(0, boxCollider.bounds.extents.y + 0.05f, 0) + spawnPos;
        GameObject newBox = Instantiate(elevatorBoxPrefab, updatedPos, Quaternion.identity);


        activeBoxes.Enqueue(newBox);


        if (activeBoxes.Count > maxBoxes)
        {
            GameObject oldestBox = activeBoxes.Dequeue();
            if (oldestBox != null)
            {
                player.transform.SetParent(null); // Oyuncuyu ayır

                Destroy(oldestBox);
            }


        }

        currentBox = newBox;
    }











}
