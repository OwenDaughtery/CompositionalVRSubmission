using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VertexManager : MonoBehaviour, IPooledObject{

    #region variables

    //NOTE: Temporary text for fing what vertex is being held
    [SerializeField]
    Text text;

	//the line manager belonging to the parent of the attached gameobject
	[SerializeField]
	private LineManager parentsLineManager;
	//the transform of the baseline gameobject that the vertex sphere will belong too
	private Transform baselineParent;
	//whether the attached vertex is selected
	private bool isSelected;

	//ID of the vertex, unique ONLY to each baseline
	[SerializeField]
	private int vertexID;
	//a line renderer to point from where a vertex is, to where it will be snapped too.
	private LineRenderer tracer;
	//the object pool to get vertices from.
	private ObjectPooler objectPooler;
	//a variable to hold a light that will "blink" when the attached vertex is "played"
	private List<GameObject> playedLights;
	//how fast the above stated light should fade
	private float lightFade = 0.5f;
	private GridManager.Notes lastVertexNote;
	private float parentsRotation;
	private GridManager gridManager;

	//=====Sound Variables:=====
	[SerializeField]
	private float vertexAngle;
	[SerializeField]
	private float vertexVolume;
	[SerializeField]
	private float vertexTiming;
	[SerializeField]
	private float vertexLength;
	[SerializeField]
	private GridManager.Notes vertexNote;
	//=====End Sound Variables=====

	#endregion

	#region getters and setters
	public int getVertexID(){
		return vertexID;
	}

	public void setVertexID(int ID){
		vertexID=ID;
	}

	public void setIsSelected(bool newIsSelected){
		isSelected = newIsSelected;
	}

	public float getVertexTiming(){
		return vertexTiming;	
	}

	public void setVertexTiming(float newVertexTiming){
		float oldTiming = getVertexTiming();
		if(newVertexTiming<0){
			vertexTiming = -1;
		}else{
			vertexTiming = newVertexTiming;	
		}
		parentsLineManager.updateTimingDict(oldTiming, getVertexTiming(), this);
	}

	public float getVertexVolume(){
		return vertexVolume;	
	}

	public void setVertexVolume(float newVertexVolume){
		vertexVolume = newVertexVolume;	
	}	

	public void setVertexAngle(float newvertexAngle){
		vertexAngle = newvertexAngle;
	}

	public float getVertexLength(){
		return vertexLength;
	}

	public void setVertexLength(float newVertexLength){
		vertexLength = newVertexLength;
	}

	public GridManager.Notes getVertexNote(){
		return vertexNote;
	}

	public void setVertexNote(GridManager.Notes newVertexNote){
		vertexNote = newVertexNote;
	}

	public Transform getBaseLineParent(){
		return baselineParent;
	}

	public LineManager getParentsLineManager(){
		return parentsLineManager;
	}

	public void setParentsLineManager(LineManager newParentsLineManager){
		parentsLineManager = newParentsLineManager;
	}

	public float getVertexAngle(){
		return vertexAngle;
	}

	public float getParentsRotation(){
		return parentsRotation;
	}

	public void setLastVertexNote(GridManager.Notes newLastVertexNote){
		lastVertexNote = newLastVertexNote;
	}

	#endregion

	void Start(){
		objectPooler = ObjectPooler.Instance;
		setUp();
	}

	void Update () {
		//if statement to run code if the vertex is being "held"
		if(isSelected){
			float distance = Vector2.Distance(Vector2.zero, new Vector2(gameObject.transform.position.x, gameObject.transform.position.z));
			parentsLineManager.moveLineVertex(vertexID, gameObject.transform.position, parentsRotation);
			tracerUpdate();
			noteBoundaryUpdate(getVertexNote()); //NOTE: comment this out to not show note boundaries.
            text.text = getVertexNote().ToString();//set the text object to whatever this vertex ID is.
        }

		//update all of the lights in the scene of this vertex if any.
		if(playedLights.Count!=0){
			lightUpdate();
		}
	}

    #region set ups and updates

    /// <summary>
    /// the version of start for this script, sets up variables.
    /// </summary>
    public void OnObjectSpawn() {
		setUp();
	}

    /// <summary>
    /// method used for setting variables to appropriate values, will be called both on start and onObjectSpawn()
    /// </summary>
    private void setUp(){
        text = GameObject.FindGameObjectWithTag("Text").GetComponent<Text>();
        setLastVertexNote(GridManager.Notes.none);
		playedLights = new List<GameObject>();
		baselineParent=gameObject.transform.parent;
		parentsRotation=baselineParent.transform.eulerAngles.y;
		parentsLineManager = transform.parent.GetComponent<LineManager>();	
		gridManager = GameObject.FindGameObjectWithTag("Grid").GetComponent<GridManager>();
		isSelected=false;	
		vertexLength=1;

		//tracer setting up:
		tracer =  gameObject.AddComponent<LineRenderer>();
		tracer.positionCount=0;
    	Material tracerMaterial = (Material)Resources.Load("Materials/LineTexture");
		tracer.material = tracerMaterial;
		tracer.startWidth = 0.01f;
		tracer.endWidth = 0.01f;
		tracer.SetColors(Color.white, Color.white);
	}

    /// <summary>
    /// method used to set the values of the tracer
    /// </summary>
    private void tracerUpdate(){
		Vector3[] tracerPositions = new Vector3[2];
		tracerPositions.SetValue(gameObject.transform.parent.transform.position, 0);
		tracerPositions.SetValue(transform.parent.GetComponent<InteractionManager>().snap() , 1);
		tracer.SetPositions(tracerPositions);
	}

    /// <summary>
    /// method used to call methods to show note boundaries if they have been "passed".
    /// </summary>
    /// <param name="vertexNote">Get the vertex note that was just entered</param>
    private void noteBoundaryUpdate(GridManager.Notes vertexNote){
		if(lastVertexNote!=GridManager.Notes.none){
			if(lastVertexNote!=vertexNote){
				gridManager.showNoteBoundaries(lastVertexNote, vertexNote);
			}
		}
		setLastVertexNote(vertexNote);
	}

    #endregion

    #region light methods

    /// <summary>
    /// method used to get a playedLight object from the pool, and set its colour to the passed variable.
    /// </summary>
    /// <param name="colour">the colour to set the played list too</param>
    public void lightUpVertex(Color colour){
		GameObject playedLight = objectPooler.spawnFromPool("PlayedLight", Vector3.zero, gameObject.transform);
		playedLight.GetComponent<Light>().color=colour;
		playedLight.transform.name = "PlayedLight";
		playedLights.Add(playedLight);
	}

    /// <summary>
    /// decrease the intensity of the current lights, if it goes below 0, return it to the pool
    /// </summary>
    private void lightUpdate(){
		List<GameObject> lightsToRemove = new List<GameObject>();
		foreach (GameObject lightObject in playedLights){
			Light lightScript = lightObject.GetComponent<Light>();
			lightScript.intensity -= Time.deltaTime * lightFade;
			if(lightScript.intensity<=0){
				lightScript.intensity=1;
				lightsToRemove.Add(lightObject);
				objectPooler.returnToPool("PlayedLight", lightObject);
			}			
		}
		foreach (GameObject lightToRemove in lightsToRemove){
			playedLights.Remove(lightToRemove);
		}

	}

    #endregion

    #region get higher/lower vertex

    /// <summary>
    /// get the y cordinate and the timing of the next vertex up which is a sibling to this one.
    /// </summary>
    /// <param name="siblingY">The variable to hold the y cordinate above this vertex</param>
    /// <param name="siblingTiming">The variable to hold the timing above this vertex</param>
    public void getHigherVertex(out float siblingY, out float siblingTiming){
		List<GameObject> siblings = parentsLineManager.getChildrenVertices();
		foreach (GameObject sibling in siblings){
			int siblingID = sibling.GetComponent<VertexManager>().getVertexID();
			if(siblingID!=0 && siblingID!=getParentsLineManager().getNumberOfVertices()-1){
				if(siblingID==getVertexID()-1){
					
					//found the next vertex up
					siblingTiming = sibling.GetComponent<VertexManager>().getVertexTiming();
					siblingY = sibling.transform.position.y;
					return;
				}
			}
		}
		siblingTiming = float.MinValue;
		siblingY = float.MaxValue;
		return;
	}

    /// <summary>
    /// get the y cordinate and the timing of the next vertex down which is a sibling to this one (if there is one).
    /// </summary>
    /// <param name="siblingY">The variable to hold the y cordinate below this vertex</param>
    /// <param name="siblingTiming">The variable to hold the timing below this vertex</param>
    public void getLowerVertex(out float siblingY, out float siblingTiming){
		List<GameObject> siblings = parentsLineManager.getChildrenVertices();
		foreach (GameObject sibling in siblings){
			int siblingID = sibling.GetComponent<VertexManager>().getVertexID();
			if(siblingID!=0 && siblingID!=getParentsLineManager().getNumberOfVertices()-1){
				if(siblingID==getVertexID()+1){
					
					//found the next vertex up
					siblingTiming = sibling.GetComponent<VertexManager>().getVertexTiming();
					siblingY = sibling.transform.position.y;
					return;
				}
			}
		}
		siblingTiming = float.MaxValue;
		siblingY = float.MinValue;
		return;
	}

    #endregion

    #region utilities

    /// <summary>
    /// method used to return the sound variables of a vertex.
    /// </summary>
    /// <param name="note">The variable to hold the note of this vertex</param>
    /// <param name="timing">the variable to hold the timing of this vertex</param>
    /// <param name="volume">the variable to hold the volume of this vertex</param>
    /// <param name="length">the variable to hold the length of this vertex</param>
    /// <param name="voice">the varaible to hold the voice of this vertex</param>
    public void formatVertex(out GridManager.Notes note, out float timing, out float volume, out float length, out string voice){
		note = getVertexNote();
		timing = getVertexTiming();
		volume = getVertexVolume();
		length = getVertexLength();
		voice = "Voice" + getParentsLineManager().getVoice().ToString();
	}

    /// <summary>
    /// called when a vertex is picked up
    /// </summary>
    public void onPickUp(){
		tracer.positionCount+=2;
	}

    /// <summary>
    /// called when a vertex is let go.
    /// </summary>
    public void onPutDown(){
        text.text = "None";
		tracer.positionCount-=2;
	}

    /// <summary>
    /// method used to set a vertex (spheres) position to the passed parameter, and then move the associated line vertex to the same position
    /// </summary>
    /// <param name="newPos">the vector to move this gameobject too</param>
    public void moveTo(Vector3 newPos){
		gameObject.transform.position = newPos;
		parentsLineManager.moveLineVertex(getVertexID(), newPos, parentsRotation);
	}

    /// <summary>
    /// decrease the phyiscal size of the vertex as long as it's not too small already
    /// </summary>
    public void decreaseSize(){
		if((transform.lossyScale-new Vector3(0.01f, 0.01f, 0.01f)).x > 0.06f){
			transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
			setVertexLength(getVertexLength()-0.25f);
		}
		
	}

    /// <summary>
    /// increase the phyiscal size of the vertex as long as it's not too large already
    /// </summary>
    public void increaseSize(){
		if((transform.lossyScale-new Vector3(0.01f, 0.01f, 0.01f)).x < 0.161f){
			transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
			setVertexLength(getVertexLength()+0.25f);
		}
	}

    #endregion

    #region playing notes
    /// <summary>
    /// method that gathers all of the needed variables to play this note, and calls contactSC to talk to Super Collider
    /// </summary>
    public void playVertex(){
		if(getVertexID()>1 && getVertexID()!=getParentsLineManager().getNumberOfVertices()-1){
			lightUpVertex(parentsLineManager.getColourOfVoice());
			GridManager.Notes note;
			float timing;
			float volume;
			float length;
			string voice;
			formatVertex(out note, out timing, out volume, out length, out voice);
			contactSC(note,volume, length, voice);

			//==UNCOMMENT THIS LINE TO KNOW WHEN A VERTEX HAS BEEN PLAYED:==
			//print(note.ToString() + " " + volume.ToString());
		}
	}

    /// <summary>
    /// send a message to the OSC Client
    /// </summary>
    /// <param name="note">the note to be converted into a frequency.</param>
    /// <param name="volume">The float volume to send to the Client</param>
    /// <param name="length">The float length to send to the client</param>
    /// <param name="voice">The voice as a string to the client</param>
    public static void contactSC(GridManager.Notes note, float volume, float length, string voice){
		if(voice!="None"){
            if (voice != "VoiceA") {
                volume /= 3.5f;
            }
            if (voice == "VoiceD") {
                voice = "VoiceA";
            }
            note = (GridManager.Notes)(((int)note)+12);//adding another octave


            print("contacting SC: " + "(length: " + length + "), (note: " + note + "), (volume: " + volume + ")");
            //OSC Send
            List<string> args = new List<string>();
			args.Add(volume.ToString());
			args.Add(GridManager.noteToFreq[note].ToString());
			args.Add(length.ToString());


           
			OSCHandler.Instance.SendMessageToClient("SuperCollider", "/play"+voice, args);
           
		}

	}
	#endregion

}
