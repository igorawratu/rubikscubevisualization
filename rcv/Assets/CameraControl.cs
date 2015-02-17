using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
    public float cameraSpeed = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKey(KeyCode.W)) {
            if(gameObject.transform.localRotation.eulerAngles.x < 90 || gameObject.transform.localRotation.eulerAngles.x > 269)
                gameObject.transform.RotateAround(Vector3.zero, transform.right, 360 * Time.deltaTime * cameraSpeed);
        }
        else if(Input.GetKey(KeyCode.S)) {
            if(gameObject.transform.localRotation.eulerAngles.x < 91 || gameObject.transform.localRotation.eulerAngles.x > 270)
                gameObject.transform.RotateAround(Vector3.zero, -transform.right, 360 * Time.deltaTime * cameraSpeed);
        }

        if(Input.GetKey(KeyCode.A)) {
            gameObject.transform.RotateAround(Vector3.zero, Vector3.up, 360 * Time.deltaTime * cameraSpeed);
        }
        else if(Input.GetKey(KeyCode.D)) {
            gameObject.transform.RotateAround(Vector3.zero, Vector3.down, 360 * Time.deltaTime * cameraSpeed);
        }
	}
}
