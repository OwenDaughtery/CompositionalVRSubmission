using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseManager : MonoBehaviour {

	#region variables

	//how high the pulses should be
	private float height;
	//what speed the pulses should be travelling at
	[SerializeField]
	private float speed;
	//a list to hold all the line managers in the game
	private List<LineManager> lineManagers = new List<LineManager>();
	//a dictionary to hold each line manager and it's pulse (if it has one)
	private Dictionary<LineManager, List<GameObject>> LMtoPulse = new Dictionary<LineManager, List<GameObject>>();
	private Dictionary<GameObject, float> PulseToHeight = new Dictionary<GameObject, float>();
	//the object pool to get pulses from
	private ObjectPooler objectPooler;
    [SerializeField]
    KeyManager keyManager;
    [SerializeField]
    TrackManager trackManager;
    [SerializeField]
    MarkovManager markovManager;
    private int bars = 0;

	#endregion

	void Start () {
		GameObject[] baseLines = GameObject.FindGameObjectsWithTag("BaseLine");
		foreach (GameObject baseLine in baseLines){
			lineManagers.Add(baseLine.GetComponent<LineManager>());
		}
		height=-1;
		speed = GridManager.getYSegments()/8.5f;
		objectPooler = ObjectPooler.Instance;
		foreach (LineManager LM in lineManagers){
			LMtoPulse.Add(LM, new List<GameObject>());
        }
        if (markovManager.getPhase() == -1 && trackManager.getKey() != GridManager.Notes.none)
        {
            markovManager.advancePhase();//advance to learning phase
        }
    }
	
	void Update () {
		updatePulses();
	}

    #region updating functions
    /// <summary>
    /// method used to take each line manager, and if it has a pulse, make the position of that pulse = to a new position.
    /// </summary>
    public void updatePulses(){
		Vector3 newPulsePos;
		List<LineManager> linesToAddTo = new List<LineManager>();
		Dictionary<LineManager, GameObject> pulsesToRemove = new Dictionary<LineManager, GameObject>();
		List<LineManager> keys = new List<LineManager> (LMtoPulse.Keys);

		foreach(LineManager key in keys){
			foreach (GameObject pulse in LMtoPulse[key]){
				PulseToHeight[pulse] = PulseToHeight[pulse]+(Time.deltaTime * speed);
				if(PulseToHeight[pulse]>=GridManager.getYSegments()-1 && LMtoPulse[key].Count==1){
					//line manager needs to have a pulse added
					linesToAddTo.Add(key);
				}
				if(PulseToHeight[pulse]>=GridManager.getYSegments()){
					//line manager needs to have a pulse removed
					pulsesToRemove.Add(key, pulse);
				}else{
					//update the pulse position
					float rotation = key.getLocalRotation();
					newPulsePos = key.interpole(PulseToHeight[pulse], (LMtoPulse[key]!=null));
					newPulsePos = key.rotateVertex(newPulsePos, rotation);
					pulse.transform.position = newPulsePos;
				}
				
			}
		}

		foreach(LineManager lm in linesToAddTo){
			activateLineManager(lm);
		}


        //remove pulses, and then call adaptKey as "end of bar" has been reached.
		foreach (KeyValuePair<LineManager, GameObject> pair in pulsesToRemove){
			LMtoPulse[pair.Key].Remove(pair.Value);
			PulseToHeight.Remove(pair.Value);
			objectPooler.returnToPool("Pulse", pair.Value);
		}

        //end of bar reached:
        //if statement entered if the markov managed line is actually set to be markov managed.

        if (markovManager.getMarkovBaseLine().getIsMarkovManaged())
        {
            if (pulsesToRemove.Count != 0)
            {
                bars += 1;
                //key detection phase
                if (trackManager.getKey() == GridManager.Notes.none)
                {
                    trackManager.keyDetection();
                }//learning phase (aka phase 0)

                if (markovManager.getPhase() == 0 && bars % 4 == 0)
                {
                    markovManager.influenceChain();
                    markovManager.influenceRhythmChain();
                    //populate track with a suggestion
                    print("populate track from learning!");
                    bars = 0;
                    markovManager.populateTrack();
                    //print("as an example, a markov transition is: " + markovManager.getMarkovChain().getState(GridManager.Notes.C2).getTransition(GridManager.Notes.C2));
                }
                else if (markovManager.getPhase() == 1 && bars % 2 == 0)
                {
                    print("populate track from breeding!");
                    markovManager.populateTrack();
                    bars = 0;
                }
                else if (markovManager.getPhase() == 2 && bars % 2 == 0)
                {
                    print("populate track from reduction phase");
                    markovManager.populateTrack();
                    bars = 0;
                }

            }
        }
        
    }

    #endregion

    #region utilities

    /// <summary>
    /// "create" a pulse and add it to the dictionary.
    /// </summary>
    /// <param name="newLM">The LineManager script to create a pulse for</param>
    public void activateLineManager(LineManager newLM){
		GameObject pulse = createPulse(newLM);
		PulseToHeight.Add(pulse, -1);
		(LMtoPulse[newLM]).Add(pulse);
	}

    /// <summary>
    /// get a pulse object from the pool.
    /// </summary>
    /// <param name="LM">The LineManager receiving the pulse</param>
    /// <returns>The pulse object after being created.</returns>
    private GameObject createPulse(LineManager LM){
		GameObject pulse = objectPooler.spawnFromPool("Pulse", Vector3.zero, gameObject.transform);
		return pulse;
	}

	#endregion
}
