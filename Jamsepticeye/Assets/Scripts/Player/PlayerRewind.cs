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

[Header("Corpse Prefab")]
[SerializeField] GameObject _corpse;

[Header("Rewind Preview")]
[SerializeField] private GameObject rewindGhostPrefab; // ghost indicator prefab

[Header("Safe Zone Prefab")]
[SerializeField] private GameObject safeZonePrefab; // prefab for safe zone marker
[SerializeField] private float safeZoneMoveSpeed = 5f; // how quickly the safe zone slides

private SpriteRenderer spriteRenderer;
private List<PositionRecord> positionHistory = new List<PositionRecord>();
private float recordTimer;
private bool isRewinding = false;
private Rigidbody2D rb;
private Collider2D col;

// Input Action
private InputAction rewindAction;

// ghost instance
private GameObject rewindGhostInstance;

// Safe zone system
private Vector2? confirmedSafeZonePosition = null; // last survived safe zone
private Vector2? pendingSafeZonePosition = null; // preview safe zone waiting for confirmation
private GameObject safeZoneInstance = null; // visual marker
private float lastRewindTime;
private bool hasSurvivedRewind = false;

// Water check
private bool _inWater = false;

void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    col = GetComponent<Collider2D>();

    // Temp InputAction for "Rewind" (press E by default)
    //rewindAction = new InputAction("Rewind", InputActionType.Button, "<Keyboard>/e");
    //rewindAction.performed += ctx => OnRewindPressed();

    spriteRenderer = GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = normalSprite;

    // spawn ghost if prefab assigned
    if (rewindGhostPrefab != null)
    {
        rewindGhostInstance = Instantiate(rewindGhostPrefab, transform.position, Quaternion.identity);
        rewindGhostInstance.SetActive(false);
    }

    // spawn safe zone instance at start if prefab assigned
    if (safeZonePrefab != null)
    {
        safeZoneInstance = Instantiate(safeZonePrefab, transform.position, Quaternion.identity);
        safeZoneInstance.SetActive(false);
    }
}

//void OnEnable() { rewindAction.Enable(); }
//void OnDisable() { rewindAction.Disable(); }

void Update()
{
    if (!isRewinding) RecordPositions();

    // update ghost position while not rewinding
    if (!isRewinding && rewindGhostInstance != null)
    {
        // Only show ghost if there is enough data (at least 1s recorded)
        bool hasEnoughData = positionHistory.Count > 0 && (Time.time - positionHistory[0].timestamp >= 1f);
        Vector2? previewPos = hasEnoughData ? GetRewindPreviewPosition() : null;

        if (previewPos != null)
        {
            rewindGhostInstance.SetActive(true);
            rewindGhostInstance.transform.position = previewPos.Value;
        }
        else
        {
            rewindGhostInstance.SetActive(false);
        }
    }

    // Smoothly move safe zone marker if it exists and has a target
    if (safeZoneInstance != null && pendingSafeZonePosition != null)
    {
        safeZoneInstance.transform.position = Vector2.Lerp(
            safeZoneInstance.transform.position,
            pendingSafeZonePosition.Value,
            Time.deltaTime * safeZoneMoveSpeed
        );
    }

    // Safe zone check: survived 1 second after rewind
    if (!isRewinding && pendingSafeZonePosition != null && !hasSurvivedRewind && Time.time - lastRewindTime >= 1f)
    {
        // only confirm safe zone if not in water
        if (!_inWater)
        {
            confirmedSafeZonePosition = pendingSafeZonePosition;
            hasSurvivedRewind = true;
            Debug.Log("Safe zone CONFIRMED at: " + confirmedSafeZonePosition.Value);

            if (safeZoneInstance != null)
                safeZoneInstance.SetActive(true); // show the marker after surviving
        }
        else
        {
            Debug.Log("Safe zone NOT confirmed: player is in water");
            pendingSafeZonePosition = null;

            // keep the old confirmed safe zone instance (don’t destroy it)
            if (safeZoneInstance != null)
                safeZoneInstance.SetActive(false);
        }
    }
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
    // Only allow manual rewind if we have at least 1 second of recorded data
    // AND the ghost preview has a valid position
    if (!isRewinding)
    {
        // check if we have enough data recorded
        bool hasEnoughData = positionHistory.Count > 0 && (Time.time - positionHistory[0].timestamp >= 1f);

        // check if ghost has a valid position
        Vector2? previewPos = GetRewindPreviewPosition();

        if (hasEnoughData && previewPos != null)
        {
            StartCoroutine(Rewind());

            //corpse (only spawn if rewind is actually happening)
            Death corpse = Instantiate(_corpse, transform.position, Quaternion.identity).GetComponent<Death>();

            if (CardManager.Instance != null)
            {
                corpse.CurrentUpgrade = CardManager.Instance.SelectedCard;
            }

            // clear history on death
            positionHistory.Clear();

            // reset survival tracker
            hasSurvivedRewind = false;
            lastRewindTime = Time.time;
        }
        else
        {
            Debug.Log("Cannot rewind: not enough history data or no valid ghost position.");
        }
    }
}

// calculates preview rewind position (used by ghost)
private Vector2? GetRewindPreviewPosition()
{
    for (int i = positionHistory.Count - 1; i >= 0; i--)
    {
        if (Time.time - positionHistory[i].timestamp >= rewindDuration)
        {
            if (!Physics2D.OverlapCircle(positionHistory[i].position, collisionCheckRadius, collisionMask))
                return positionHistory[i].position;
        }
    }

    for (int i = positionHistory.Count - 1; i >= 0; i--)
    {
        if (!Physics2D.OverlapCircle(positionHistory[i].position, collisionCheckRadius, collisionMask))
            return positionHistory[i].position;
    }

    return null;
}

System.Collections.IEnumerator Rewind()
{
    spriteRenderer.sprite = rewindSprite;
    isRewinding = true;
    // this makes sure that the bool is flipped and its not gonna be stuck in a position setting loop
    rb.linearVelocity = Vector2.zero;
    rb.bodyType = RigidbodyType2D.Kinematic;
    col.enabled = false;

    // hide preview ghost while rewinding
    if (rewindGhostInstance != null)
        rewindGhostInstance.SetActive(false);

    GameObject particle = null;
    // for now it will just Instantiate but if we have more time it would be nice to make a particle for this
    if (rewindParticlePrefab != null)
        particle = Instantiate(rewindParticlePrefab, transform.position, Quaternion.identity, transform);

    // target position calculation
    Vector2? targetPos = null;

    // If you die within 2s, always return to the last confirmed safe zone
    if (confirmedSafeZonePosition != null && Time.time - lastRewindTime < 2f)
    {
        targetPos = confirmedSafeZonePosition;
        Debug.Log("Rewind using CONFIRMED safe zone!");
    }
    else
    {
        //Find ~3s ago position (safe)
        for (int i = positionHistory.Count - 1; i >= 0; i--)
        {
            if (Time.time - positionHistory[i].timestamp >= rewindDuration)
            {
                if (!Physics2D.OverlapCircle(positionHistory[i].position, collisionCheckRadius, collisionMask))
                {
                    // make sure its not colliding with anything, but its limited to layer mask
                    targetPos = positionHistory[i].position;
                    break; //this breakpoint will exit the loop
                }
                else { Debug.Log("Collision Detected"); }
            }
        }

        // If none valid, go further back
        if (targetPos == null)
        {
            for (int i = positionHistory.Count - 1; i >= 0; i--) // we remove the rewindDuration limitation here so it can access the full list
            {
                if (!Physics2D.OverlapCircle(positionHistory[i].position, collisionCheckRadius, collisionMask))
                {
                    targetPos = positionHistory[i].position;
                    break;
                }
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

        // --- Safe zone assignment moved here ---
        if (!_inWater)
        {
            pendingSafeZonePosition = targetPos.Value;
            hasSurvivedRewind = false; // reset, will be confirmed after 1s
            lastRewindTime = Time.time;

            Debug.Log("Safe zone PREVIEW set at rewind position: " + pendingSafeZonePosition.Value);

            // spawn marker once at start, then lerp it toward new pos
            if (safeZoneInstance != null)
            {
                safeZoneInstance.SetActive(true); // keep visible but moving
            }
        }
    }

    // unfuck everything
    if (particle != null) Destroy(particle);
    rb.bodyType = RigidbodyType2D.Dynamic;
    col.enabled = true;
    isRewinding = false;
    spriteRenderer.sprite = normalSprite;
    positionHistory.Clear();
    hasSurvivedRewind = false;
    lastRewindTime = Time.time;
}

/*
void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
}
*/

// --- Water trigger detection ---
private void OnTriggerEnter2D(Collider2D collision)
{
    Debug.Log("In Water");
    // does a check for if it's in water
    if(collision.gameObject.layer == LayerMask.NameToLayer("Water"))
    {
        _inWater = true;
    }
}

private void OnTriggerExit2D(Collider2D collision)
{
    if(collision.gameObject.layer == LayerMask.NameToLayer("Water"))
    {
        _inWater = false;
    }
}
// Call this from hazards, enemies, etc.
public void Die()
{
    if (!isRewinding)
    {
        Debug.Log("Player died, initiating rewind...");

        // spawn corpse
        Death corpse = Instantiate(_corpse, transform.position, Quaternion.identity).GetComponent<Death>();
        if (CardManager.Instance != null)
        {
            corpse.CurrentUpgrade = CardManager.Instance.SelectedCard;
        }

        // force rewind to safe zone if possible
        StartCoroutine(Rewind());

        // clear history on death
        positionHistory.Clear();

        // reset survival tracker
        hasSurvivedRewind = false;
        lastRewindTime = Time.time;
    }
}

// only god knows at this point how this script works, once so did I - Manvir  
}
