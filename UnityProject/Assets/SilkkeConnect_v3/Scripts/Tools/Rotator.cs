using UnityEngine;

public class Rotator : MonoBehaviour
{
    public bool activated = true;
    public Vector3 rotationVector;
    public float rotationSpeed = 2.0f;

    void Update()
    {
        if (activated)
            transform.Rotate(rotationVector * rotationSpeed * Time.fixedDeltaTime);
    }
}
