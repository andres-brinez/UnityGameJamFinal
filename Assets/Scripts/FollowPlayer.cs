using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;        
        //offset = transform.position - playerTransform.position;
        offset = new Vector3(0.8f, 0.24f, -0.7f);
        
    }

 
    void LateUpdate()
    {
        Vector3 desiredPosition = playerTransform.position + playerTransform.TransformDirection(offset);
        transform.position = desiredPosition;

        transform.rotation = playerTransform.rotation;

    }
}
