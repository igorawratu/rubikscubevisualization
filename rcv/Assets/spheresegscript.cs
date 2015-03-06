using UnityEngine;
using System.Collections;

public class spheresegscript : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float dotBack = Vector3.Dot(gameObject.transform.forward, Camera.main.transform.position - gameObject.transform.position);
        float dotUp = Vector3.Dot(gameObject.transform.up, Camera.main.transform.up);

        if(dotBack > 0) 
            gameObject.transform.RotateAround(transform.position, transform.up, 180);

        if(dotUp < 0) 
            gameObject.transform.RotateAround(transform.position, transform.right, 180);
	}

    public void setSpheresegInfo(int _orientation, int _spheresegID) {
        mOrientation = _orientation;
        mSpheresegID = _spheresegID;

        updateText();
    }

    public void rot(Axis _axis, bool _dirPos) {
        if(_dirPos)
            mOrientation = recalculateOrientation(_axis);
        else {
            mOrientation = recalculateOrientation(_axis);
            mOrientation = recalculateOrientation(_axis);
            mOrientation = recalculateOrientation(_axis);
        }
        if(gameObject.GetComponent<TextMesh>().text != "")
            updateText();
    }

    public void toggleText() {
        if(gameObject.GetComponent<TextMesh>().text == "")
            updateText();
        else gameObject.GetComponent<TextMesh>().text = "";
    }

    private void updateText() {
        string textToDisplay = "";
        textToDisplay += "Sphere segment ID: " + mSpheresegID + "\n";
        textToDisplay += "Orientation: " + mOrientation;
        gameObject.GetComponent<TextMesh>().text = textToDisplay;
    }

    private int recalculateOrientation(Axis _axis) {
        switch(_axis) {
            case Axis.X:
                if(mOrientation > 4)
                    return mOrientation;
                else return mOrientation == 4 ? 1 : mOrientation + 1;
            case Axis.Y:
                if(mOrientation == 1 || mOrientation == 3)
                    return mOrientation;
                else if(mOrientation == 2)
                    return 6;
                else if(mOrientation == 4)
                    return 5;
                else if(mOrientation == 5)
                    return 2;
                else if(mOrientation == 6)
                    return 4;
                else return -1;
            case Axis.Z:
                if(mOrientation == 2 || mOrientation == 4)
                    return mOrientation;
                else if(mOrientation == 1)
                    return 5;
                else if(mOrientation == 3)
                    return 6;
                else if(mOrientation == 5)
                    return 3;
                else if(mOrientation == 6)
                    return 1;
                else return -1;
            default:
                return -1;
        }
    }

    private int mOrientation;
    private int mSpheresegID;
}
