using UnityEngine;

/// <summary>
/// Simple camera follow for a Roll-a-Ball style game.
/// - Attach to the Camera game object
/// - Assign the player Transform as the target
/// - Configure offset, follow speed and whether the camera should look at the target
/// </summary>
[HelpURL("https://docs.unity3d.com/ScriptReference/Transform.html")]
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Transform to follow (usually the player)")]
    public Transform target;

    [Tooltip("Offset from the target in local space (x:right, y:up, z:back)")]
    public Vector3 offset = new Vector3(0f, 5f, -8f);

    [Tooltip("Follow speed. Higher values follow faster.")]
    [Range(0.01f, 100f)]
    public float followSpeed = 10f;

    [Tooltip("If true, uses SmoothDamp. If false, uses Lerp.")]
    public bool useSmoothDamp = true;

    [Tooltip("Whether the camera should rotate to look at the target")]
    public bool lookAtTarget = true;

    [Tooltip("How fast the camera rotates when looking at the target")]
    [Range(0.01f, 50f)]
    public float rotationSpeed = 8f;

    // Internal velocity used by SmoothDamp
    private Vector3 currentVelocity = Vector3.zero;

    // If camera should interpret offset in target's local space
    [Tooltip("Interpret offset in target's local space (true) or world space (false)")]
    public bool offsetInLocalSpace = true;

    void Reset()
    {
        // sensible defaults when added
        offset = new Vector3(0f, 5f, -8f);
        followSpeed = 10f;
        useSmoothDamp = true;
        lookAtTarget = true;
        rotationSpeed = 8f;
        offsetInLocalSpace = true;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = offsetInLocalSpace ? target.TransformPoint(offset) : target.position + offset;

        if (useSmoothDamp)
        {
            // SmoothDamp uses a smoothing time; convert followSpeed into a reasonable smooth time
            float smoothTime = Mathf.Max(0.0001f, 1f / Mathf.Max(0.0001f, followSpeed));
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }

        if (lookAtTarget)
        {
            Vector3 lookDir = (target.position - transform.position).normalized;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(lookDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}

