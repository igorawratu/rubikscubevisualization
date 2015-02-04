using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Axis{X, Y, Z};

public class RubiksCube : MonoBehaviour{
    public GameObject cubePrefab;
    public int dimensions;

    void Awake() {
        mAngleAcc = 0;
        mRotatingObjects = new List<GameObject>();
        mCurrAxis = Vector3.zero;

        mRubiksCube = new GameObject[dimensions, dimensions, dimensions];
        for(uint k = 0; k < dimensions; ++k) {
            for(uint i = 0; i < dimensions; ++i) {
                for(uint l = 0; l < dimensions; ++l) {
                    mRubiksCube[k, i, l] = (GameObject)Instantiate(cubePrefab);
                }
            }
        }
    }

    // Use this for initialization
    void Start() {
	    
	}
	
	// Update is called once per frame
	void Update(){
	
	}

    public bool rotateAxis(int _index, Axis _axis, bool _dirNeg){
        if(_index >= dimensions)
            return true;

        if(!rotate()){
            mRotatingObjects.Clear();
            GameObject[,,] newCube = new GameObject[dimensions, dimensions, dimensions];

            for(int k = 0; k < dimensions; ++k){
                for(int i = 0; i < dimensions; ++i){
                    for(int l = 0; l < dimensions; ++l){
                        if(_axis == Axis.X && k == _index) {
                            mRotatingObjects.Add(mRubiksCube[k, i, l]);

                        } 
                        else if(_axis == Axis.Y && i == _index) {
                            mRotatingObjects.Add(mRubiksCube[k, i, l]);

                        } 
                        else if(_axis == Axis.Z && l == _index) {
                            mRotatingObjects.Add(mRubiksCube[k, i, l]);

                        } 
                        else newCube[k, i, l] = mRubiksCube[k, i, l];
                    }
                }
            }

            mRubiksCube = newCube;

            return true;
        }


        return false;
    }

    private bool rotate(){
        if(mRotatingObjects.Count == 0)
            return false;

        if(mAngleAcc >= Mathf.PI/2) {
            mAngleAcc = 0;
            return false;
        }

        foreach(GameObject rotObj in mRotatingObjects) {
            float currRotAmount = Mathf.PI/2 * Time.deltaTime;
            if(mAngleAcc + currRotAmount > Mathf.PI / 2)
                currRotAmount -= (mAngleAcc + currRotAmount) - Mathf.PI / 2;
            
            rotObj.transform.RotateAround(Vector3.zero, mCurrAxis, currRotAmount);
            mAngleAcc += currRotAmount;
        }

        return true;
    }

    private float mAngleAcc;
    private Vector3 mCurrAxis;
    private GameObject[,,] mRubiksCube;
    private List<GameObject> mRotatingObjects;
}
