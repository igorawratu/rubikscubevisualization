using UnityEngine;
using System.Collections;

public enum Axis{X, Y, Z};

public class RubiksCube : MonoBehaviour{
    public GameObject cubePrefab;
    public int dimensions;

    void Awake() {
        rubiksCube = new GameObject[dimensions, dimensions, dimensions];
        for(uint k = 0; k < dimensions; ++k) {
            for(uint i = 0; i < dimensions; ++i) {
                for(uint l = 0; l < dimensions; ++l) {
                    rubiksCube[k, i, l] = (GameObject)Instantiate(cubePrefab);
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

    bool rotate(int _rowColIndex, bool _dirNeg, Axis _axis){
        if(_rowColIndex >= dimensions)
            return false;


        return true;
    }

    private GameObject[,,] rubiksCube;
}
