using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public enum Axis{X, Y, Z, NONE};

public class RubiksCube : MonoBehaviour{
    public GameObject cubePrefab;
    public float rotationSpeed = 1;
    public Text moveQueueText;
    public UnityEngine.UI.InputField moveIF;

    void Awake() {
        mAngleAcc = 0;
        mRotatingObjects = new List<GameObject>();
        mCurrAxis = Vector3.zero;
        mCommandQueue = new List<KeyValuePair<Axis, bool>>();

        mRubiksCube = new GameObject[2, 2, 2];
        mInitialCube = new GameObject[2, 2, 2];
        for(uint k = 0; k < 2; ++k) {
            for(uint i = 0; i < 2; ++i) {
                for(uint l = 0; l < 2; ++l) {
                    mRubiksCube[k, i, l] = (GameObject)Instantiate(cubePrefab);
                    mRubiksCube[k, i, l].transform.position = new Vector3(-0.5f + k, -0.5f + i, -0.5f + l);
                    mInitialCube[k, i, l] = mRubiksCube[k, i, l];
                }
            }
        }
    }

    // Use this for initialization
    void Start() {
	}
	
	// Update is called once per frame
	void Update(){
        if(Input.GetKeyDown(KeyCode.R)) {
            resetCube();
            return;
        }
        if(!Global.locked && !isIFFocused(moveIF))
            checkNewMoves();
        else if(isIFFocused(moveIF) && Input.GetKeyDown(KeyCode.Return)) {
            submitMoveIF();
        }

        if(mCommandQueue.Count == 0)
            rotateAxis(Axis.NONE, false);
        else {
            KeyValuePair<Axis, bool> currCommand = mCommandQueue[0];
            if(rotateAxis(currCommand.Key, currCommand.Value))
                mCommandQueue.RemoveAt(0);
        }

        moveQueueText.text = constructMoveQueueText();
	}

    public void addCommands(string _commands) {
        List<KeyValuePair<Axis, bool>> commands = interpretCommands(_commands);
        mCommandQueue.AddRange(commands);
    }

    public void removeLastMove() {
        mCommandQueue.RemoveAt(mCommandQueue.Count - 1);
    }

    public void addCommand(KeyValuePair<Axis, bool> command) {
        mCommandQueue.Add(command);
    }

    public bool rotateAxis(Axis _axis, bool _dirPositive){
        if(!rotate()){
            mRotatingObjects.Clear();

            if(_axis == Axis.NONE)
                return false;

            switch(_axis) {
                case Axis.X:
                    mCurrAxis = _dirPositive ? Vector3.right : Vector3.left;
                    rotx(_dirPositive);
                    break;
                case Axis.Y:
                    mCurrAxis = _dirPositive ? Vector3.up : Vector3.down;
                    roty(_dirPositive);
                    break;
                case Axis.Z:
                    mCurrAxis = _dirPositive ? Vector3.back : Vector3.forward;
                    rotz(_dirPositive);
                    break;
                default:
                    break;
            }

            return true;
        }


        return false;
    }

    private void resetCube() {
        for(uint k = 0; k < 2; ++k) {
            for(uint i = 0; i < 2; ++i) {
                for(uint l = 0; l < 2; ++l) {
                    mRubiksCube[k, i, l] = mInitialCube[k, i, l];
                    mRubiksCube[k, i, l].transform.rotation = Quaternion.identity;
                    mRubiksCube[k, i, l].transform.position = new Vector3(-0.5f + k, -0.5f + i, -0.5f + l);
                }
            }
        }
        mCommandQueue.Clear();
        Global.locked = false;
        mRotatingObjects.Clear();
        mAngleAcc = 0;
        mCurrAxis = Vector3.zero;
    }

    private void submitMoveIF() {
        addCommands(moveIF.text);
    }

    private List<KeyValuePair<Axis, bool>> interpretCommands(string _commands) {
        List<KeyValuePair<Axis, bool>> commands = new List<KeyValuePair<Axis, bool>>();
        foreach(char c in _commands) {
            switch(c) {
                case 'x':
                    commands.Add(new KeyValuePair<Axis, bool>(Axis.X, true));
                    break;
                case 'X':
                    commands.Add(new KeyValuePair<Axis, bool>(Axis.X, false));
                    break;
                case 'y':
                    commands.Add(new KeyValuePair<Axis, bool>(Axis.Y, true));
                    break;
                case 'Y':
                    commands.Add(new KeyValuePair<Axis, bool>(Axis.Y, false));
                    break;
                case 'z':
                    commands.Add(new KeyValuePair<Axis, bool>(Axis.Z, true));
                    break;
                case 'Z':
                    commands.Add(new KeyValuePair<Axis, bool>(Axis.Z, false));
                    break;
                default:
                    break;
            }
        }

        return commands;
    }

    private void checkNewMoves() {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
            if(Input.GetKeyDown(KeyCode.X))
                mCommandQueue.Add(new KeyValuePair<Axis, bool>(Axis.X, false));
            else if(Input.GetKeyDown(KeyCode.Y))
                mCommandQueue.Add(new KeyValuePair<Axis, bool>(Axis.Y, false));
            else if(Input.GetKeyDown(KeyCode.Z))
                mCommandQueue.Add(new KeyValuePair<Axis, bool>(Axis.Z, false));
        }
        else{
            if(Input.GetKeyDown(KeyCode.X))
                mCommandQueue.Add(new KeyValuePair<Axis, bool>(Axis.X, true));
            else if(Input.GetKeyDown(KeyCode.Y))
                mCommandQueue.Add(new KeyValuePair<Axis, bool>(Axis.Y, true));
            else if(Input.GetKeyDown(KeyCode.Z))
                mCommandQueue.Add(new KeyValuePair<Axis, bool>(Axis.Z, true));
        }
    }

    private bool rotate(){
        if(mRotatingObjects.Count == 0)
            return false;

        if(mAngleAcc >= 90) {
            mAngleAcc = 0;
            return false;
        }

        float currRotAmount = 90 * Time.deltaTime / rotationSpeed;
        foreach(GameObject rotObj in mRotatingObjects) {
            if(mAngleAcc + currRotAmount > 90)
                currRotAmount = 90 - mAngleAcc;

            rotObj.transform.RotateAround(Vector3.zero, mCurrAxis, currRotAmount);
        }
        mAngleAcc += currRotAmount;

        return true;
    }

    private void rotx(bool _dirPos) {
        GameObject[,,] newCubeState = new GameObject[2, 2, 2];

        
        for(int k = 0; k < 2; ++k) {
            for(int i = 0; i < 2; ++i) {
                newCubeState[1, k, i] = mRubiksCube[1, k, i];
                mRotatingObjects.Add(mRubiksCube[0, k, i]);
            }
        }

        if(!_dirPos) {
            newCubeState[0, 0, 0] = mRubiksCube[0, 1, 0];
            newCubeState[0, 1, 0] = mRubiksCube[0, 1, 1];
            newCubeState[0, 1, 1] = mRubiksCube[0, 0, 1];
            newCubeState[0, 0, 1] = mRubiksCube[0, 0, 0];
        } 
        else {
            newCubeState[0, 1, 0] = mRubiksCube[0, 0, 0];
            newCubeState[0, 1, 1] = mRubiksCube[0, 1, 0];
            newCubeState[0, 0, 1] = mRubiksCube[0, 1, 1];
            newCubeState[0, 0, 0] = mRubiksCube[0, 0, 1];
        }

        mRubiksCube = newCubeState;
    }

    private void roty(bool _dirPos) {
        GameObject[, ,] newCubeState = new GameObject[2, 2, 2];

        for(int k = 0; k < 2; ++k) {
            for(int i = 0; i < 2; ++i) {
                newCubeState[k, 1, i] = mRubiksCube[k, 1, i];
                mRotatingObjects.Add(mRubiksCube[k, 0, i]);
            }
        }

        if(!_dirPos) {
            newCubeState[0, 0, 0] = mRubiksCube[0, 0, 1];
            newCubeState[0, 0, 1] = mRubiksCube[1, 0, 1];
            newCubeState[1, 0, 1] = mRubiksCube[1, 0, 0];
            newCubeState[1, 0, 0] = mRubiksCube[0, 0, 0];
        } 
        else {
            newCubeState[0, 0, 1] = mRubiksCube[0, 0, 0];
            newCubeState[1, 0, 1] = mRubiksCube[0, 0, 1];
            newCubeState[1, 0, 0] = mRubiksCube[1, 0, 1];
            newCubeState[0, 0, 0] = mRubiksCube[1, 0, 0];
        }

        mRubiksCube = newCubeState;
    }

    private void rotz(bool _dirPos) {
        GameObject[, ,] newCubeState = new GameObject[2, 2, 2];

        for(int k = 0; k < 2; ++k) {
            for(int i = 0; i < 2; ++i) {
                newCubeState[k, i, 1] = mRubiksCube[k, i, 1];
                mRotatingObjects.Add(mRubiksCube[k, i, 0]);
            }
        }

        if(_dirPos) {
            newCubeState[0, 0, 0] = mRubiksCube[1, 0, 0];
            newCubeState[1, 0, 0] = mRubiksCube[1, 1, 0];
            newCubeState[1, 1, 0] = mRubiksCube[0, 1, 0];
            newCubeState[0, 1, 0] = mRubiksCube[0, 0, 0];
        }
        else {
            newCubeState[1, 0, 0] = mRubiksCube[0, 0, 0];
            newCubeState[1, 1, 0] = mRubiksCube[1, 0, 0];
            newCubeState[0, 1, 0] = mRubiksCube[1, 1, 0];
            newCubeState[0, 0, 0] = mRubiksCube[0, 1, 0];
        }

        mRubiksCube = newCubeState;
    }

    private string constructMoveQueueText() {
        string result = "Move Queue: ";
        foreach(KeyValuePair<Axis, bool> move in mCommandQueue){
            string currKey = "";
            switch(move.Key) {
                case Axis.X:
                    currKey = move.Value ? "x" : "X";
                    break;
                case Axis.Y:
                    currKey = move.Value ? "y" : "Y";
                    break;
                case Axis.Z:
                    currKey = move.Value ? "z" : "Z";
                    break;
                default:
                    break;
            }
            result += currKey;
        }

        return result;
    }

    private void lockInput() {
        Global.locked = true;
    }

    private void unlockInput() {
        Global.locked = false;
    }

    private bool isIFFocused(UnityEngine.UI.InputField _if) {
        GameObject currObj = EventSystem.current.currentSelectedGameObject;
        return (currObj != null && currObj.GetComponent<UnityEngine.UI.InputField>() == _if);
    }

    private float mAngleAcc;
    private Vector3 mCurrAxis;
    private GameObject[,,] mRubiksCube;
    private GameObject[,,] mInitialCube;
    private List<GameObject> mRotatingObjects;
    private List<KeyValuePair<Axis, bool>> mCommandQueue;
}
