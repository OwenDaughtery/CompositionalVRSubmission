using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour {
    //How many seconds to wait inbetween moving the read head.
    float waitForSeconds = 0.15f;

    //How many octaves notes should be transposed up from the bottom octave.
    int octaveOffsetMultiplier = 3;

    //How many notes in an octave.
    int octaveOffset = 12;
    //Where the read head should start/where it currently is.
    int timingCount = -1;

    //Variable to hold the markovManager script.
    [SerializeField]
    private MarkovManager markovManager;

    //variable to hold the keyManager script.
    [SerializeField]
    private KeyManager keyManager;

    //Key of the system, determined by key manager during key detection phase.
    public GridManager.Notes key;

    //Static array to hold the semi-tone-steps of a major scale.
    private static int[] majorScale = new int[7] { 0, 2, 4, 5, 7, 9, 11 };

    void Start() {

    }

    /// <summary>
    /// Get current key of system.
    /// </summary>
    /// <returns>GridManager enum of current key.</returns>
    public GridManager.Notes getKey() {
        return key;
    }

    /// <summary>
    /// Method used to attempt to figure out what key user is playing in by inspecting the overall melody.
    /// </summary>
    public void keyDetection() {
        GridManager.Notes predictedKey = keyManager.adaptkey(getOverallMelody());

        if (predictedKey != GridManager.Notes.none)
        {
            print("Loop: successfully narrowed key down. Setting key.");
            key = predictedKey;
            markovManager.advancePhase();//entering learning phase
        }
        else
        {
            print("still not certain on key yet");
        }
    }

    /// <summary>
    /// Given a key, generates all of the notes in the scale of that key.
    /// </summary>
    /// <param name="key">The key of the scale to generate</param>
    /// <param name="octaveMultiplier">What octave the scale should start at.</param>
    /// <returns>A list of GridManager enums of the scale in the given key, Does multiple octaves according to gridheight(possible)</returns>
    public static List<GridManager.Notes> generateScale(GridManager.Notes key, int octaveMultiplier) {
        int scaleOffset = ((int)key);
        List<GridManager.Notes> scale = new List<GridManager.Notes>();
        //if no key has been selected:
        if (key == GridManager.Notes.none) {
        }
        else {
            foreach (int offset in majorScale) {
                scale.Add((GridManager.Notes)(offset + scaleOffset));
            }
        }
        return scale;
    }

    void Update() {
        updateReadHead();
    }

    /// <summary>
    /// Move read head along by 1.
    /// </summary>
    private void updateReadHead() {
        Vector3Int readHeadPos = new Vector3Int(timingCount, -1, 0);
    }

    /// <summary>
    /// Given an int, get the GridManager enum for that int.
    /// </summary>
    /// <param name="y">The passed int, must +1 to this int to take into account there is a dummy enum at the beginning of the Enum list for GridManager.</param>
    /// <returns>GridManager enum for the given int.</returns>
    private GridManager.Notes getNoteFromInt(int y) {
        y += 1;
        return((GridManager.Notes)y);
    }

    /// <summary>
    /// Main purpose of the class, a loop method that simulates the read head going round and round the music.
    /// </summary>
    /// <returns>Waits for however many seconds specified in the variable "waitForSeconds"</returns>
    IEnumerator Loop(){
        //How many bars have been played since the last reset of bars (For example: Every 4 bars do something and reset bars.)
        int bars = 0;
        List<GridManager.Notes> selectedNotes = new List<GridManager.Notes>();

        int phase;
        while (true) {
            //variable to keet track of read head.
            timingCount += 1;
            phase = markovManager.getPhase();

            //enter if statement if "readhead" has gone after y segments.
            if ((timingCount % GridManager.getYSegments() == 0) && timingCount>0) {
                bars+=1;
                //read head reset
                timingCount %= GridManager.getYSegments();
                //Given the users tilemap, influence the current markov chain and markovRhythm chain.
                if (phase != -1) {
                    markovManager.influenceChain();
                    markovManager.influenceRhythmChain();
                }


                //Check which phase system is in to do correct action.
                if (phase == -1)
                {
                    //Key detection Phase

                }
                else if (phase == 0)
                {
                    //Learning Phase
                    if (bars % 1 == 0)
                    {
                        //markovManager.populateTrack(markovChainMap, markovTileBase);
                        bars = 0;
                    }
                }
                else if (phase == 1)
                {
                    //Breeding Phase
                    if (bars % 1 == 0)
                    {
                        //markovManager.populateTrack(markovChainMap, markovTileBase);
                        bars = 0;
                    }
                }
                else if (phase == 2) {
                    if (bars % 1 == 0) {
                        //markovManager.populateTrack(markovChainMap, markovTileBase);
                        bars = 0;
                    }
                }
            

            }

           
            yield return new WaitForSeconds(waitForSeconds);

        }
        
    }

    /// <summary>
    /// Used to get all of the notes in the environment.
    /// </summary>
    /// <returns>A dictionary relating timing as int to a list of notes. Reprsenting the entire melody of the environment (not including markov managed line)</returns>
    public Dictionary<int, List<GridManager.Notes>> getOverallMelody() {
        Dictionary<int, List<GridManager.Notes>> overallMelody = new Dictionary<int, List<GridManager.Notes>>();
        Dictionary<int, List<GridManager.Notes>> lineMelody;

        for (int i = 0; i < GridManager.getYSegments(); i++){
            overallMelody[i] = new List<GridManager.Notes>();
        }

        GameObject[] baseLines = GameObject.FindGameObjectsWithTag("BaseLine");
        foreach (GameObject baseLine in baseLines) {
            //if statement to not add notes to melody if line is markov managed.
            if (!baseLine.GetComponent<LineManager>().getIsMarkovManaged()) { 
                lineMelody = baseLine.GetComponent<LineManager>().getMelodyDictFromLine();
                foreach (KeyValuePair<int, List<GridManager.Notes>> pair in lineMelody) {
                    foreach (GridManager.Notes note in pair.Value) {
                    
                        overallMelody[pair.Key].Add(note);
                    }
                }
            }
        }

        return overallMelody;
    }

    /// <summary>
    /// Method used to contact SuperCollider with a specific message to play a note.
    /// </summary>
    /// <param name="note"></param>
    public static void contactSC(GridManager.Notes note)
    {
        List<string> args = new List<string>();
        args.Add("0.3f");
        args.Add(GridManager.noteToFreq[note].ToString());
        args.Add("0.3f");

        //OSC Send
        OSCHandler.Instance.SendMessageToClient("SuperCollider", "/play" + "VoiceA", args);
      

    }


}
