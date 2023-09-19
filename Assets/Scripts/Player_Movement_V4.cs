using UnityEngine;

public class Player_Movement_V4 : MonoBehaviour
{
    // Public variables for controlling player movement and abilities
    [Header("Movement Settings")]
    public float playerMovementSpeed = 4f;      // Speed of player movement
    public float rotationSpeed = 40f;           // Rotation speed of the player

    [Header("Ability Settings")]
    public GameObject abilityPrefab;            // Prefab of the player's ability GameObject
    public float abilitySpeed = 6f;             // Speed of the player's ability
    public float abilityCooldownTime = 1f;      // Cooldown time for the player's ability

    // Private variables for input and ability cooldown
    private float horizontalInput;              // Stores horizontal input (-1 to 1)
    private float verticalInput;                // Stores vertical input (-1 to 1)
    private bool buttonInput;                   // Stores a button press input
    private float abilityTimer;                 // Timer for ability cooldown

    void Start()
    {
        // Initialize ability timer to the cooldown time at the start of the game
        abilityTimer = abilityCooldownTime;
    }

    void Update()
    {
        // Get horizontal, vertical, and button inputs from the player
        horizontalInput = Input.GetAxisRaw("Horizontal");   // Keyboard: W, S, Arrow Key Up, Arrow Key Down
        verticalInput = Input.GetAxisRaw("Vertical");       // Keyboard: A, D, Arrow Key Left, Arrow Key Right
        buttonInput = Input.GetButton("Fire1");             // Keyboard: Left Mouse Click

        // Move the player character forward or backward based on vertical input
        if (verticalInput > 0)
        {
            // Move the player forward using transform position and deltaTime
            transform.position = transform.position + transform.up * playerMovementSpeed * Time.deltaTime;
        }
        else if (verticalInput < 0)
        {
            // Move the player backward using transform position and deltaTime
            transform.position = transform.position + transform.up * -playerMovementSpeed * Time.deltaTime;
        }

        // Rotate the player character left or right based on horizontal input
        if (horizontalInput > 0)
        {
            // Rotate the player counterclockwise using deltaTime
            transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
        else if(horizontalInput < 0)
        {
            // Rotate the player clockwise using deltaTime
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        // Call and update ability cooldown
        AbilityCooldown();
    }

    void AbilityCooldown()
    {
        // IF the ability timer has reached zero and the button is pressed
        if (abilityTimer <= 0 && buttonInput == true)
        {
            // When the timer hits 0, UseAbility() method is called and the ability timer is reset
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
        // Instantiate a new GameObject at the NPC's position, and facing in the same direction
        GameObject ability = Instantiate(abilityPrefab, transform.position, transform.rotation);
        // Set the speed of the instantiated ability GameObject
        ability.GetComponent<Ability_TankBullet_V1>().abilitySpeed = abilitySpeed;
    }
}