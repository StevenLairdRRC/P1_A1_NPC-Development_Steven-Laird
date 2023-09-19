using UnityEngine;

public class NPC_V4 : MonoBehaviour
{
    [Header("NPC Speed Settings")]
    public float wanderSpeed = 1f;                          // Speed when wandering
    public float followingSpeed = 1.5f;                     // Speed when following the player
    public float targetingSpeed = 0.5f;                     // Speed when targeting the player

    [Header("NPC Range Settings")]
    public float randomWaypointSize = 0.5f;                 // Size of the random waypoint for wandering
    public float wanderRange = 25f;                         // Maximum distance for wandering
    public float detectionRange = 20f;                      // Range for detecting the player
    public float abilityRange =10f;                         // Range for using the ability
    public bool showDetectionRangeRadius = true;            // Show detection range radius in the game
    public bool showAbilityRangeRadius = true;              // Show ability range radius in the game
    private GameObject detectionRangeObject;                // Reference to the detection range object
    private GameObject abilityRangeObject;                  // Reference to the ability range object

    [Header("NPC Rotation Settings")]
    public RotationStyle rotationStyle;                     // Rotation style enumeration
    public enum RotationStyle
    {
        smooth, snap, none
    }
    public float wanderRotationSpeed = 20f;                 // Rotation speed while wandering
    public float followRotationSpeed = 25f;                 // Rotation speed while following
    public float directVisionAngleThreshold = 5f;           // Angle threshold for direct vision

    [Header("NPC Target & Ability Settings")]
    public Player_Movement_V4 targetPlayer;                 // Reference to the player
    public GameObject abilityPrefab;                        // Prefab for the NPC's ability
    public float abilitySpeed = 6f;                         // Speed of the NPC's ability
    public float abilityCooldownTime = 3f;                  // Cooldown time for using the ability


    [Header("Debug Info")]
    public bool enableDestinationRay = true;                // Enable debugging ray for destination
    public bool enableWanderRangeAndWaypoints = true;       // Enable debugging visuals for wander range and waypoints
    public bool enableDetectionAndAbilityRange = true;      // Enable debugging visuals for detection and ability range

    public float currentSpeed;                              // Current speed of the NPC
    public float currentRotationSpeed;                      // Current rotation speed of the NPC
    public float abilityTimer;                              // Timer for the ability cooldown
    public bool targetInDirectVision;                       // Flag indicating if the target is in direct vision

    private Vector2 originPosition;                         // Starting position of the NPC
    private Vector2 randomWaypoint;                         // Random waypoint for wandering
    private Vector2 currentDestination;                     // Current destination of the NPC
    private SpriteRenderer spriteColor;                     // Reference to the NPC's sprite renderer
                                                            // Color defining range state;
                                                            // wander(green), follow(yellow), ability(red)


    void Start()
    {
        // Initialize detection and ability range objects as the children of the NPC GameObject
        detectionRangeObject = gameObject.transform.GetChild(0).gameObject;
        abilityRangeObject = gameObject.transform.GetChild(1).gameObject;
        // Initialize spriteColor with SpriteRenderer for cleaner code
        spriteColor = GetComponent<SpriteRenderer>();
        // Find and initialize the targetPlayer using it's script
        targetPlayer = FindAnyObjectByType<Player_Movement_V4>();
        // Initialize origin position for npc's wander radius

        originPosition = (Vector2)transform.localPosition;
        // Call SetNewDestination to select an initial random waypoint/destination within wander radius
        SetNewDestination();
        // Reset ability timer so the ability doesn't get used immediately when in range of using an ability
        abilityTimer = abilityCooldownTime;
        // Initialze targetInDirectVision as true for Snap Rotation and No Rotation
        // It is required by the UseAbility() method. Only the SmoothRotation() method can make it false
        targetInDirectVision = true;        
    }

    void Update()
    {
        // Determines if target is within range to follow or use ability; Changes destination, speed, and color accordingly
        RangeDetection();
        // Enables/Disables transparent circle that shows target detection range
        VisualDetection();

        // When the NPC has reached the waypoint, calls SetNewDestination() to select a new random waypoint within the wander radius
        if (Vector2.Distance(transform.position, randomWaypoint) <= randomWaypointSize)
        {
            SetNewDestination();
        }

        // Calls the appropriate method to rotate the NPC depending on which Enum is selected in the Inspector
        // Instantly rotate towards current target
        if (rotationStyle == RotationStyle.snap)
        {
            SnapRotation();
        }
        // Smoothly rotate towards current target
        else if (rotationStyle == RotationStyle.smooth)
        {
            SmoothRotation();
        }
        // No rotation
        else if (rotationStyle == RotationStyle.none)
        {
            // NPC moves towards currentDestination at currentSpeed
            transform.position = Vector2.MoveTowards(transform.position, currentDestination, currentSpeed * Time.deltaTime);
        }
    }

    void SetNewDestination()
    {
        // Select a random waypoint / destination within wander radius of NPC's starting position
        randomWaypoint = Random.insideUnitCircle * wanderRange + originPosition;
    }

    Vector3 RotateTo()
    {
        // Calculate directional Vector3 pointing towards NPC's current destination (waypoint or target)
        Vector3 rotateTowards = ((Vector3)currentDestination - transform.position).normalized;
        return rotateTowards;
    }

    void SnapRotation()
    {
        // Calculate angle in degrees required to reach target in world space
        float snapRotateAngle = Vector2.SignedAngle(Vector2.up, RotateTo());
        // Rotate NPC in the direction of target
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, snapRotateAngle));
        // NPC moves towards currentDestination at currentSpeed
        transform.position = Vector2.MoveTowards(transform.position, currentDestination, currentSpeed * Time.deltaTime);
    }

    void SmoothRotation()
    {
        // Calculate angle in degrees required to reach target in local space
        float smoothRotateAngle = Vector2.SignedAngle(transform.up, RotateTo());

        // Turns the NPC towards the target at a fixed rate. currentRotationSpeed determined in RangeDetection()
        if (smoothRotateAngle > 0)
        {
            // Rotate Left
            transform.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);
        }
        else if (smoothRotateAngle < 0)
        {
            // Rotate Right
            transform.Rotate(0f, 0f, -currentRotationSpeed * Time.deltaTime);
        }

        // IF the angle value used for rotation is within the directVisionAngleThreshold, target is in direct vision of NPC
        if (smoothRotateAngle > -directVisionAngleThreshold && smoothRotateAngle < directVisionAngleThreshold)
        {
            targetInDirectVision = true;
        }
        else
        {
            targetInDirectVision = false;
        }

        // NPC can only move in the forward facing direction at currentSpeed
        transform.position = transform.position + transform.up * currentSpeed * Time.deltaTime;
    }

    void RangeDetection()
    {
        // Determines if target is within range to use an ability or to follow;
        // Changes destination, speed, rotation speed, and color accordingly

        // Red - Target is within ability range
        if (Vector2.Distance(transform.position, targetPlayer.transform.position) <= abilityRange)
        {
            spriteColor.color = Color.red;
            currentDestination = targetPlayer.transform.position;
            currentSpeed = targetingSpeed;
            currentRotationSpeed = followRotationSpeed;

            AbilityCooldown();

            if (Vector2.Distance(transform.position, targetPlayer.transform.position) <= 2f)
            { currentSpeed = 0f; }
        }
        // Yellow - Target is outside ability range, but within following range
        else if (Vector2.Distance(transform.position, targetPlayer.transform.position) > abilityRange
            && Vector2.Distance(transform.position, targetPlayer.transform.position) <= detectionRange)                
        {
            spriteColor.color = Color.yellow;
            currentDestination = targetPlayer.transform.position;
            currentSpeed = followingSpeed;
            currentRotationSpeed = followRotationSpeed;
        }
        // Green - Target is not in any range to use an ability or to follow
        else
        {
            spriteColor.color = Color.green;
            currentDestination = randomWaypoint;
            currentSpeed = wanderSpeed;
            currentRotationSpeed = wanderRotationSpeed;
        }
    }

    void VisualDetection()
    {
        if (showDetectionRangeRadius == true)
        {
            // Enable in game follow range circle and set its size using detectionRange set in Inspector
            detectionRangeObject.gameObject.SetActive(true);
            detectionRangeObject.transform.localScale = new Vector2(detectionRange, detectionRange);
        }
        else if (showDetectionRangeRadius == false)
        {
            // Disable in game follow range circle
            detectionRangeObject.gameObject.SetActive(false);
        }

        if (showAbilityRangeRadius == true)
        {
            // Enable in game ability range circle and set its size using abilityRange set in Inspector
            abilityRangeObject.gameObject.SetActive(true);
            abilityRangeObject.transform.localScale = new Vector2(abilityRange, abilityRange);
        }
        else if (showAbilityRangeRadius == false)
        {
            // Disable in game ability range circle
            abilityRangeObject.gameObject.SetActive(false);
        }
    }

    void AbilityCooldown()
    {
        if (abilityTimer <= 0 && targetInDirectVision == true)
        {
            // When the timer hits 0 AND the target is in direct vision of the NPC,
            // UseAbility() method is called and the ability timer is reset
            UseAbility();
            abilityTimer = abilityCooldownTime;
        }
        else
        {
            // ELSE the ability is not in use, and counts down the ability timer
            abilityTimer -= Time.deltaTime;
        }
    }

    void UseAbility()
    {
        // Instantiate a new GameObject at the NPC's position, passing abilitySpeed variable to that instantiated GameObject's TankBulletAbility script
        // & this GameObjects facing direction using transform.rotation
        GameObject ability = Instantiate(abilityPrefab, transform.position, transform.rotation);
        ability.GetComponent<Ability_TankBullet_V1>().abilitySpeed = abilitySpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {        
        // Temporary failsafe for NPC getting stuck on barriers

        // If this object collides with an object containing a trigger collider that is tagged with Barrier
        if (collision.gameObject.CompareTag("Barrier"))
        {
            // Add 180 deg. to the rotation of the NPC in Euler angles. NPC will face opposite direction
            transform.rotation = Quaternion.Euler(0f, 0f, transform.eulerAngles.z + 180);
            // Calls SetNewDestination() to select a new random waypoint within the wander radius
            SetNewDestination();
        }
    }

    private void OnDrawGizmos()
    {        
        if (enableDestinationRay == true)
        {
            // Visual Debug - NPC Destination Direction
            Debug.DrawRay(transform.position, RotateTo() * 5f, Color.black);
        }
        if (enableWanderRangeAndWaypoints == true)
        {
            // Visual Debug - NPC Wander Range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(originPosition, wanderRange);
            // Visual Debug - NPC Random Waypoint
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(randomWaypoint, randomWaypointSize);
        }
        if (enableDetectionAndAbilityRange == true)
        {
            // Visual Debug - NPC's Player Detection Range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            // Visual Debug - NPC's Ability Range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, abilityRange);
        }
    }
}