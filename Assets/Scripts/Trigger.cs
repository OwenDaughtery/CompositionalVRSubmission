using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour {

	#region variables
	public int startID;
	public int endID;
    #endregion

    #region getters and setters
    /// <summary>
    /// simple method used to set the start and end ids of the object.
    /// </summary>
    /// <param name="newStartID">int of the StartID to set</param>
    /// <param name="newEndID">int of the EndID to set</param>
    public void setIDs(int newStartID, int newEndID){
		startID = newStartID;
		endID = newEndID;
	}

    /// <summary>
    /// simple method to get the start ID
    /// </summary>
    /// <returns>int reflecting the EndID</returns>
    public int getStartID(){
		return startID;
	}

    /// <summary>
    /// simple method to get the end ID
    /// </summary>
    /// <returns>int reflecting the EndID</returns>
    public int getEndID(){
		return endID;
	}

    #endregion

    #region utilities
    /// <summary>
    /// for every frame that a collider is in the box collider, call setHoveringBoxColliders on interaction manager.
    /// </summary>
    /// <param name="other">Collider that this object is colliding with.</param>
    public void OnTriggerStay(Collider other){
		if(other.tag=="GameController"){
			other.GetComponent<InteractionManager>().setHoveringBoxColliders(gameObject);
		}
	}
	#endregion
}
