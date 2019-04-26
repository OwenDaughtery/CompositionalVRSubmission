using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour {


    #region variables

    //Whether line is used by markov manager, if so, should not influence markov manager, or be able to be grabbed by user.
    public bool isMarkovManaged;

	//The attached script line renderer.
	public LineRenderer attachedLR;
	//The pulse manager in the game.
	private PulseManager pulseManager;
	//the actual game object that the script is attached too. (aka a baseline)
	private GameObject attachedObject;

	//an enum of possible voices, along with a variable to hold a voice enum.
	public enum Voices {None, A, B, C, D};
	public Voices voice;

	//the vertices of the line renderer.
	private Vector3[] vertices;

	//boolean as to whether the baseline is currently tethered to the tether point yet.
	//private bool isTethered = true;
	private bool isLineActive = true;

	//the object pool to get vertices from.
	private ObjectPooler objectPooler;
	
	public Dictionary<float, List<VertexManager>> timingDict;

	//int variable to hold the index of the last vertex that is played.
	[SerializeField]
	private int lastPlayedVertex;
	//the last height of what is essentially the "read head" of the system.
	private int lastHeight;
	//the local rotation of this baseline
	private float localRotation;
	private BoxColliderManager boxColliderManager;



	//possible colours where the letter corressponds to the voice enum.
	Color colourA = new Color(0.89f, 0.12f, 0.05f, 1);
	Color colourB = new Color(0.05f, 0.09f, 0.89f, 1);
	Color colourC = new Color(0.05f, 0.89f, 0.47f, 1);
    Color colourD = new Color(1f, 0.0f, 0.0f, 1);

    Color colourNone = new Color(1f, 1f, 1f, 1);
    


	#endregion

	void Start () {
		localRotation = gameObject.transform.eulerAngles.y;
		timingDict = new Dictionary<float, List<VertexManager>>();
		for (int i = -1; i <= GridManager.getYSegments(); i++){
			timingDict[i] = new List<VertexManager>();
		}
		lastPlayedVertex = -1;
		objectPooler = ObjectPooler.Instance;
		attachedObject = gameObject;
		Vector3[] currentVerts = getVertices();
		drawVerts(currentVerts);
		GameObject objectPulseManager = GameObject.FindGameObjectWithTag("PulseManager");
		pulseManager = objectPulseManager.GetComponent<PulseManager>();
		chooseVoice();

		boxColliderManager = this.GetComponent<BoxColliderManager>();
		pulseManager.activateLineManager(this);
	}

	void Update(){
	}

	public static float FindDegree(float x, float y){
      float value = ((float)((System.Math.Atan2(x, y) / System.Math.PI) * 180f));
      return value;
  	}	

	#region  getters and setters
	//get the variable local rotation (that represents how this empty object that this line renderer is a child of is rotated)
	public float getLocalRotation(){
		return localRotation;
	}

    public bool getIsMarkovManaged() {
        return isMarkovManaged;
    }

	//==MAY REMOVE THIS METHOD==
	public Dictionary<float, List<VertexManager>> getTimingDict(){
		return timingDict;

	}

	//update the timing dictionary by removing the vertex manager from the old key and inserting it into the new one.
	public void updateTimingDict(float oldTiming, float newTiming, VertexManager vm){
		timingDict[oldTiming].Remove(vm);
		timingDict[newTiming].Add(vm);
	}

	public int getNumberOfVertices(){
		return attachedLR.positionCount;
	}
	#endregion
	
	#region tethering
    /// <summary>
    /// Check if passed vertex is "tethered" by calling methods below.
    /// </summary>
    /// <param name="vertex">the gameobject to check.</param>
    /// <returns>bool of if vertex is tethered</returns>
	public bool checkIfTethered(GameObject vertex){
		return checkIfTetheredForY(vertex) && checkIfTetheredForXZ(vertex);
	}

    /// <summary>
    /// Check if passed vertex is "out of bounds" vertically.
    /// </summary>
    /// <param name="vertex">gameobject to check</param>
    /// <returns>bool if vertex is out of bounds</returns>
	public bool checkIfTetheredForY(GameObject vertex){
		return vertex.GetComponent<VertexManager>().getVertexTiming()==GridManager.getYSegments();
	}

    /// <summary>
    /// Check if passed vertex is "out of bounds" horizontally.
    /// </summary>
    /// <param name="vertex">gameobject to check</param>
    /// <returns>bool if vertex is out of bounds</returns>
	public bool checkIfTetheredForXZ(GameObject vertex){
		return vertex.GetComponent<VertexManager>().getVertexVolume()>0.955f;
	}

    #endregion

    #region voice and colour

    /// <summary>
    /// method called once to choose a random voice, and set the attached line renderer to the corresponding colour.
    /// </summary>
    private void chooseVoice(){
        if (isMarkovManaged) {
            voice = Voices.D;
            setVoiceColour();
        }
        else { 
		    voice = (Voices)Random.Range(1, (Voices.GetNames(typeof(Voices)).Length)-1);
		    setVoiceColour();
        }
    }

    /// <summary>
    /// method used to check the current assigned voice and set the attached line renderer to the corresponding colour.
    /// </summary>
    private void setVoiceColour(){
        if (voice == Voices.A)
        {
            attachedLR.SetColors(colourA, new Color(0.89f, colourA.g + 0.41f, 0.05f, 1));
        }
        else if (voice == Voices.B)
        {
            attachedLR.SetColors(colourB, new Color(colourB.g + 0.18f, colourB.r, 0.89f, 1));
        }
        else if (voice == Voices.C)
        {
            attachedLR.SetColors(colourC, new Color(0.05f, 0.89f, colourC.b - 0.28f, 1));
        }
        else if (voice == Voices.D) {
            attachedLR.SetColors(colourD, colourD);
        }
        else if (voice == Voices.None)
        {
            attachedLR.SetColors(colourNone, colourNone);
        }
	}

    /// <summary>
    /// get colour of the attached line renderer that corresponds to a voice.
    /// </summary>
    /// <returns>Color object different for each Enum voice.</returns>
    public Color getColourOfVoice(){
        if (voice == Voices.A)
        {
            return new Color(0.89f, colourA.g + 0.41f, 0.05f, 1);
        }
        else if (voice == Voices.B)
        {
            return new Color(colourB.g + 0.18f, colourB.r, 0.89f, 1);
        }
        else if (voice == Voices.C)
        {
            return new Color(0.05f, 0.89f, colourC.b - 0.28f, 1);
        }
        else if (voice == Voices.D) {
            return new Color(1f, 0f, 0f, 1);
        }
		return new Color(0,0,0,1);
	}

    /// <summary>
    /// simple method to get the voice variable of this line manager
    /// </summary>
    /// <returns>Voices enum</returns>
    public Voices getVoice(){
		return voice;
	}
    #endregion

    #region getting vertices (spheres and line renderer vertices)

    /// <summary>
    /// Method used to get the current vertices of the LineRenderer and return them as a Vector3[]
    /// </summary>
    /// <returns>Vector3[] of all vertices in the line renderer attached to this baseline.</returns>
    private Vector3[] getVertices(){
		vertices = new Vector3[attachedLR.positionCount];
		attachedLR.GetPositions(vertices);
		return vertices;

	}

    /// <summary>
    /// Method used to get vertices (spheres), and return them as a List<GameObject>
    /// </summary>
    /// <returns>A list of the gameobjects that are children of this gameobject that are vertices.</returns>
    public List<GameObject> getChildrenVertices(){
		 List<GameObject> childrenvertices = new List<GameObject>();

		 foreach(Transform child in transform){
			 if(child.tag == "Vertex"){
				childrenvertices.Add(child.gameObject);
			}
		 }
		 return childrenvertices;
	}

    /// <summary>
    /// method used to get all vertices (spheres), even ones that used to belong to this line manager but are being held by a controller.
    /// </summary>
    /// <returns>List of gameobjects that are children of this gameobject including any held by a controller</returns>
    private List<GameObject> getAllChildrenVerticesForInterpole(){
		List<GameObject> childrenVertices = getChildrenVertices();
		//if the currently tracked children equals the number that's expected, just return those (means controllers don't have this lines children)
		if(childrenVertices.Count==attachedLR.positionCount){
			return childrenVertices;
		}
		//get controllers
		GameObject[] controllers = GameObject.FindGameObjectsWithTag("GameController");
		foreach (GameObject controller in controllers){
			GameObject possibleChild = controller.GetComponent<InteractionManager>().getCurrentGameObject();
			//if controller has child, and child isn't in this list, and childs baseline parent is this gameobject:
			if(possibleChild && !childrenVertices.Contains(possibleChild) && possibleChild.GetComponent<VertexManager>().getBaseLineParent()==gameObject.transform){
				childrenVertices.Add(possibleChild);
			}
		}
		childrenVertices.Sort(sortByVertexID);
		return childrenVertices;
	}
    #endregion

    #region drawing verts (spheres)

    /// <summary>
    /// Method used to call drawVert(...) multiple time with the given Vector3[] of vertices
    /// </summary>
    /// <param name="currentVerts">a list of vertices to draw a new vertex</param>
    private void drawVerts(Vector3[] currentVerts){
		int vertexID=0;
		foreach (var vert in currentVerts){
			VertexManager newVert = drawVert(vert, vertexID, true).GetComponent<VertexManager>();
			if(vertexID==0){
				newVert.setVertexTiming(-1);
			}else if(vertexID==getNumberOfVertices()-1){
				newVert.setVertexTiming(GridManager.getYSegments());
			}
			vertexID++;
		}	
	}

    /// <summary>
    /// Method used to create a new vertex (which is a sphere) by grabbing one from pool
    /// </summary>
    /// <param name="vert">The vertex to create a vertex at</param>
    /// <param name="vertexID">the id of the vertex being created</param>
    /// <param name="isStatic">whether the vertex should be set to static or not.</param>
    /// <returns>GameObject of the newly created vertex</returns>
    private GameObject drawVert(Vector3 vert, int vertexID, bool isStatic){
		GameObject vertex = null;
		vertex = objectPooler.spawnFromPool("Vertex", vert, attachedLR.transform);
		
		//Get the object from pool, which will spawn it in, and set it's details to the parameters
		vertex.transform.name = "Vertex";
		vertex.AddComponent<VertexManager>();
		vertex.AddComponent<Rigidbody>();
		vertex.GetComponent<Rigidbody>().useGravity = false;
		vertex.GetComponent<Rigidbody>().isKinematic= true;
		vertex.GetComponent<VertexManager>().setVertexID(vertexID);
		vertex.GetComponent<VertexManager>().setParentsLineManager(this);
		
		return vertex;
	}

    /// <summary>
    /// given the rotation and a position, rotate that position using Quaternions.
    /// </summary>
    /// <param name="pos">Vector3 of the vertex</param>
    /// <param name="rotationFloat">the rotation of the baseline that the vector needs to pivot around</param>
    /// <returns>the new Vector3 that a vertex should be set too</returns>
    public Vector3 rotateVertex(Vector3 pos, float rotationFloat){
		Quaternion rotation = Quaternion.Euler(0,rotationFloat,0);
		Vector3 myVector = pos;
		Vector3 rotateVector = rotation * myVector;
		return rotateVector;
	}

    /// <summary>
    /// Method used to move the vertex of a linerenderer to a position
    /// </summary>
    /// <param name="index">The index of the vertex to change in this baseline</param>
    /// <param name="pos">the position that the vertex to change is at</param>
    /// <param name="eugerValueRotation">how much the vector needs to be rotated using rotateVertex method</param>
    public void moveLineVertex(int index, Vector3 pos, float eugerValueRotation){
		pos = rotateVertex(pos, -getLocalRotation());
		attachedLR.SetPosition(index, pos);
	}

    #endregion

    #region addVertex
    /// <summary>
    /// Add a vertex specified by the markov manager.
    /// </summary>
    /// <param name="note">The note of the vertex being added</param>
    /// <param name="timing">the timing of the vertex being added</param>
    /// <returns></returns>
    public GameObject addVertexFromMarkov(GridManager.Notes note, int timing){
        GameObject newVertex = null;
        float y = GridManager.getYFromTiming(timing);
        float z = 1f;
        float x = 1f;
        Vector3 pos = new Vector3(x, y, z);


        float angle = GridManager.convertNoteToAngle(note);
        
        //Calculating ID
        List<GameObject> children = getChildrenVertices();
        VertexManager vm;
        int previousID = -1;
        foreach (GameObject gameObject in children) {
            vm = gameObject.GetComponent<VertexManager>();
            if (vm.getVertexTiming() <= timing) {
                previousID = vm.getVertexID()+1;
            }
        }

        pos.x = 2.5f * Mathf.Sin(((angle+90)%360) * Mathf.Deg2Rad);
        pos.z = 2.5f * Mathf.Cos(((angle+90)%360) * Mathf.Deg2Rad);

        addVertex(pos, previousID, null);

        return newVertex;
    }

    /// <summary>
    /// Method used to add a new line renderer vertex and a new vertex sphere.
    /// </summary>
    /// <param name="pos">the position to add the new vertex too</param>
    /// <param name="vertexID">the id of the new vertex to be created</param>
    /// <param name="selectedVertex">the gameobject that may be being held by the user (may be null)</param>
    /// <returns></returns>
    public GameObject addVertex(Vector3 pos, int vertexID, GameObject selectedVertex){
		pos = rotateVertex(pos, -getLocalRotation());
		
		//Get the vertex spheres that are children of this object
		List<GameObject> children = getChildrenVertices();
		//Add the selected vertex to that list (as it won't be a child of the object if it's selected), and sort the list.
		if(selectedVertex){
			children.Add(selectedVertex);
		}
		
		children.Sort(sortByVertexID);

		//for loop to go through each vertex and update it's vertexID if it's PAST the selected vertex
		foreach(GameObject child in children){
			VertexManager childsVM = child.GetComponent<VertexManager>();
			if(childsVM.getVertexID()>=vertexID){
				childsVM.setVertexID(childsVM.getVertexID()+1);
			}
		}
		
		//create a new vertex sphere
		GameObject newVert = drawVert(pos, vertexID, false);
		VertexManager newVertsVM = newVert.GetComponent<VertexManager>();
		//make the new vertex's size and "length" field equal to the one currently selected
		if(selectedVertex){
			newVert.transform.localScale=selectedVertex.transform.lossyScale;
			newVertsVM.setVertexLength(selectedVertex.GetComponent<VertexManager>().getVertexLength());
		}
		//add this new vertex to the dictionary of vertexmanagers with its current timing
		timingDict[newVertsVM.getVertexTiming()].Add(newVertsVM);
		//add this new vertex to the list of children, and sort the list again.
		children.Add(newVert);
		children.Sort(sortByVertexID);
		//create a Vector3[] to hold the positions that the line renderer will be set to.
		Vector3[] finalPositions = new Vector3[children.Count];
		//translate the position of every child and add it to finalPositions.
		for (int i = 0; i < children.Count; i++){
			Vector3 tempPos = children[i].transform.position;
			tempPos = rotateVertex(tempPos, -getLocalRotation());
			//tempPos = vertexRotationTwo(tempPos, localRotation+270, i);
			finalPositions[i]=tempPos;	
		}
		
		//create a new vertex on the line renderer and set it's positions to finalPositions.
		attachedLR.positionCount+=1;
		attachedLR.SetPositions(finalPositions);
		
		boxColliderManager.addBoxCollider(vertexID);
		return newVert;
	}
    #endregion

    #region removeVertex

    /// <summary>
    /// method used to clear an entire line of all of it's vertices (both spheres and scripts)
    /// </summary>
    public void wipeLine() {
        VertexManager vm;
        List<GameObject> childrenCopy = new List<GameObject>(getChildrenVertices());
        foreach (GameObject gameObject in childrenCopy) {
            vm = gameObject.GetComponent<VertexManager>();
            if (vm.getVertexNote()!=GridManager.Notes.none && vm.getVertexID()!=1) {
                removeVertex(gameObject.transform.position, vm.getVertexID(), gameObject);
            }
        }
        
    }

    /// <summary>
    /// Method used to remove a vertex from a line and return it back to the pool
    /// </summary>
    /// <param name="pos">the vector of the vertex being removed</param>
    /// <param name="vertexID">the id of the vertex being removed</param>
    /// <param name="selectedVertex">the actual gameobject of the vertex being removed.</param>
    public void removeVertex(Vector3 pos, int vertexID, GameObject selectedVertex){
		boxColliderManager.removeBoxCollider(vertexID);

		//if statement to detect whether the vertex being removed is the last in the line.
		if(vertexID==attachedLR.positionCount-1){
			//Lower the position count by 1 and return the object to the pool.
			attachedLR.positionCount-=1;
			timingDict[selectedVertex.GetComponent<VertexManager>().getVertexTiming()].Remove(selectedVertex.GetComponent<VertexManager>());
			objectPooler.returnToPool("Vertex", selectedVertex);
		}else{
			//removing a vertex from the middle of a list
			//get the children of the lineBase and sort them
			List<GameObject> children = getChildrenVertices();
			children.Sort(sortByVertexID);
			
			//go through each vertex sphere that is a child of lineBase and lower it's vertexID by 1 if it is past the vertex being removed.
			foreach(GameObject child in children){
				VertexManager childsVM = child.GetComponent<VertexManager>();
				if(childsVM.getVertexID()>=vertexID){
					childsVM.setVertexID(childsVM.getVertexID()-1);
				}
			}

			//Translate the vector3 pos to take into account the position of the baseline object.
			pos=rotateVertex(pos, -getLocalRotation());

			//create a list of final positions that the line renderer vertices will be set too
			Vector3[] finalPositions = new Vector3[children.Count];
			for (int i = 0; i < children.Count; i++){
				Vector3 tempPos = children[i].transform.position;
				tempPos = rotateVertex(tempPos, -getLocalRotation());
				finalPositions[i]=tempPos;	
			}

			//lower the number of vertices in the line renderer by 1, and set the remaining vertices to finalPositions
			attachedLR.positionCount-=1;
			timingDict[selectedVertex.GetComponent<VertexManager>().getVertexTiming()].Remove(selectedVertex.GetComponent<VertexManager>());
			attachedLR.SetPositions(finalPositions);
			//return the removed vertex sphere back to the pool
			objectPooler.returnToPool("Vertex", selectedVertex);


		}	
	}
    #endregion

    #region utilities

    /// <summary>
    /// method used to take the voice id of this line manager, and cycle it by 1 along the list of voices.
    /// </summary>
    public void cycleVoices(){
        if (voice != Voices.D) {
            int voiceID = (int)voice;
            voiceID = (voiceID + 1) % Voices.GetNames(typeof(Voices)).Length;
            voice = (Voices)voiceID;
            if (voice == Voices.D)
            {
                voice = Voices.None;
            }

            setVoiceColour();
        }
	}

    /// <summary>
    /// simple sorter method to compare 2 vertices by their vertex id's
    /// </summary>
    /// <param name="v1">The first gameobject being compared</param>
    /// <param name="v2">the second gameobject being compared</param>
    /// <returns>which gameobjects vertexID is higher</returns>
    static int sortByVertexID(GameObject v1, GameObject v2){
		return v1.GetComponent<VertexManager>().getVertexID().CompareTo(v2.GetComponent<VertexManager>().getVertexID());
	}

    /// <summary>
    /// simple method that translates a given position by the position of the attached gameobject
    /// </summary>
    /// <param name="pos">Vector3 to translate to a new position</param>
    /// <returns>the passed paramter after translation</returns>
    private Vector3 VertexTranslation(Vector3 pos){
		Vector3 translation = attachedObject.transform.position;
		
		pos.x-=translation.x;
		pos.y-=translation.y;
		pos.z-=translation.z;

		return pos;
	}



    /// <summary>
    /// Method used to get each of the vertices in this entire line, and make them into a list of gridmanager.notes to return to trackManager.
    /// </summary>
    /// <returns>A list of GridManager.notes representing each note in turn on this line.</returns>
    public List<GridManager.Notes> getMelodyFromLine() {
        List<GridManager.Notes> melody = new List<GridManager.Notes>();
        VertexManager vm;
        List<GameObject> childVertices = getChildrenVertices();
        childVertices.Sort(sortByVertexID);
        foreach (GameObject child in getChildrenVertices()) {
            vm = child.GetComponent<VertexManager>();
            melody.Add(vm.getVertexNote());
        }
        return melody;
    }

    /// <summary>
    /// Similar to getMelodyFromLine, but this method adds in empty lists to represent spaces.
    /// </summary>
    /// <returns>A dictionary of ints representing timing of the experience which relate to a list of GridManager notes</returns>
    public Dictionary<int, List<GridManager.Notes>> getMelodyDictFromLine() {
        Dictionary<int, List<GridManager.Notes>> melodyDict = new Dictionary<int, List<GridManager.Notes>>();
        List<VertexManager> vertexManagers = new List<VertexManager>();
        foreach (GameObject gameObject in getChildrenVertices()) {
            vertexManagers.Add(gameObject.GetComponent<VertexManager>());
        }
        for (int i = 0; i < GridManager.getYSegments()-1; i++){
            melodyDict.Add(i, new List<GridManager.Notes>());
            foreach (VertexManager vm in vertexManagers){
                if (vm.getVertexID()!=1 && vm.getVertexNote()!=GridManager.Notes.none) {
                    if (vm.getVertexTiming() == i)
                    {
                        melodyDict[i].Add(vm.getVertexNote());
                    }
                }
                
            }
        }

        return melodyDict;
    }

    #endregion

    #region Pulse Methods

    /// <summary>
    /// method used for translating a pulse specifically, different from vertex translator.
    /// </summary>
    /// <param name="pos">The vector3 to be translated</param>
    /// <returns>The passed parameter after being translated</returns>
    private Vector3 PulseTranslation(Vector3 pos){
		Vector3 translation = attachedObject.transform.position;
		
		pos.x+=translation.x;
		pos.z+=translation.z;

		return pos;
	}

    /// <summary>
    /// method called by pulse manager to calculate a pulse should be on this line renderer. (and for creating new lead on pulses)
    /// </summary>
    /// <param name="height">The height that the pulse is at between 2 vertices on the line</param>
    /// <param name="playVertex">Whether a vertex needs to be played</param>
    /// <returns>the position a pulse should be now</returns>
    public Vector3 interpole(float height, bool playVertex){
		float lowerBoundTiming = float.MinValue;
		float upperBoundTiming = float.MaxValue;
		int lowerBoundIndex = 0;
		int upperBoundIndex = 0;
		int flooredHeight = Mathf.FloorToInt(height);

		//get all of the children, even ones attached to controllers.
		List<GameObject> childrenVertices = getAllChildrenVerticesForInterpole();
		childrenVertices.Sort(sortByVertexID);
		//first 1/3 of method used for deducing which 2 vertices the pulse should be between based on a given height.

		foreach (GameObject Vertex in childrenVertices){
			VertexManager currentVertexManager = Vertex.GetComponent<VertexManager>();
			if(currentVertexManager.getVertexTiming()>=lowerBoundTiming && currentVertexManager.getVertexTiming()<=height){
				lowerBoundTiming = currentVertexManager.getVertexTiming();
				lowerBoundIndex=currentVertexManager.getVertexID();
			}
			if(currentVertexManager.getVertexTiming()<upperBoundTiming && currentVertexManager.getVertexTiming()>height && currentVertexManager.getVertexTiming()!=upperBoundTiming){
				upperBoundTiming = currentVertexManager.getVertexTiming();
				upperBoundIndex = currentVertexManager.getVertexID();
			}
			
		} 
	
		if(lastHeight>flooredHeight && flooredHeight==0){
			if(playVertex){
				foreach(VertexManager vm in timingDict[0]){
					vm.playVertex();
				}
			}
		}
		else if(lastHeight<flooredHeight){
			
			float diffOfHeights = flooredHeight - lastHeight;
			if(playVertex && flooredHeight<GridManager.getYSegments()-1){
				foreach (VertexManager vm in timingDict[flooredHeight]){
					vm.playVertex();	
				}
			}
		}
		
		if(flooredHeight!=-1){
			lastHeight = flooredHeight;
		}
		
		float difference = upperBoundTiming - lowerBoundTiming;
		float segment = 1/difference;
		float leftover = height - lowerBoundTiming;
		int numberOfDivs = Mathf.FloorToInt(leftover);
		float t = (numberOfDivs * segment) + ((leftover%1) / difference);

		if(flooredHeight==-1){
			//lead on pulse:
			return Vector3.Lerp(PulseTranslation(attachedLR.GetPosition(0)),PulseTranslation(attachedLR.GetPosition(1)),1-(height*-1));
		}else{

			return Vector3.Lerp(PulseTranslation(attachedLR.GetPosition(lowerBoundIndex)),PulseTranslation(attachedLR.GetPosition(upperBoundIndex)), t);
			
		}
	}

	#endregion
	
}
