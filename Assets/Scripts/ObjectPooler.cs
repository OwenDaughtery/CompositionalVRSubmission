using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {

	//class Pool to hold prefabs with a tag and the amount to spawn at the start of the game.
	[System.Serializable]
	public class Pool{
		public string tag;
		public GameObject prefab;
		public int size;
	}

	#region variables

	//a list of the current pools
	public List<Pool> pools;
	//a dictionary holding a string, (the tag of a game object), and a queue of that game object
	public Dictionary<string, Queue<GameObject>> poolDictionary;
	public static ObjectPooler Instance;
    #endregion

    /// <summary>
    /// on wake up, go through each pool and create the neccassary number of prefabs of everything
    /// </summary>
    void Awake(){
		Instance = this;
		poolDictionary = new Dictionary<string, Queue<GameObject>>();
		foreach (Pool pool in pools){
			Queue<GameObject> objectPool = new Queue<GameObject>();
			for (int i = 0; i < pool.size; i++){
				GameObject obj = Instantiate(pool.prefab);
				obj.SetActive(false);
				objectPool.Enqueue(obj);
				obj.transform.parent=gameObject.transform;
			}
			poolDictionary.Add(pool.tag, objectPool);
		}
	}

    #region spawn from and return to pool

    /// <summary>
    /// takes a tag, position to set the gameobject to, and which gameobject is in charge of the given gameobject
    /// </summary>
    /// <param name="tag">The tag of the GameObject to get from the object pool</param>
    /// <param name="position">the position the gameobject should be set to</param>
    /// <param name="parent">the parent the gameobject will be a child of</param>
    /// <returns></returns>
    public GameObject spawnFromPool(string tag, Vector3 position, Transform parent){
		//"doesnt exist" error catching
		if(!poolDictionary.ContainsKey(tag)){
			Debug.LogWarning("Pool with tag: " + tag + " doesn't exist.");
			return null;
		}
		
		//"is empty" error catching
		if(poolDictionary[tag].Count==0){
			Debug.LogWarning("Pool with tag: " + tag + " is empty");
			return null;
		}

		//take an object from the queue, set it to active, set up its transform, and return it.
		GameObject objectToSpawn = poolDictionary[tag].Dequeue();
		objectToSpawn.SetActive(true);
		objectToSpawn.transform.parent = parent;
		objectToSpawn.transform.localPosition = position;

		return objectToSpawn;
	}

    /// <summary>
    /// method used to take an object and return it to it's pool for future use.
    /// </summary>
    /// <param name="tag">The tag of the object being returned</param>
    /// <param name="objectToReturn">The game object being returned</param>
    public void returnToPool(string tag, GameObject objectToReturn){
		//objectToReturn.transform.parent=null;
		objectToReturn.transform.parent=gameObject.transform;
		poolDictionary[tag].Enqueue(objectToReturn);
		objectToReturn.SetActive(false);

	}

	#endregion
	
}
