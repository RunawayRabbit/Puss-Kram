using UnityEngine;

public class TurntableCamera : MonoBehaviour
{
    [SerializeField, Range(1.0f, 20.0f)] public float speed = 5.0f;

    void Update()
    {
        transform.Rotate(0.0f, speed * Time.deltaTime, 0.0f);
    }

}

