using UnityEngine;

namespace Mutations
{
    public abstract class Mutation
    {
        public abstract string Name { get; }
        public abstract int Stage { get; }
        public bool Enabled { get; set; }

        public Player playerClass;
        public GameObject player;

        public SpriteRenderer spriteRenderer;
        public Animator animator;

        public abstract void Init();

        public KeyCode ActivationKey;

        public abstract void Passive();

        public abstract void Activate();

        public abstract bool CheckMutatable();

        public Mutation()
        {
            playerClass = GameObject.FindObjectOfType<Player>();
            player = playerClass.gameObject;

            spriteRenderer = player.GetComponent<SpriteRenderer>();
            animator = player.GetComponent<Animator>();
        }
    };

    public class Sun : Mutation
    {
        public override string Name => "Sun";
        public override int Stage => 1;

        GameObject mushroomLight = GameObject.Find("Mushroom Light");

        float maxHeat = 5f;
        float heat = 0f;

        public override bool CheckMutatable()
        {
            GameObject[] lights = { GameObject.Find("Sunlight"), GameObject.Find("lightsource"), GameObject.Find("lightsource2") };

            for (int i = 0; i < lights.Length; i++)
            {
                GameObject light = lights[i];
                Debug.Log("light");
                Vector2 lightPosition = new Vector2(light.transform.position.x, light.transform.position.y);
                Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);

                Vector2 direction = -(lightPosition - playerPosition).normalized;
                RaycastHit2D raycast = Physics2D.Raycast(lightPosition, direction);

                if (raycast.collider != null && raycast.collider.gameObject == player)
                    return true;
            }

            return false;
        }

        public override void Activate() { }

        public override void Init()
        {
            Effects.BloodExplosion(player.transform.position + new Vector3(0, 0.5f, 0), 150);
            Globals.CameraShake(1, 1);
            Enabled = true;
        }

        public override void Passive()
        {
            Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);

            if (CheckMutatable() == true)
            {
                if (heat < maxHeat)
                    heat += 0.1f;
            }
            else
            {
                if (heat > 0f)
                    heat -= 0.02f;
            }

            mushroomLight.transform.position = playerPosition;
            if (heat > 0f)
                mushroomLight.GetComponent<UnityEngine.Rendering.Universal.Light2D>().intensity = 1 / (maxHeat / heat);
            else
                mushroomLight.GetComponent<UnityEngine.Rendering.Universal.Light2D>().intensity = 0f;
        }
    }

    public class Water : Mutation
    {
        public override string Name => "Water";
        public override int Stage => 2;

        public KeyCode ActivationKey = KeyCode.F;

        public override bool CheckMutatable()
        {
            return false;
        }

        public override void Activate()
        {
            
        }

        public override void Init()
        {
            Enabled = true;
        }

        public override void Passive() { }
    }

    public class Ground : Mutation
    {
        public override string Name => "Ground";
        public override int Stage => 3;

        public override void Activate() { }

        public override void Init()
        {
            Enabled = true;
        }

        public override void Passive()
        {
            if (Time.time - playerClass.wallClimbEnd < 1f)
                return;

            RaycastHit2D raycast = Physics2D.Raycast(player.transform.position, player.transform.right * (spriteRenderer.flipX ? 1 : -1), 1f);
            if (raycast.collider == null)
                return;
            
            float angle = Vector2.SignedAngle(player.transform.up, raycast.normal);
            player.transform.Rotate(new Vector3(0, 0, angle));

            if (player.transform.eulerAngles == Vector3.zero)
            {
                playerClass.wallClimbing = false;
            } 
            else
            {
                playerClass.wallClimbing = true;
                playerClass.wallClimbBegin = Time.time;
            }
        }
        
        public override bool CheckMutatable()
        {
            return false;
        }
    }

    public class Air : Mutation
    {
        public override string Name => "Air";
        public override int Stage => 4;

        public override void Activate() { }

        public override void Init()
        {
            playerClass.Movements[KeyCode.Space] = new Player.Movement("Fly", new Vector2(0, 1f), 12f, Player.MovementType.Set, false);
            Enabled = false;
        }

        public override void Passive() { }

        public override bool CheckMutatable()
        {
            return false;
        }
    }
}
