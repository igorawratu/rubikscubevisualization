using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public enum Axis{X, Y, Z, NONE};

public class RubiksCube : MonoBehaviour{
    public GameObject[] quarters = new GameObject[8];
    public float rotationSpeed = 1;
    public Text moveQueueText;
    public UnityEngine.UI.InputField moveIF;
    public UnityEngine.UI.InputField shuffleIF;
    public int shuffleMoves = 20;
    public Text shuffleText;

    void Awake() {
        mAngleAcc = 0;
        mRotatingObjects = new List<GameObject>();
        mCurrAxis = Vector3.zero;
        mCommandQueue = new List<KeyValuePair<Axis, bool>>();
        mShuffleQueue = new List<KeyValuePair<Axis, bool>>();
        mRng = new System.Random();

        mRubiksCube = new GameObject[2, 2, 2];
        mInitialCube = new GameObject[2, 2, 2];
        int counter = 0;

        for(uint k = 0; k < 2; ++k) {
            for(uint i = 0; i < 2; ++i) {
                for(uint l = 0; l < 2; ++l) {
                    mRubiksCube[k, i, l] = (GameObject)Instantiate(quarters[counter++]);
                    mInitialCube[k, i, l] = mRubiksCube[k, i, l];
                    spheresegscript sss = mRubiksCube[k, i, l].transform.Find("orientation").GetComponent<spheresegscript>();
                    int ssid = getSSID(k, i, l);
                    int orientation = ssid < 5 ? 1 : 3;
                    sss.setSpheresegInfo(orientation, ssid);
                    sss.toggleText();
                }
            }
        }
    }

    // Use this for initialization
    void Start() {
	}
	
	// Update is called once per frame
	void Update(){
        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        updateParticleEffect();

        if(Input.GetKeyDown(KeyCode.R)) {
            resetCube();
            return;
        }
        if(mShuffleQueue.Count == 0 && !isIFFocused(moveIF))
            checkNewMoves();
        else if(isIFFocused(moveIF) && Input.GetKeyDown(KeyCode.Return)) {
            submitMoveIF();
        }

        List<KeyValuePair<Axis, bool>> currentActiveQueue = mShuffleQueue.Count > 0 ? mShuffleQueue : mCommandQueue;

        if(currentActiveQueue.Count == 0)
            rotateAxis(Axis.NONE, false);
        else {
            KeyValuePair<Axis, bool> currCommand = currentActiveQueue[0];
            if(rotateAxis(currCommand.Key, currCommand.Value))
                currentActiveQueue.RemoveAt(0);
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
                    mCurrAxis = _dirPositive ? Vector3.forward : Vector3.back;
                    rotz(_dirPositive);
                    break;
                default:
                    break;
            }

            return true;
        }


        return false;
    }

    public void shuffle() {
        int newShuffleAmount = 0;
        bool success = System.Int32.TryParse(shuffleIF.text, out newShuffleAmount);

        if(success)
            shuffleMoves = newShuffleAmount;

        resetCube();

        string[] possibleMoves = new string[6]{"x", "X", "z", "Z", "y", "Y"};
        string moves = "";

        for(int k = 0; k < shuffleMoves; ++k)
            moves += possibleMoves[mRng.Next(0, 6)];

        mShuffleQueue = interpretCommands(moves);

        shuffleText.text = "Shuffle moves: " + moves;
    }

    public void toggleText() {
        for(uint k = 0; k < 2; ++k) {
            for(uint i = 0; i < 2; ++i) {
                for(uint l = 0; l < 2; ++l) {
                    spheresegscript sss = mRubiksCube[k, i, l].transform.Find("orientation").GetComponent<spheresegscript>();
                    sss.toggleText();
                }
            }
        }
    }

    private void resetCube() {
        int counter = 0;

        for(uint k = 0; k < 2; ++k) {
            for(uint i = 0; i < 2; ++i) {
                for(uint l = 0; l < 2; ++l) {
                    mRubiksCube[k, i, l] = mInitialCube[k, i, l];
                    mRubiksCube[k, i, l].transform.rotation = quarters[counter].transform.rotation;
                    mRubiksCube[k, i, l].transform.position = quarters[counter++].transform.position;
                    spheresegscript sss = mRubiksCube[k, i, l].transform.Find("orientation").GetComponent<spheresegscript>();
                    int ssid = getSSID(k, i, l);
                    int orientation = ssid < 5 ? 1 : 3;
                    sss.setSpheresegInfo(orientation, ssid);
                }
            }
        }
        mCommandQueue.Clear();
        mRotatingObjects.Clear();
        mAngleAcc = 0;
        mCurrAxis = Vector3.zero;
        mShuffleQueue.Clear();
    }

    private void submitMoveIF() {
        addCommands(moveIF.text);
        moveIF.text = "";
        EventSystem.current.SetSelectedGameObject(null);
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
                spheresegscript sss = mRubiksCube[0, k, i].transform.Find("orientation").GetComponent<spheresegscript>();
                sss.rot(Axis.X, _dirPos);
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
                spheresegscript sss = mRubiksCube[k, 0, i].transform.Find("orientation").GetComponent<spheresegscript>();
                sss.rot(Axis.Y, _dirPos);
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
                spheresegscript sss = mRubiksCube[k, i, 0].transform.Find("orientation").GetComponent<spheresegscript>();
                sss.rot(Axis.Z, _dirPos);
            }
        }

        if(!_dirPos) {
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

    private bool isIFFocused(UnityEngine.UI.InputField _if) {
        GameObject currObj = EventSystem.current.currentSelectedGameObject;
        return (currObj != null && currObj.GetComponent<UnityEngine.UI.InputField>() == _if);
    }

    private void updateParticleEffect() {
        bool solved = isSolved();
        if(solved && !gameObject.GetComponent<ParticleSystem>().isPlaying) {
            gameObject.GetComponent<ParticleSystem>().Play();
        } else if(!solved && !gameObject.GetComponent<ParticleSystem>().isStopped) {
            gameObject.GetComponent<ParticleSystem>().Stop();
        }
        
    }

    private bool isSolved() {
        for(uint k = 0; k < 2; ++k) {
            for(uint i = 0; i < 2; ++i) {
                for(uint l = 0; l < 2; ++l) {
                    if(mRubiksCube[k, i, l] != mInitialCube[k, i, l])
                        return false;
                }
            }
        }

        return true;
    }

    private int getSSID(uint _x, uint _y, uint _z) {
        if(_x == 0 && _y == 1 && _z == 0)
            return 1;
        if(_x == 1 && _y == 1 && _z == 0)
            return 2;
        if(_x == 0 && _y == 1 && _z == 1)
            return 3;
        if(_x == 1 && _y == 1 && _z == 1)
            return 4;
        if(_x == 0 && _y == 0 && _z == 0)
            return 5;
        if(_x == 1 && _y == 0 && _z == 0)
            return 6;
        if(_x == 0 && _y == 0 && _z == 1)
            return 7;
        if(_x == 1 && _y == 0 && _z == 1)
            return 8;
        else return 0;
    }

    private float mAngleAcc;
    private Vector3 mCurrAxis;
    private GameObject[,,] mRubiksCube;
    private GameObject[,,] mInitialCube;
    private List<GameObject> mRotatingObjects;
    private List<KeyValuePair<Axis, bool>> mCommandQueue;
    private List<KeyValuePair<Axis, bool>> mShuffleQueue;
    private System.Random mRng;
}
