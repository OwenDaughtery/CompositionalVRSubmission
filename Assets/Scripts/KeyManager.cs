using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Tilemaps;


public class KeyManager : MonoBehaviour { 
    //The range of possible keys that the system could be in
    private List<GridManager.Notes> possibleKeys = new List<GridManager.Notes>();
    //the circle of fifths that shows the notes in a scale for every key.
    private Dictionary<GridManager.Notes, List<GridManager.Notes>> circleOfFifths = new Dictionary<GridManager.Notes, List<GridManager.Notes>>();

    void Start(){
        //setting up possible keys and circleOfFifths.
        for (int i = 1; i <= 12; i++) {//c# might not be included here in later versions.
            possibleKeys.Add((GridManager.Notes)i);
        }
        foreach (GridManager.Notes key in possibleKeys){
            List<GridManager.Notes> tempscale = TrackManager.generateScale(key, 0);
            List<GridManager.Notes> scale = new List<GridManager.Notes>();
            for (int i = 0; i <= 6; i++){
                scale.Add(MarkovManager.clampToBottomOctave(tempscale[i]));
            }
            circleOfFifths.Add(key, scale);
        }
    }

    void Update(){
    }

    /// <summary>
    /// Given a melody from a tilemap, try to predict what key its in given a already established list of possible keys.
    /// </summary>
    /// <param name="melody">A list of list of GridManager enums representing all of the notes in a tilemap</param>
    /// <returns>A list of possible keys the user could be playing in.</returns>
    public GridManager.Notes adaptkey(Dictionary<int, List<GridManager.Notes>> melody) {
        List<GridManager.Notes> uniqueNotes = new List<GridManager.Notes>();
        Dictionary<GridManager.Notes, int> keyDistances = new Dictionary<GridManager.Notes, int>();
        foreach (KeyValuePair<GridManager.Notes, List<GridManager.Notes>> pair in circleOfFifths) {
            keyDistances.Add(pair.Key, 0);
        }
        keyDistances.Add(GridManager.Notes.none, 3);//Adding none with an int of X means that keys must have a distance of X or more to be considered "the key"
        //getting every unique note from the melody given by the user.
        foreach(KeyValuePair<int, List<GridManager.Notes>> pair in melody){
            foreach (GridManager.Notes note in pair.Value) {
                GridManager.Notes clampedNote = MarkovManager.clampToBottomOctave(note);
                if (!uniqueNotes.Contains(clampedNote))
                {
                    uniqueNotes.Add(clampedNote);
                }
            }
        }



        //for each unique note, check if it is in a scale from the possible keys.
        foreach (GridManager.Notes uniqueNote in uniqueNotes) {
            foreach (KeyValuePair<GridManager.Notes, List<GridManager.Notes>> pair in circleOfFifths) {
                if (pair.Value.Contains(uniqueNote)) {
                    keyDistances[pair.Key] += 1;
                }
                
            }
        }

        GridManager.Notes chosenKey = GridManager.Notes.none;
        foreach (KeyValuePair<GridManager.Notes, int> pair in keyDistances) {
            if (pair.Value >= keyDistances[chosenKey]) {
                chosenKey = pair.Key;
            }
        }

        print("predicted key is: " + chosenKey);
        return chosenKey;
    }
}
