using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

	#region variables
	private ObjectPooler objectPooler;	

	//A list for storing every Vertex (sphere) in the environment
	private GameObject[] allVertices;
	
	//3 variables that will hold the distance of a vertex (sphere) in relation to x, y, and z
	private float xDist;
	private float yDist;
	private float zDist;

	//3 vectors that will hold the single x, y, and z cordinate of a vertex (sphere)
	Vector3 xVector;
	Vector3 yVector;
	Vector3 zVector;

	//a list of all note boundaries
	private List<GameObject> noteBoundaries = new List<GameObject>();
	//==UNSURE IF THIS LIST IS NEEDED:==
	//a list of note boundaries that are fading in
	private List<GameObject> fadingInNoteBoundaries = new List<GameObject>();
	//==UNSURE IF THIS LIST IS NEEDED:==
	//a list of note boundaries that are fading out
	private List<GameObject> fadingOutNoteBoundaries = new List<GameObject>();
	//a dictionary of note boundaries that are fading out
	private Dictionary<GameObject, float> fadeOutDictionary = new Dictionary<GameObject, float>();


	/*plan is to remove these soon, they are temp debugging */
	private List<GameObject> timingBoundaries = new List<GameObject>();
	
	private VertexManager vertexManager;
	//how quickly should the note boundaries fade out
	private float fadeSpeed = 0.25f;

	//how many sections to split the y-axis into.
	private static int ySegments = 16;

	//Dictionary to hold a reference for converting a y cordinate to a timing int
	private static Dictionary<float, int> YToTiming = new Dictionary<float,int>(); 
	//Dictionary to hold a reference for converting a timing int into a y cordinate
	private static Dictionary<int, float> TimingToY = new Dictionary<int, float>();
	#endregion

	#region notes
	//The list of possible notes to play, always includes none at start.
	public enum Notes {none, C2, Cs2, D2, Ds2, E2, F2, Fs2, G2, Gs2, A2, As2, B2, C3, Cs3, D3, Ds3, E3, F3, Fs3, G3, Gs3, A3, As3, B3, C4, Cs4, D4, Ds4, E4, F4, Fs4, G4, Gs4, A4, As4, B4, C5, Cs5, D5, Ds5, E5, F5, Fs5, G5, Gs5, A5, As5, B5, C6, Cs6, D6, Ds6, E6, F6, Fs6, G6, Gs6, A6, As6, B6, C7};

	public static Dictionary<Notes, float> noteToFreq = new Dictionary<Notes, float>{
		{Notes.none, 0f},
		{Notes.C2, 65.41f},
		{Notes.Cs2, 69.30f},
		{Notes.D2, 73.42f},
		{Notes.Ds2, 77.78f},
		{Notes.E2, 82.41f},
		{Notes.F2, 87.31f},
		{Notes.Fs2, 92.50f},
		{Notes.G2, 98.00f},
		{Notes.Gs2, 103.83f},
		{Notes.A2, 110.00f},
		{Notes.As2, 116.54f},
		{Notes.B2, 123.47f},
		{Notes.C3, 130.81f},
		{Notes.Cs3, 138.59f},
		{Notes.D3, 146.83f},
		{Notes.Ds3, 155.56f},
		{Notes.E3, 164.81f},
		{Notes.F3, 174.61f},
		{Notes.Fs3, 185.00f},
		{Notes.G3, 196.00f},
		{Notes.Gs3, 207.65f},
		{Notes.A3, 220.00f},
		{Notes.As3, 233.08f},
		{Notes.B3, 246.94f},
		{Notes.C4, 261.626f},
		{Notes.Cs4, 277.183f},
		{Notes.D4, 293.665f},
		{Notes.Ds4, 311.127f},
		{Notes.E4, 329.628f},
		{Notes.F4, 349.228f},
		{Notes.Fs4, 369.994f},
		{Notes.G4, 391.995f},
		{Notes.Gs4, 415.305f},
		{Notes.A4, 440.000f},
		{Notes.As4, 466.164f},
		{Notes.B4, 493.883f},
		{Notes.C5, 523.251f},
		{Notes.Cs5, 554.365f},
		{Notes.D5, 587.330f},
		{Notes.Ds5, 622.254f},
		{Notes.E5, 659.255f},
		{Notes.F5, 698.456f},
		{Notes.Fs5, 739.989f},
		{Notes.G5, 783.991f},
		{Notes.Gs5, 830.609f},
		{Notes.A5, 880.000f},
		{Notes.As5, 932.328f},
		{Notes.B5, 987.767f},
		{Notes.C6, 1046.50f},
		{Notes.Cs6, 1108.73f},
		{Notes.D6, 1174.66f},
		{Notes.Ds6, 1244.51f},
		{Notes.E6, 1318.51f},
		{Notes.F6, 1396.91f},
		{Notes.Fs6, 1479.98f},
		{Notes.G6, 1567.98f},
		{Notes.Gs6, 1661.22f},
		{Notes.A6, 1760.00f},
		{Notes.As6, 1864.66f},
		{Notes.B6, 1975.53f},
		{Notes.C7, 2093.00f}
	};

	#endregion

	void Start () {
		float yIncrement = 3.3f/getYSegments();
		for (int i = 0; i <= getYSegments(); i++){
			YToTiming.Add(3.3f-(i*yIncrement),i);
			TimingToY.Add(i, 3.3f-(i*yIncrement));
		}
		objectPooler = ObjectPooler.Instance;
		xVector = new Vector3(0f, 0f, 0f);
		yVector = new Vector3(0f, 0f, 0f);
		zVector = new Vector3(0f, 0f, 0f);
		createNoteBoundaries();
		createTimingBoundaries();
	}
	
	void Update () {
		getVertexStats();
		tempFadeOut();
	}

    #region updating and setting up

    /// <summary>
    /// create all of the needed note boundaries
    /// </summary>
    private void createNoteBoundaries(){
        float segmentIncrement = 360/24;
        //float segmentIncrement = 360/(Notes.GetNames(typeof(Notes)).Length-2);
        float numberOfSegments = Mathf.CeilToInt(360/segmentIncrement);
		for (int i = 0; i < numberOfSegments; i++){
			GameObject noteBoundary = objectPooler.spawnFromPool("NoteBoundary", Vector3.zero, gameObject.transform);
			noteBoundaries.Add(noteBoundary);
			float eularAngleValue = i*segmentIncrement;
			if(eularAngleValue<0){
				eularAngleValue+=360;
			}
			noteBoundary.transform.eulerAngles = new Vector3(0f, eularAngleValue, 90f);
			noteBoundary.transform.position = rotateNoteBoundary(noteBoundary.transform.position, i*segmentIncrement);
			noteBoundary.SetActive(false);
		}
		
	}

    /// <summary>
    /// create all of the needed timing boundaries
    /// </summary>
    private void createTimingBoundaries(){
		float segmentIncrement = 3.3f/getYSegments();
		float numberOfSegments = getYSegments();
		for(int i = 0; i < numberOfSegments; i++){
			GameObject timingBoundary = objectPooler.spawnFromPool("NoteBoundary", Vector3.zero, gameObject.transform);
			timingBoundaries.Add(timingBoundary);
			float yHeight = 3.3f-(i*segmentIncrement);
			timingBoundary.transform.position = new Vector3(0f,yHeight,0f);
			timingBoundary.SetActive(false);
		}
	}

    /// <summary>
    /// method used to keep track of which note boundaries need to be removed
    /// </summary>
    public void tempFadeOut(){
		List<GameObject> keys = new List<GameObject> (fadeOutDictionary.Keys);
		foreach(GameObject key in keys) {
			fadeOutDictionary[key] = fadeOutDictionary[key]-0.01f;
			if(fadeOutDictionary[key]<=0){
				key.SetActive(false);
				fadeOutDictionary.Remove(key);
			}
		}
		
	}

	#endregion

	#region getters and setters
	public static int getYSegments(){
		return ySegments;
	}

	public static float getTimingFromY(float y){
		return YToTiming[y];
	}

	public static float getYFromTiming(int timing){
		return TimingToY[timing];
	}

    /// <summary>
    /// given a y cordinate, return the closest timing
    /// </summary>
    /// <param name="y">float of the y cordinate to be exmained</param>
    /// <returns>an int representing the closest timing to the passed y cordinate</returns>
    public static int getClosestTiming(float y){
		int lastValue = -1;
		foreach(KeyValuePair<float, int> pair in YToTiming){
				
		if(y>pair.Key){
			return (int)lastValue;
		}	
		lastValue = pair.Value;
		} 
		
		return (int)lastValue;
	}

    #endregion

    #region note boundary methods
    /// <summary>
    /// given a position and an angle, rotate the note.
    /// </summary>
    /// <param name="pos">The vector3 of the position of the note boundary</param>
    /// <param name="theta">float representing how far to rotate the note boundary</param>
    /// <returns>the vector3 of the new position of the note boundary</returns>
    private Vector3 rotateNoteBoundary(Vector3 pos, float theta){
		pos.x= 2.5f * Mathf.Sin(theta* Mathf.Deg2Rad);
		pos.z=2.5f * Mathf.Cos(theta* Mathf.Deg2Rad);
		return pos;
	}

    /// <summary>
    /// Method called when a vertex passes through a note boundary
    /// </summary>
    /// <param name="lastNote">The note that the vertex previously was</param>
    /// <param name="newNote">The note that the vertex now is</param>
	public void showNoteBoundaries(Notes lastNote, Notes newNote){
		int lastNoteID = (int)lastNote-1;
		int newNoteID = (int)newNote-1;
        //int lengthOfNotes = Notes.GetNames(typeof(Notes)).Length-2;
        int lengthOfNotes = 24;

		if((newNoteID==0 && lastNoteID==lengthOfNotes-1) || (lastNoteID==0 && newNoteID==lengthOfNotes-1)){

			noteBoundaries[6].SetActive(true);
			if(!fadeOutDictionary.ContainsKey(noteBoundaries[6])){
				fadeOutDictionary.Add(noteBoundaries[6], 1f);
			}
			
		}else if(lastNoteID<newNoteID){
			if(!fadeOutDictionary.ContainsKey(noteBoundaries[(newNoteID+6)%lengthOfNotes])){
				noteBoundaries[(newNoteID+6)%lengthOfNotes].SetActive(true);
				fadeOutDictionary.Add(noteBoundaries[(newNoteID+6)%lengthOfNotes], 1f);
			}
		}else if(lastNoteID>=newNoteID){
			if(!fadeOutDictionary.ContainsKey(noteBoundaries[(newNoteID+7)%lengthOfNotes])){
				noteBoundaries[(newNoteID+7)%lengthOfNotes].SetActive(true);
				fadeOutDictionary.Add(noteBoundaries[(newNoteID+7)%lengthOfNotes], 1f);
			}
		}
	}
    #endregion

    #region main

    /// <summary>
    /// the main method for this script, used to go through all vertices (spheres) in the environment and update their positions
    /// </summary>
    private void getVertexStats(){
		//get all vertices (spheres)
		allVertices = GameObject.FindGameObjectsWithTag("Vertex");
		foreach (GameObject vertex in allVertices){
			//get each vertexManager attached to current vertex (sphere)
			vertexManager = vertex.GetComponent<VertexManager>();

			//Set the vector variables according to the vertice (sphere) position. To do maths on later.
			xVector.Set(vertex.transform.position.x, 0f, 0f);
			yVector.Set(0f, vertex.transform.position.y, 0f);
			zVector.Set(0f, 0f, vertex.transform.position.z);

			//calculate the distance between the vertices (sphere) x, y, and z cords from the center.
			xDist = Vector3.Distance(gameObject.transform.position, xVector);
			yDist = Vector3.Distance(gameObject.transform.position, yVector);
			zDist = Vector3.Distance(gameObject.transform.position, zVector);
		
			if(vertexManager!=null && vertexManager.getVertexID()!=0 && vertexManager.getVertexID()!=vertexManager.getParentsLineManager().getNumberOfVertices()-1){
				//setting vertex volume by passing x^2 + z^2 to convert volume
				vertexManager.setVertexVolume(convertVolume((xDist*xDist) + (zDist*zDist)));

				//setting vertex timing by passing y distance.
				vertexManager.setVertexTiming(convertTiming(vertex.transform.position.y));

				//calling calculateAngle with the vertices (sphere) x and z cordinates, and then setting the vertices note accordingly.
				float vertexAngle = calculateAngle(vertexManager.transform.position.x, vertexManager.transform.position.z);
				vertexManager.setVertexAngle(vertexAngle);
				vertexManager.setVertexNote(convertAngle(vertexAngle));
			}
		

		}
	}

    #endregion

    #region volume related

    /// <summary>
    /// method used to "clamp" volume to range 0-1
    /// </summary>
    /// <param name="oldVolume">float representing the old volume of a vertex between 17 and 0.</param>
    /// <returns>the new clamped version of the vertex</returns>
    private float convertVolume(float oldVolume){
		return (1-((oldVolume - 0) / (17 - 0) * (1 - 0) + 0));
	}

    #endregion

    #region timing related

    /// <summary>
    /// method used to "clamp" volume to range 0-ySegments
    /// </summary>
    /// <param name="yDist">float representing the y cordinate of a vertex to be clamped.</param>
    /// <returns>float of the new clamped y cordinate.</returns>
    private float convertTiming(float yDist){
		int test;
		if(YToTiming.TryGetValue(yDist, out test)){
			test = YToTiming[yDist];
		}else{
			return getClosestTiming(yDist);
		}
		return test;
	}

    #endregion

    #region note related

    /// <summary>
    /// method used to calcluate the angle of a note to a certain point in the environment. by performing trig on x and z cordinate of vertex (sphere) and calling calculateQuad
    /// </summary>
    /// <param name="x">float representing x cordinate of a vertex</param>
    /// <param name="z">float representing z cordinate of a vertex</param>
    /// <returns>float representing the angle a vertex is to a certain point in the environment.</returns>
    static public float calculateAngle(float x, float z){
		int quad = calculateQuad(x, z);
		//converting to degrees
		float angle = (Mathf.Atan(z/x)* Mathf.Rad2Deg);

		//applying maths depending on which quarter the vertex (sphere) is in
		if(quad==0){
			angle = ((angle-90)*-1)+270;
		}else if(quad==1){
			angle = angle*-1;
		}else if(quad==2){
			angle = ((angle-90)*-1)+90;
		}else if(quad==3){
			angle = (angle*-1)+180;
		}
		return angle;
	}

    /// <summary>
    /// method that takes a float 0-360, and uses enum Notes to decide which enum to return.
    /// </summary>
    /// <param name="angle">The angle of a vertex to a certain point in the environment</param>
    /// <returns>The note that the vertex at the angle passed should be assigned.</returns>
    private Notes convertAngle(float angle){
		Notes note = Notes.none;
        float segmentIncrement = 360 / 24;
        //float segmentIncrement = 360/(Notes.GetNames(typeof(Notes)).Length-2);
        float currentSegment=0;
		int index =0;
		while(index<25){
			if(angle>=currentSegment && angle<currentSegment+segmentIncrement){
				note = (Notes)index+1;
			}
			index+=1;
			currentSegment+=segmentIncrement;
		}
        try
        {
            if (note == Notes.none) {
                print("none found at angle: " + angle);
                throw new System.Exception();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e, this);
            throw;
        }
        
		return note;
	}

    /// <summary>
    /// At what point the boundary of a certain note starts in the environment.
    /// </summary>
    /// <param name="note">The note to be converted into a float angle</param>
    /// <returns>float representing the position of a vertex with the passed not  should be in in the environment</returns>
    static public float convertNoteToAngle(GridManager.Notes note) {
        //float segmentIncrement = 360 / (Notes.GetNames(typeof(Notes)).Length - 2);
        float segmentIncrement = 360 / 24;
        int index = 1;
        int noteAsInt = ((int)note)-1;
        float currentSegment = segmentIncrement * (noteAsInt);
        return currentSegment;
    }

    /// <summary>
    /// method used to find out which quarter ("quadrant") a given cordinate is in.
    /// </summary>
    /// <param name="x">The x cordinate of the cordinate</param>
    /// <param name="z">the z cordinate of the cordinate</param>
    /// <returns>an int representing which quarter of the "circle" the cordinate is in.</returns>
    static public int calculateQuad(float x, float z){
		if(x>=0 && z>=0){
			return 0;
		}else if(x>=0 && z<0){
			return 1;
		}else if(x<0 && z<0){
			return 2;
		}else{
			return 3;
		}
	}
	#endregion


}
