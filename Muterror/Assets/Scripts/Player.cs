using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Mutations;

public class Player : MonoBehaviour
{
    private GameObject player;
    private SpriteRenderer spriteRenderer;

    public float friction = 0.16f; // BETWEEN 0 AND 1
    public float gravity = -2f;

    private bool grounded = false;
    private Vector2 velocity = Vector2.zero;

    // Movement Types
    class Movement {
        public KeyCode key;
        public string identifier;
        public float strength;
        public Vector2 direction;
        public bool needsGround;

        public Movement(KeyCode key, string identifier, float strength, Vector2 direction, bool needsGround)
        {
            this.key = key;
            this.identifier = identifier;
            this.strength = strength;
            this.direction = direction;
            this.needsGround = needsGround;
        }
    }

    Movement[] movements =
    {
        new Movement(KeyCode.Space, "Space", 25f, new Vector2(0, 1), true),
        new Movement(KeyCode.A, "Left", 2f, new Vector2(-1, 0), false),
        new Movement(KeyCode.D, "Right", 2f, new Vector2(1, 0), false),
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
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime; // makes values correlate to seconds rather than be ambiguous
        Vector2 position = player.transform.position;

        // Change velocity based on inputs
        for (int i = 0; i < movements.Length; i++)
        {
            Movement movement = movements[i];
            if (Input.GetKey(movement.key))
            {
                if (movement.needsGround == true && grounded == false)
                    continue;

                velocity = velocity + (movement.direction * movement.strength);
            }
        }

        // Activate mutations
        if (Sun.InSunlight() == true)
            Sun.Mutate();
        if (Sun.enabled == true)
            Sun.Passive();
        
        // Apply gravity
        velocity.y += gravity;
        velocity.x *= 1 - friction;

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
