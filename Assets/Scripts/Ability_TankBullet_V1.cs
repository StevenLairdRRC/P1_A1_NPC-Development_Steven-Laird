using UnityEngine;

public class Ability_TankBullet_V1 : MonoBehaviour
{
    [Header("Ability Destruction Settings")]
    private float abilityDestroyTimer;          // Timer to track the ability's remaining lifespan
    public float abilityDestroyIn = 10f;        // Time before the ability is destroyed
    public float explosionDestroyIn = 1f;       // Time before the explosion effect is destroyed
    public GameObject explosionPrefab;          // Prefab for the explosion effect when the ability hits something

    [Header("Value Passed From NPC Script")]
    public float abilitySpeed;                  // Speed at which the ability moves


    void Start()
    {
        // Move this GameObject at a speed passed from the NPC script which is assigned an NPC's Inspector
        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        rb2d.AddForce(transform.up * abilitySpeed, ForceMode2D.Impulse);

        // Initialize destroyTimer with destroyAfter value set in the Inspector
        abilityDestroyTimer = abilityDestroyIn;
    }
 
    void Update()
    {
        // Update DestroyTimer every frame
        DestroyTimer();
    }

    void DestroyTimer()
    {
        // IF the destroyTimer is greater than 0,
        if (abilityDestroyTimer > 0f)
        {
            // Continue the countdown timer
            abilityDestroyTimer -= Time.deltaTime;
        }
        // ELSE the destroyTimer has reached 0,
        else
        {
            // Call the DestroyObject method
            DestroyAbility();
        }
    }

    void DestroyAbility()
    {
        // Destroy this GameObject (the ability)
        Destroy(gameObject);
        // Spawn an explosion prefab at this GameObject's position and rotation
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        // Destroy the explosion effect after a specified time
        Destroy(explosion, explosionDestroyIn);
    }

    // Handle collision with other GameObjects containing Rigidbody2D and Collider2D components
    // Note: Specific GameObjects can be ignored by assigning them to a layer and using collision matrix or layer overrides
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Call the DestroyAbility method to remove the ability
        DestroyAbility();

        // Check if the collision involves an object tagged as "NPC"
        if (collision.gameObject.CompareTag("NPC"))
        {
            // Destroy the NPC GameObject
            Destroy(collision.gameObject);
            // Spawn an explosion prefab at the NPC's position and rotation
            GameObject explosion = Instantiate(explosionPrefab, collision.transform.position, Quaternion.identity);
            // Destroy the explosion effect after a specified time
            Destroy(explosion, explosionDestroyIn);
        }
    }
}