using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Mutations;
using System.Xml;
using UnityEditor.Experimental.GraphView;

public class Player : MonoBehaviour
{
    private GameObject player;
    private SpriteRenderer spriteRenderer;

    private bool grounded = false;
    [SerializeField] private Vector2 velocity = Vector2.zero;

    // Movement Types
    public enum MovementType
    {
        Add,
        Set
    }

    public class Movement {
        public string identifier;
        public Vector2 direction;
        public float strength;
        public MovementType type;
        public bool needsGround;

        public Movement(string identifier, Vector2 direction, float strength, MovementType type, bool needsGround)
        {
            this.identifier = identifier;
            this.direction = direction;
            this.strength = strength;
            this.type = type;
            this.needsGround = needsGround;
        }
    }

    public static Dictionary<KeyCode, Movement> movements = new Dictionary<KeyCode, Movement>()
    {
        [KeyCode.Space] = new Movement("Jump", new Vector2(0, 1), 25f, MovementType.Add, true),
        [KeyCode.W] = new Movement("Jump", new Vector2(0, 1), 25f, MovementType.Add, true),
        [KeyCode.A] = new Movement("Left", new Vector2(-1, 0), 2f, MovementType.Add, false),
        [KeyCode.D] = new Movement("Right", new Vector2(1, 0), 2f, MovementType.Add, false),
    };

    private void CalculateCollision(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 position = player.transform.position;

        Vector2 normal = contact.normal;
        Vector2 perpendicular = Vector2.Perpendicular(normal);

        float distance = -contact.separation;
        float x = Math.Abs(perpendicular.x);
        float y = Math.Abs(perpendicular.y);

        if (normal.y >= 0.5f)
            grounded = true;

        velocity *= new Vector2(x, y);
        player.transform.position = position + normal * distance;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CalculateCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CalculateCollision(collision);
    }

    private void Start()
    {
        player = GameObject.Find("Player");
        spriteRenderer = player.GetComponent<SpriteRenderer>();

        Wings.Mutate();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime; // makes values correlate to seconds rather than be ambiguous
        Vector2 position = player.transform.position;

        // Change velocity based on inputs
        foreach (KeyValuePair<KeyCode, Movement> entry in movements)
        {
            KeyCode keyCode = entry.Key;
            Movement movement = entry.Value;

            if (Input.GetKey(keyCode))
            {
                if (movement.needsGround == true && grounded == false)
                    continue;
                if (movement.type == MovementType.Add)
                {
                    velocity = velocity + (movement.direction * movement.strength);
                } 
                else
                {
                    Vector2 oppositeVector = new Vector2(velocity.x * (1 - movement.direction.x), velocity.y * (1 - movement.direction.y));
                    velocity = oppositeVector + (movement.direction * movement.strength);
                }
            }
        }

        // Activate mutations
        if (Sun.InSunlight() == true)
            Sun.Mutate();
        if (Sun.enabled == true)
            Sun.Passive();
        
        // Apply gravity
        velocity.y += Globals.gravity;
        velocity.x *= 1 - Globals.friction;

        // Detect if out-of-range
        if (position.y + velocity.y * deltaTime < -10f)
        {
            player.transform.position = new Vector2(0, 0);
            return;
        }

        // Set position
        position = position + velocity * deltaTime;

        // Move Player
        player.transform.position = position;

        // Reset grounded
        grounded = false;
    }
}
