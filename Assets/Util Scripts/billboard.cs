using UnityEngine;

public class billboard : MonoBehaviour
{
    private Camera c;
    private void Awake()
    {
        if(!c) c = Camera.main;
    }
    private void LateUpdate()
    {
        transform.LookAt(transform.position + c.transform.rotation * Vector3.forward, Vector3.up);
    }
}
