using UnityEngine;

public class TorqueTest : MonoBehaviour
{
    [SerializeField]
    [Header("* 3.14] 1round 2PI,velocity is 2")]
    [Header("ANG_VEROCITY rad/s ")]
    public float AngularVelocity;

    [SerializeField]
    [Header("VEROCITY m/s ")]
    Vector3 Velocity;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = float.PositiveInfinity;
        // StartCoroutine(nameof(Move),new Vector3(0, 1, 0));
        rb.velocity = Velocity;
        rb.angularVelocity = new Vector3(AngularVelocity, 0, 0) * Mathf.PI; // rad/s    1 round: 2PI     2round: 4PI
    }

    // Update is called once per frame
    void Update()
    {
    }
}
