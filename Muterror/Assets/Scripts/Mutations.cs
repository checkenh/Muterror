using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Mutations
{
    public static class Sun
    {
        static GameObject sunlight = GameObject.Find("Sunlight");

        static GameObject player = GameObject.Find("Player");
        static SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();

        // base fields
        public static string name = "Sun";
        public static bool enabled = false;

        // mutation specific fields
        static GameObject mushroomLight = GameObject.Find("Mushroom Light");

        static float maxHeat = 5f;
        static float heat = 0f;

        public static bool InSunlight()
        {
            Vector2 lightPosition = new Vector2(sunlight.transform.position.x, sunlight.transform.position.y);
            Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);

            Vector2 direction = -(lightPosition - playerPosition).normalized;
            RaycastHit2D raycast = Physics2D.Raycast(lightPosition, direction);

            if (raycast.collider != null && raycast.collider.gameObject == player)
                return true;
            else
                return false;
        }

        public static void Mutate()
        {
            spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/manmushroom");
            enabled = true;
        }

        public static void Passive()
        {
            // Vector2 lightPosition = new Vector2(sunlight.transform.position.x, sunlight.transform.position.y);
            Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);

            if (InSunlight() == true)
            {
                if (heat < maxHeat)
                    heat += 0.1f;
            } else
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

    public static class Wings
    {
        static GameObject player = GameObject.Find("Player");
        static SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();

        // base fields
        public static string name = "Wings";
        public static bool enabled = false;

        // mutation specific fields
        public static float flySpeed = 8f;
        
        public static void Mutate()
        {
            spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/manwings");

            Player.movements[KeyCode.Space] = new Player.Movement("Fly", new Vector3(0, 1f), flySpeed - Globals.gravity, Player.MovementType.Set, false);

            enabled = true;
        }
    }
}
