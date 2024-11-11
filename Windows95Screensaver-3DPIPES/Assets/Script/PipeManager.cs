using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{

    public GameObject PipeSegmentPrefab;
    public GameObject PipeBendPrefab;
    public Material PipeMaterial;


    public int GridSize = 10;
    public float SegmentLength = 1.0f;
    public float BuildDelay = 0.05f;
    public float PipeLifetime = 60f;


    public Vector3 CylinderScale = new Vector3(0.1f, 1f, 0.1f);
    public Vector3 SphereScale = new Vector3(0.2f, 0.2f, 0.2f);

    public HashSet<Vector3Int> OccupiedPositions { get; private set; } = new HashSet<Vector3Int>();
    private List<Pipe> pipes = new List<Pipe>();

    void Start()
    {
        
        PipeSegmentPrefab.transform.localScale = CylinderScale;
        PipeBendPrefab.transform.localScale = SphereScale;

        StartCoroutine(GeneratePipes());
    }

    void Update()
    {
        // Destroy old pipes
        float currentTime = Time.time;
        foreach (Pipe pipe in pipes)
        {
            pipe.DestroyOldSegments(currentTime, PipeLifetime);
        }
    }
    // Generate pipes
    private IEnumerator GeneratePipes()
    {
        while (true)
        {
            Vector3Int startGridPosition = new Vector3Int(
                Random.Range(-GridSize, GridSize),
                Random.Range(-GridSize, GridSize),
                Random.Range(-GridSize, GridSize)
            );

            if (OccupiedPositions.Contains(startGridPosition))
            {
                yield return null;
                continue;
            }

         
            Material materialInstance = new Material(PipeMaterial);
            materialInstance.color = Random.ColorHSV();

          
            Pipe newPipe = new Pipe(startGridPosition, materialInstance, this);
            pipes.Add(newPipe);

            yield return new WaitForSeconds(5);
        }
    }
    // Check if the position is valid
    public bool IsPositionValid(Vector3Int gridPosition)
    {
        return Mathf.Abs(gridPosition.x) <= GridSize &&
               Mathf.Abs(gridPosition.y) <= GridSize &&
               Mathf.Abs(gridPosition.z) <= GridSize &&
               !OccupiedPositions.Contains(gridPosition);
    }
    // Convert PipeDirection to Vector3Int
    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        return new Vector3(gridPosition.x * SegmentLength, gridPosition.y * SegmentLength, gridPosition.z * SegmentLength);
    }
}

