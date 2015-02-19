using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
    public float cameraSpeed = 1;
    public float minDist = 3;
    public float maxDist = 10;
    public float zoomSpeed = 3;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(Input.GetKey(KeyCode.W)) {
            if(gameObject.transform.localEulerAngles.x < 80 || gameObject.transform.localEulerAngles.x > 270)
                gameObject.transform.RotateAround(Vector3.zero, transform.right, 360 * Time.deltaTime * cameraSpeed);
        }
        else if(Input.GetKey(KeyCode.S)) {
            if(gameObject.transform.localEulerAngles.x < 90 || gameObject.transform.localEulerAngles.x > 280)
                gameObject.transform.RotateAround(Vector3.zero, -transform.right, 360 * Time.deltaTime * cameraSpeed);
        }

        if(Input.GetKey(KeyCode.A)) {
            gameObject.transform.RotateAround(Vector3.zero, Vector3.up, 360 * Time.deltaTime * cameraSpeed);
        }
        else if(Input.GetKey(KeyCode.D)) {
            gameObject.transform.RotateAround(Vector3.zero, Vector3.down, 360 * Time.deltaTime * cameraSpeed);
        }

        float distFromOrigin = gameObject.transform.position.magnitude;

        if(Input.GetKey(KeyCode.Q) && distFromOrigin < maxDist) {
            gameObject.transform.localPosition += -gameObject.transform.forward * zoomSpeed * Time.deltaTime;
        }
        else if(Input.GetKey(KeyCode.E) && distFromOrigin > minDist) {
            gameObject.transform.localPosition += gameObject.transform.forward * zoomSpeed * Time.deltaTime;
        }
	}
}
