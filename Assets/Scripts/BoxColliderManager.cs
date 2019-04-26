using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderManager : MonoBehaviour {

	#region variables
	//the line manager of the baseline that this box collider is attached too
	LineManager lineManager;
	[SerializeField]
	//the line renderer that this box collider associates too
	LineRenderer attachedLR;
	private ObjectPooler objectPooler;
	[SerializeField]
	//a list of box colliders for the attached baseline.
	List<GameObject> lineColliders = new List<GameObject>();
	#endregion

	void Start () {
		lineManager = this.GetComponent<LineManager>();
		objectPooler = ObjectPooler.Instance;
		addBoxColliders();
	}
	
	void Update () {
		updateBoxColliders();
	}

    /// <summary>
    /// Rotation a collider based off the given position and rotation.
    /// </summary>
    /// <param name="pos">The position to rotate the collider around</param>
    /// <param name="rotationFloat">How far to rotate the collider</param>
    /// <returns></returns>
	public Vector3 rotateCollider(Vector3 pos, float rotationFloat){
		Quaternion rotation = Quaternion.Euler(0,rotationFloat,0);
		Vector3 myVector = pos;
		Vector3 rotateVector = rotation * myVector;
		return rotateVector;
	}

    #region collider methods
    /// <summary>
    /// simple method used to call box collider for every line in the attached line renderer.
    /// </summary>
    private void addBoxColliders(){
		for (int i = 0; i < attachedLR.positionCount-1; i++){
			addBoxCollider(i);
		}
	}

    /// <summary>
    /// method that creates a box collider, sets its variables, and sets it dimensions appropriately, and change the indexes of all other colliders appropriately.
    /// </summary>
    /// <param name="index">the index of a vertex that the box collider should "start" from</param>
    public void addBoxCollider(int index){
		Vector3 startPoint = attachedLR.GetPosition(index);
		Vector3 endPoint = attachedLR.GetPosition(index+1);
		GameObject newBoxCollider = objectPooler.spawnFromPool("BoxCollider", Vector3.zero, gameObject.transform);
		BoxCollider boxCollider = newBoxCollider.GetComponent<BoxCollider>();
		boxCollider.GetComponent<Trigger>().setIDs(index, index+1);
		moveBoxCollder(newBoxCollider, boxCollider, startPoint, endPoint);
		for (int i = index; i < lineColliders.Count; i++){
			lineColliders[i].GetComponent<Trigger>().setIDs(i+1, i+2);
			lineColliders[i].name = (i+1 + " - " + i+2);
		}
		newBoxCollider.name = (index) + " - " + (index+1);
		lineColliders.Add(newBoxCollider);
		lineColliders.Sort(sortByTriggerID);
	}

    /// <summary>
    /// given an index, remove the box collider that starts at that index, and change the indexes of all other colliders appropriately.
    /// </summary>
    /// <param name="index">The index of a vertex that the box collider "starts" from.</param>
    public void removeBoxCollider(int index){
		GameObject colliderToRemove = null;
		foreach (GameObject lc in lineColliders){
			Trigger trigger = lc.GetComponent<Trigger>();
			if(trigger.getStartID()==index){
				colliderToRemove=lc;
			}else if(trigger.getStartID()>=index){
	
				trigger.setIDs(trigger.getStartID()-1, trigger.getEndID()-1);
			}
		}
		lineColliders.Remove(colliderToRemove);
		objectPooler.returnToPool("BoxCollider", colliderToRemove);
	}

    /// <summary>
    /// move a box collider to the given start point and end point
    /// </summary>
    /// <param name="objectBoxCollider">The game object box collider to be moved</param>
    /// <param name="boxCollider">the box collider beloning to the game object being removed</param>
    /// <param name="startPoint">the vector3 that both the game object and boxCollider should "start" from</param>
    /// <param name="endPoint">the vector3 that both the game object and boxCollider should "end" at</param>
    private void moveBoxCollder(GameObject objectBoxCollider, BoxCollider boxCollider, Vector3 startPoint, Vector3 endPoint){
		startPoint = lineManager.rotateVertex(startPoint, lineManager.getLocalRotation());
		endPoint = lineManager.rotateVertex(endPoint, lineManager.getLocalRotation());

		Vector3 midPoint = (startPoint + endPoint)/2;
		boxCollider.transform.position = midPoint; 

		float lineLength = Vector3.Distance(startPoint, endPoint); 
		boxCollider.size = new Vector3(lineLength, attachedLR.endWidth+0.1f, attachedLR.endWidth+0.1f); 

		float angle = Mathf.Atan2((endPoint.z - startPoint.z), (endPoint.x - startPoint.x));
		angle *= Mathf.Rad2Deg;
		angle *= -1; 
		boxCollider.transform.LookAt(startPoint);
		Quaternion currentRot= boxCollider.transform.rotation;
		boxCollider.transform.Rotate(0,90,0);

	}

    /// <summary>
    /// move all of the box colliders that this script manages.
    /// </summary>
    private void updateBoxColliders(){
		int i = 0;
		foreach (GameObject lineCollider in lineColliders){
			moveBoxCollder(lineCollider, lineCollider.GetComponent<BoxCollider>(), attachedLR.GetPosition(i), attachedLR.GetPosition(i+1));
			i++;
		}
	}
    #endregion

    #region utilities
    /// <summary>
    /// simple method that is passed as a parameter when sorting by a variable.
    /// </summary>
    /// <param name="v1">The first game object to be sorted according to its "trigger" start id.</param>
    /// <param name="v2">The second game object to be sorted according to its "trigger" start id.</param>
    /// <returns></returns>
    static int sortByTriggerID(GameObject v1, GameObject v2){
		return v1.GetComponent<Trigger>().getStartID() .CompareTo (v2.GetComponent<Trigger>().getStartID());
	}
	#endregion
}
