using UnityEngine;

public class BushController : MonoBehaviour
{
    [Header("Settings")]
    public float followSpeed = 5f;
    public float returnSpeed = 2f;
    public float maxRotationAngle = 15f;
    public float reactionDistance = 2f;

    private Transform target;
    private Quaternion initialRot;

    void Start() 
    {
        GameObject found = GameObject.FindGameObjectWithTag("BushInteractor");
        if (found != null)
            target = found.transform;

        initialRot = transform.localRotation;
    }

    void Update() 
    {
        if (target == null)
            return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        float distance = dir.magnitude;

        if (distance < reactionDistance) 
        {
            Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            Quaternion limitedRot = Quaternion.Slerp(initialRot, lookRot, distance / reactionDistance);
            transform.rotation = Quaternion.Slerp(transform.rotation, limitedRot, Time.deltaTime * followSpeed);
        } 
        else 
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRot, Time.deltaTime * returnSpeed);
        }
    }
}
