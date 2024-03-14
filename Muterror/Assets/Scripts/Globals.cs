using UnityEngine;

public class Globals : MonoBehaviour
{
    public static Globals SINGLETON;

    //PLAYER GLOOPER
    public GameObject player;

    public const float FRICTION = 0.16f; // BETWEEN 0 AND 1
    public const float GRAVITY = -2.5f;
    public const float AIRRESISTANCE = 0.07f;

    //CAMERA GLOOPER
    public const float CAMERASPEED = 0.3f;
    public const float CAMERADISTANCE = 10f;

    public float cameraShakeAmplitude = 0;
    public float cameraShakeChange = 0;

    public Vector3 GetCameraPosition()
    {
        Vector3 playerPosition = player.transform.position;
        return new Vector3(playerPosition.x, playerPosition.y, -CAMERADISTANCE);
    }

    public static void CameraShake(float duration, float amplitude)
    {
        SINGLETON.cameraShakeAmplitude = amplitude;
        SINGLETON.cameraShakeChange = 1 / (60 * duration) * duration;
    }

    public void Start()
    {
        SINGLETON = this;

        player = GameObject.Find("Player");
    }

    public void FixedUpdate()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 playerPosition = player.transform.position;

        if (cameraShakeAmplitude > 0f)
        {
            Vector3 shakeOffset = new Vector3(Random.Range(-cameraShakeAmplitude, cameraShakeAmplitude), Random.Range(-cameraShakeAmplitude, cameraShakeAmplitude), 0);
            Camera.main.transform.position = Vector3.Lerp(cameraPosition - shakeOffset, GetCameraPosition(), CAMERASPEED) + shakeOffset;
            cameraShakeAmplitude -= cameraShakeChange;
        }
        else
        {
            Camera.main.transform.position = Vector3.Lerp(cameraPosition, GetCameraPosition(), CAMERASPEED);
        }
    }
}
