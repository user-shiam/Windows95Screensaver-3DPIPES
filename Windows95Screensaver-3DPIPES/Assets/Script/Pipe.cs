using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe
{
    private Material material;
    private Vector3Int currentGridPosition;
    private PipeDirection currentPipeDirection;
    private Quaternion currentRotation;
    private List<PipeSegment> segments = new List<PipeSegment>();
    private PipeManager manager;

    public Pipe(Vector3Int startGridPosition, Material material, PipeManager manager)
    {
        this.manager = manager;
        this.material = material;
        currentGridPosition = startGridPosition;
        currentPipeDirection = (PipeDirection)Random.Range(0, 6);
        currentRotation = Quaternion.FromToRotation(Vector3.up, PipeDirectionToVector3(currentPipeDirection));

      
        manager.OccupiedPositions.Add(currentGridPosition);
        Vector3 position = manager.GridToWorldPosition(currentGridPosition);
        CreateBend(position);

     
        manager.StartCoroutine(Grow());
    }
    // Grow the pipe
    private IEnumerator Grow()
    {
        while (true)
        {
            Vector3Int nextGridPosition = currentGridPosition + PipeDirectionToVector3Int(currentPipeDirection);

            if (!manager.IsPositionValid(nextGridPosition))
            {
                if (!TryChangePipeDirection())
                {
                    yield break;
                }
                continue;
            }
            // Create a segment at the current position

            Vector3 segmentPosition = manager.GridToWorldPosition(currentGridPosition) + PipeDirectionToVector3(currentPipeDirection) * (manager.SegmentLength / 2);
            CreateSegment(segmentPosition);

           
            manager.OccupiedPositions.Add(nextGridPosition);

         
            if (Random.Range(0, 4) == 0) // 25% chance
            {
                Vector3 bendPosition = manager.GridToWorldPosition(nextGridPosition);
                CreateBend(bendPosition);

                if (!TryChangePipeDirection())
                {
                    yield break;
                }
            }

           
            currentGridPosition = nextGridPosition;

            yield return new WaitForSeconds(manager.BuildDelay);
        }
    }
    // Create a segment at the given position
    private void CreateSegment(Vector3 position)
    {
        GameObject segmentObject = Object.Instantiate(manager.PipeSegmentPrefab, position, currentRotation);
        AdjustSegmentScale(segmentObject);
        segmentObject.GetComponent<Renderer>().material = material;
        segments.Add(new PipeSegment(segmentObject));
    }
    // Create a bend at the given position
    private void CreateBend(Vector3 position)
    {
        GameObject bendObject = Object.Instantiate(manager.PipeBendPrefab, position, Quaternion.identity);
        bendObject.transform.localScale = manager.SphereScale;
        bendObject.GetComponent<Renderer>().material = material;
        segments.Add(new PipeSegment(bendObject));
    }
    // Adjust the scale of the segment to match the segment length
    private void AdjustSegmentScale(GameObject segmentObject)
    {
        Vector3 scale = manager.CylinderScale;
        float defaultCylinderHeight = 2.0f; 
        float scaleFactor = manager.SegmentLength / defaultCylinderHeight;
        scale.y *= scaleFactor;
        segmentObject.transform.localScale = scale;
    }
    // Try to change the PipeDirection to a valid one
    private bool TryChangePipeDirection()
    {
        List<PipeDirection> possiblePipeDirections = GetPerpendicularPipeDirections(currentPipeDirection);
        foreach (PipeDirection PipeDirection in possiblePipeDirections)
        {
            Vector3Int nextPosition = currentGridPosition + PipeDirectionToVector3Int(PipeDirection);
            if (manager.IsPositionValid(nextPosition))
            {
                currentPipeDirection = PipeDirection;
                currentRotation = Quaternion.FromToRotation(Vector3.up, PipeDirectionToVector3(currentPipeDirection));
                return true;
            }
        }
        return false;
    }
    // Get all PipeDirections that are perpendicular to the given PipeDirection
    private List<PipeDirection> GetPerpendicularPipeDirections(PipeDirection PipeDirection)
    {
        List<PipeDirection> perpendicularPipeDirections = new List<PipeDirection>();
        foreach (PipeDirection dir in System.Enum.GetValues(typeof(PipeDirection)))
        {
            if (IsPerpendicular(PipeDirection, dir))
            {
                perpendicularPipeDirections.Add(dir);
            }
        }
        return perpendicularPipeDirections;
    }
    // Check if two PipeDirections are perpendicular
    private bool IsPerpendicular(PipeDirection dir1, PipeDirection dir2)
    {
        Vector3Int v1 = PipeDirectionToVector3Int(dir1);
        Vector3Int v2 = PipeDirectionToVector3Int(dir2);
        int dotProduct = v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        return dotProduct == 0;
    }
    // Convert PipeDirection to Vector3
    private Vector3 PipeDirectionToVector3(PipeDirection PipeDirection)
    {
        switch (PipeDirection)
        {
            case PipeDirection.Up: return Vector3.up;
            case PipeDirection.Down: return Vector3.down;
            case PipeDirection.Left: return Vector3.left;
            case PipeDirection.Right: return Vector3.right;
            case PipeDirection.Forward: return Vector3.forward;
            case PipeDirection.Backward: return Vector3.back;
            default: return Vector3.zero;
        }
    }
    // Convert PipeDirection to Vector3Int
    private Vector3Int PipeDirectionToVector3Int(PipeDirection PipeDirection)
    {
        switch (PipeDirection)
        {
            case PipeDirection.Up: return Vector3Int.up;
            case PipeDirection.Down: return Vector3Int.down;
            case PipeDirection.Left: return Vector3Int.left;
            case PipeDirection.Right: return Vector3Int.right;
            case PipeDirection.Forward: return Vector3Int.forward;
            case PipeDirection.Backward: return Vector3Int.back;
            default: return Vector3Int.zero;
        }
    }
    // Destroy old segments that have been around for longer than the lifetime
    public void DestroyOldSegments(float currentTime, float lifetime)
    {
        while (segments.Count > 0 && currentTime - segments[0].CreationTime >= lifetime)
        {
            Object.Destroy(segments[0].GameObject);
            segments.RemoveAt(0);
        }
    }

}