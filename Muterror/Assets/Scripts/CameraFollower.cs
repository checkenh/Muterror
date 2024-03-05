using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    private GameObject player;
    public float speed = 7f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 playerPosition = player.transform.position;

        Camera.main.transform.position = Vector3.Lerp(cameraPosition, new Vector3(playerPosition.x, playerPosition.y, -10), speed * Time.fixedDeltaTime);
    }
}
