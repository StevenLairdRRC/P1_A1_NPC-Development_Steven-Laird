using UnityEngine;

public class NPC_Spawner_V0 : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] npcPrefabs;             // Array of NPC prefabs to be spawned
    public LayerMask npcs;                      // Layer mask to filter which objects are considered NPCs

    public int maxNpcInRadius = 2;              // Maximum number of NPCs allowed in the spawn radius
    public float spawnRadius = 30f;             // Radius within which NPCs can spawn
    public int spawnRateTimeMin = 30;           // Minimum time between NPC spawns
    public int spawnRateTimeMax = 45;           // Maximum time between NPC spawns

    [Header("Debug Info")]
    public bool enableSpawnRadiusGizmo = true;  // Bool to enable/disable visualizing the spawn radius in the editor
    public float spawnTimer;                    // Timer for controlling NPC spawn intervals
    private bool canSpawnNewNpc;                // Bool indicating if a new NPC can be spawned
    public int npcCount;                        // Current number of NPCs within the spawn radius
    private Vector2 spawnerCenter;              // Center position of the spawner

    void Start()
    {
        // Initialize the ability to spawn a new NPC
        canSpawnNewNpc = true;
        // Set an initial random spawn timer
        spawnTimer = Random.Range(spawnRateTimeMin, spawnRateTimeMax);
        // Store the center position of the spawner
        spawnerCenter = transform.position;
    }

    void Update()
    {
        // Call SpawnCooldown to update the cooldown timer under certain conditions
        SpawnCooldown();
        // Count the number of NPCs within the spawn radius
        npcCount = CountNpcsNearSpawner();

        // IF conditions are met
        if (canSpawnNewNpc == true)
        {
            if (npcCount < maxNpcInRadius)
            {
                // Spawn a new NPC
                SpawnNpc();
            }
            // Reset the random spawn timer
            spawnTimer = Random.Range(spawnRateTimeMin, spawnRateTimeMax);
        }
    }

    // Count the number of NPCs within the spawn radius
    public int CountNpcsNearSpawner()
    {
        int numberOfNpc = 0;
        // Detect all NPCs within the spawn radius using OverlapCircleAll
        Collider2D[] detectedNpcs = Physics2D.OverlapCircleAll(transform.position, spawnRadius, npcs);
        for (int i = 0; i < detectedNpcs.Length; i++)
        {
            numberOfNpc++;
        }
        // Return the amount of detected NPCs as an int
        return numberOfNpc;
    }

    // Spawn a new NPC at a random position within the spawn radius
    public void SpawnNpc()
    {
        // Select a random prefab from the array of prefabs set in the Inspector
        int randomPrefab = Random.Range(0, npcPrefabs.Length);
        // Generate a random waypoint within the spawn radius
        Vector2 randomWaypoint = Random.insideUnitCircle * spawnRadius + spawnerCenter;
        // Instantiate the selected NPC prefab at the random waypoint
        GameObject newNpc = Instantiate(npcPrefabs[randomPrefab], randomWaypoint, Quaternion.identity, gameObject.transform);
    }

    // Manage the cooldown before the next NPC can be spawned
    public void SpawnCooldown()
    {
        // IF the spawnTimer has reached 0
        if (spawnTimer <= 0)
        {
            // Allow a new NPC to be spawned and reset the random spawn timer
            canSpawnNewNpc = true;
            spawnTimer = Random.Range(spawnRateTimeMin, spawnRateTimeMax);
        }
        // ELSE IF the current amount of NPCs in the spawner radius is less than the max allowed
        else if (npcCount < maxNpcInRadius)
        {
            // Prevent new NPC spawns and continue counting down the spawn timer
            canSpawnNewNpc = false;
            spawnTimer -= Time.fixedDeltaTime;
        }
    }

    // Visualize the spawn radius in the Unity editor for debugging purposes
    public void OnDrawGizmos()
    {
        if (enableSpawnRadiusGizmo)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(spawnerCenter, spawnRadius);
        }
    }
}