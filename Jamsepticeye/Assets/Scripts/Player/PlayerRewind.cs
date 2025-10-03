using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRewind : MonoBehaviour
{
    [System.Serializable]
    public class PositionRecord
    {
        public Vector2 position;
        public float timestamp;
    }

    [Header("Rewind Settings")]
    public float rewindDuration = 3f;
    public float recordInterval = 0.05f;
    public float rewindSpeed = 5f;

    [Header("Collision Check")]
    public LayerMask collisionMask;
    public float collisionCheckRadius = 0.2f;

    [Header("VFX")]
    public GameObject rewindParticlePrefab;
    
    [Header("Sprite Stuff")]
    [SerializeField] private Sprite normalSprite;     
    [SerializeField] private Sprite rewindSprite;    
    
    private SpriteRenderer spriteRenderer;
    private List<PositionRecord> positionHistory = new List<PositionRecord>();
    private float recordTimer;
    private bool isRewinding = false;
    private Rigidbody2D rb;
    private Collider2D col;

    // Input Action
    private InputAction rewindAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Temp InputAction for "Rewind" (press E by default)
        rewindAction = new InputAction("Rewind", InputActionType.Button, "<Keyboard>/e");
        rewindAction.performed += ctx => OnRewindPressed();
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = normalSprite;
    }

    void OnEnable()
    {
        rewindAction.Enable();
    }

    void OnDisable()
    {
        rewindAction.Disable();
    }

    void Update()
    {
        if (!isRewinding)
            RecordPositions();
    }

    void RecordPositions()
    {
        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            positionHistory.Add(new PositionRecord { position = transform.position, timestamp = Time.time });

            // This is here to make sure we have the extra position data if the 3 second rewind is not valid 
            while (positionHistory.Count > 0 && Time.time - positionHistory[0].timestamp > rewindDuration + 2f)
            {
                positionHistory.RemoveAt(0);
            }
        }
    }

    public void OnRewindPressed()
    {
        if (!isRewinding)
            StartCoroutine(Rewind());
    }

    System.Collections.IEnumerator Rewind()
{
    spriteRenderer.sprite = rewindSprite;
    isRewinding = true; // this makes sure that the bool is flipped and its not gonna be stuck in a position setting loop 
    rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    col.enabled = false;

    GameObject particle = null; // for now it will just Instantiate but if we have more time it would be nice to make a particle for this 
    if (rewindParticlePrefab != null)
        particle = Instantiate(rewindParticlePrefab, transform.position, Quaternion.identity, transform);

    //Find ~3s ago position (safe)
    Vector2? targetPos = null;
    for (int i = positionHistory.Count - 1; i >= 0; i--)
    {
        if (Time.time - positionHistory[i].timestamp >= rewindDuration)
        {
            if (!Physics2D.OverlapCircle(positionHistory[i].position, collisionCheckRadius, collisionMask))
            {
                // make sure its not colideing with anything, but its limited to layer mask
                targetPos = positionHistory[i].position;
                break;//this breakpoint will exit the loop
            }
        }
    }

    // If none valid, go further back
    if (targetPos == null)
    {
        for (int i = positionHistory.Count - 1; i >= 0; i--) // we remove the rewindDuration limitation here so it can acces the full list 
        {
            if (!Physics2D.OverlapCircle(positionHistory[i].position, collisionCheckRadius, collisionMask))
            {
                targetPos = positionHistory[i].position;
                break;
            }
        }
    }

    // Lerp through recorded positions, we are grabbing all the positions between the current and the target then reversing it 
    if (targetPos != null)
    {
        List<Vector2> rewindPath = new List<Vector2>();
        foreach (var record in positionHistory)
        {
            if (record.timestamp >= Time.time - rewindDuration && record.timestamp <= Time.time)
                rewindPath.Add(record.position);
        }

        rewindPath.Reverse();

        // instead of lerping each small segment separately, we lerp smoothly across the whole path
        float totalTime = rewindPath.Count * (1f / rewindSpeed); 
        float elapsed = 0f;

        while (elapsed < totalTime && rewindPath.Count > 1)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / totalTime);

            // find where we are between two recorded positions
            float pathPos = progress * (rewindPath.Count - 1);
            int fromIndex = Mathf.FloorToInt(pathPos);
            int toIndex = Mathf.Clamp(fromIndex + 1, 0, rewindPath.Count - 1);
            float t = pathPos - fromIndex;

            // smoothstep makes the motion much less jerky, but its still pretty jerky 
            Vector2 smoothPos = Vector2.Lerp(rewindPath[fromIndex], rewindPath[toIndex], Mathf.SmoothStep(0f, 1f, t));
            transform.position = smoothPos;

            yield return null;
        }

        // Final snap to safe target
        transform.position = targetPos.Value;
    }

    // unfuck everything 
    if (particle != null) Destroy(particle);
    rb.isKinematic = false;
    col.enabled = true;
    isRewinding = false;
    spriteRenderer.sprite = normalSprite;
}


    /*
     void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
    }
    */
}
