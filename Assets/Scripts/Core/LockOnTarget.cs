using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    [SerializeField] private Transform lockPoint;

    public Transform LockPoint => lockPoint != null ? lockPoint : transform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
