using UnityEngine;

public class PipeSegment
{
    public GameObject GameObject { get; private set; }
    public float CreationTime { get; private set; }

    public PipeSegment(GameObject gameObject)
    {
        GameObject = gameObject;
        CreationTime = Time.time;
    }
}