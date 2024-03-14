using System;
using System.Collections.Generic;
using UnityEngine;
using Mutations;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private GameObject player;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    public bool grounded = false;
    public Vector2 velocity = Vector2.zero;

    bool sprinting = false;
    float sprintMultiplier = 1f;

    public bool wallClimbing = false;
    public float wallClimbBegin = 0;
    public float wallClimbEnd = 0;

    public int stage = 0;
    private bool movingHorizontal = false;
    private bool movingVertical = false;

    // Movement Types
    public enum MovementType
    {
        Add,
        Set,
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

    public Dictionary<KeyCode, Movement> Movements;

    public List<Mutation> Mutations;

    private void UpdateAnimator()
    {
        if (movingVertical == true)
        {
            animator.Play("Player_Jump_Stage" + stage);
            return;
        }

        if (movingHorizontal == true)
        {
            animator.speed = sprintMultiplier;
            animator.Play("Player_Run_Stage" + stage);
            return;
        }

        animator.Play("Player_Idle_Stage" + stage);
    }

    private void CalculateCollision(Vector2 normal, float distance)
    {
        Vector2 position = player.transform.position;

        float normalAngle = Vector2.SignedAngle(normal, Vector2.up);
        float differenceAngle = Vector2.SignedAngle(Vector2.up, player.transform.up);
        float radians = (normalAngle + differenceAngle) / (180 / Mathf.PI);
        Vector2 localNormal = new Vector2((float) Mathf.Sin(radians), (float) Mathf.Cos(radians));

        Vector2 perpendicular = Vector2.Perpendicular(localNormal);
        float x = Math.Abs(perpendicular.x);
        float y = Math.Abs(perpendicular.y);
        float dot = Vector2.Dot(player.transform.up, normal);

        // Set grounded
        if (dot > 0.75f)
            grounded = true;

        // Adjust velocity & position
        velocity *= new Vector2(x, y);
        player.transform.position = position + normal * distance;
    }

    private void Mutate(int newStage)
    {
        newStage = Math.Clamp(newStage, 0, Mutations.Count);

        if (newStage <= stage)
            return;
        stage = newStage;

        for (int i = 0; i < stage; i++)
        {
            Mutation mutation = Mutations[i];
            if (mutation.Enabled == false)
                Mutations[i].Init();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;
        float distance = -contact.separation;

        if (contact.rigidbody.gameObject.name.StartsWith("fan"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        CalculateCollision(normal, distance);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;
        float distance = -contact.separation;

        if (contact.rigidbody.gameObject.name.StartsWith("fan"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        CalculateCollision(normal, distance);
    }

    private void Start()
    {
        player = GameObject.Find("Player");

        spriteRenderer = player.GetComponent<SpriteRenderer>();
        animator = player.GetComponent<Animator>();

        Movements = new Dictionary<KeyCode, Movement>()
        {
            [KeyCode.Space] = new Movement("Jump", new Vector2(0, 1), 35f, MovementType.Set, true),
            [KeyCode.W] = new Movement("Jump", new Vector2(0, 1), 35f, MovementType.Set, true),
            [KeyCode.A] = new Movement("Left", new Vector2(-1, 0), 2f, MovementType.Add, false),
            [KeyCode.D] = new Movement("Right", new Vector2(1, 0), 2f, MovementType.Add, false),
        };

        Mutations = new List<Mutation>()
        {
            new Sun(),
            new Water(),
            new Ground(),
            new Air()
        };
    }

    private void Update()
    {
        // Debug
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Mutate(stage + 1);
        }
        else if (Input.GetKey(KeyCode.R))
        {
            Effects.BloodExplosion(player.transform.position, 150);
        }
        else if (Input.GetKey(KeyCode.T))
        { 
            Globals.CameraShake(1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            player.transform.Rotate(0, 0, 90);
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime; // Makes values correlate to seconds rather than be ambiguous

        Vector2 position = player.transform.position;
        Vector3 rotation = player.transform.eulerAngles;

        sprinting = Input.GetKey(KeyCode.LeftShift);
        sprintMultiplier = sprinting ? 1.5f : 1f;

        // Reset animation parameters
        movingHorizontal = false;
        movingVertical = false;

        // Change velocity based on inputs
        foreach (KeyValuePair<KeyCode, Movement> entry in Movements)
        {
            KeyCode keyCode = entry.Key;
            Movement movement = entry.Value;

            if (Input.GetKey(keyCode))
            {
                if (movement.needsGround == true && grounded == false)
                    continue;

                Vector2 direction = movement.direction;
                float strength = movement.strength;

                // Set animation parameters
                if (movement.direction.x > 0.1f)
                {
                    movingHorizontal = true;
                    spriteRenderer.flipX = true;
                } 
                else if (movement.direction.x < -0.1f)
                {
                    movingHorizontal = true;
                    spriteRenderer.flipX = false;
                }

                // Add key velocities
                if (movement.type == MovementType.Add)
                    velocity = velocity + (direction * strength * sprintMultiplier);
                else if (movement.type == MovementType.Set)
                    velocity = new Vector2(velocity.x * (1 - direction.x), velocity.y * (1 - direction.y)) + (direction * strength);
            }
        }

        // Detect if they've been wall climbing for too long or no longer sprinting and kick them off
        if (wallClimbing == true && Time.time - wallClimbBegin > 0.8f)
        {
            wallClimbEnd = Time.time;
            grounded = false;
        }

        // Set vertical movement bool
        if (grounded == false)
        {
            if (rotation != Vector3.zero)
            {
                float velocityAngle = Vector2.SignedAngle(velocity.normalized, Vector2.up);
                float differenceAngle = Vector2.SignedAngle(Vector2.up, player.transform.up);
                float radians = (velocityAngle + differenceAngle) / (180 / Mathf.PI);
                Vector2 globalVelocity = new Vector2((float)Mathf.Sin(radians), (float)Mathf.Cos(radians)) * velocity;

                player.transform.rotation = Quaternion.Euler(0, 0, 0);
                velocity = globalVelocity;

                wallClimbing = false;
            }
            movingVertical = true;
        }

        // Activate mutations
        for (int i = 0; i < Movements.Count; i++)
        {
            Mutation mutation = Mutations[i];
            if (mutation.CheckMutatable() == true)
                Mutate(mutation.Stage);
            if (mutation.Enabled == true)
                mutation.Passive();
        }

        // Apply gravity
        velocity.y += Globals.GRAVITY;
        velocity.x *= 1 - Globals.FRICTION;
        velocity.y *= 1 - Globals.AIRRESISTANCE;

        // Set position
        player.transform.Translate(velocity * deltaTime, Space.Self);

        //
        Vector2 mDir = player.transform.right * (spriteRenderer.flipX ? 1 : -1);
        float mStr = 0.5f;
        RaycastHit2D raycast = Physics2D.Raycast(player.transform.position, mDir, mStr);
        if (raycast.collider != null)
        {
            CalculateCollision(raycast.normal, mStr);
        }

        // Update animations
        UpdateAnimator();

        // Reset grounded
        grounded = false;
    }
}
