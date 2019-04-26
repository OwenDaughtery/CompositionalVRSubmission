using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//commented out tunes:
//E5,D5,C5,D5,E5,E5,E5
//Gs5, none, none, Gs5, Fs5, none, none, B5, A5, Gs5, A5, Gs5, none, none, E5, Fs5 

public class AISystemManager : MonoBehaviour {

	#region variables
	//==MAY REMOVE THIS VARIABLE:
	private LineManager[] lineManagers;

	//a fixed sized list of every note in the grid manager.
	[SerializeField]
	public static GridManager.Notes[] allNotes = new GridManager.Notes[GridManager.noteToFreq.Count];
	//the increments of semitones for a major scale.
	private static int[] majorScale = new int[8] {0,2,4,5,7,9,11,12};
	//the increments of semitones for a minor scale.
	private static int[] minorScale=  new int[8] {0,2,3,5,7,8,11,12};
	//what key the AI system is in.
	public GridManager.Notes key = GridManager.Notes.none;
	//given the key the AI system is in, what notes are avaible (this will not include all octaves)
	public List<GridManager.Notes> availableNotes;
	//whether the AI system should generate with major notes in mind
	public bool major = true;
	//BUTTON: "press" to play the scale of available notes.
	public bool playScaleButton = false;
	//FLIPSWITCH: "flip" to play the tune inputted by the user.
	public bool playButton = false;
	public bool playBaseScore = true;
	//FLIPSWITCH: "flip" to include randomly generated notes in the currently played music.
	public bool randomNoteProduction = false;
	//FLIPSWITCH: "flip" to include markov chain notes in the currently played music.
	public bool markovNoteProduction = false;
	public bool markovChordProduction = false;
	//FLIPSWITCH: "flip" to include harmonised notes of USERS input.
	public bool harmonizeInputNoteProduction = false;
	//FLIPSWITCH: "flip" to include harmonised notes of MARKOV CHAIN input
	public bool harmonizeMarkovNoteProduction = false;
	//String field to allow for input of notes
	public string notesToInput = "";
	//BUTTON: "press" to input the string in "notesToInput"
	public bool inputNotes = false;
	//Coroutine variable to hold certain coroutines in the methods.
	Coroutine coroutine = null;
	//FLIPSWITCH: "flip" to run the calculate key method.
	public bool calculateKeyButton = false;
	//random number generator.
	private System.Random random = new System.Random();
	//the last key that the system was in, used to detect a key change.
	private GridManager.Notes lastKey = GridManager.Notes.none;
	//the master score of the system, which the user will input notes too.
	public Dictionary<int, List<GridManager.Notes>> masterScore = new Dictionary<int, List<GridManager.Notes>>();

	#endregion

	void Start(){
		populateAllNotes();
		setUpMasterScore();
	}


	void Update(){
		//key has been changed and available notes needs to change
		if(lastKey!=key && key!=GridManager.Notes.none){
			setAvailableNotes();
		}

		//start playing the master score.
		if(playButton){
			if(coroutine==null){
				coroutine = StartCoroutine(playInput());
			}
		}

		//play all the available notes.
		if(playScaleButton && key!=GridManager.Notes.none){
			StartCoroutine(playScale());
			playScaleButton=false;
		}

		//take the notes in notesToInput, and convert them to GridManager.Notes
		if(inputNotes){
			List<GridManager.Notes> inputAsNotes = convertStringToNotes(notesToInput);
			notesToInput="";
			inputNotes=false;
			int index=0;
			foreach (GridManager.Notes note in inputAsNotes){
				if(note!=GridManager.Notes.none){
					masterScore[index].Add(note);
				}
				index+=4;
			}
		}
		lastKey=key;
	}

    #region setup:

    /// <summary>
    /// create 64 keys in the master score to hold a list of notes.
    /// </summary>
    private void setUpMasterScore(){
		for (int i = 0; i < 64; i++){
			masterScore.Add(i, new List<GridManager.Notes>());
		}
	}


    /// <summary>
    /// taking the key the system is in and major/minor, create all of the available notes for that key.
    /// </summary>
    private void setAvailableNotes(){
		int offset = (((int)key)-1)%12;
		availableNotes = new List<GridManager.Notes>();
		int numberOfOctaves = 3;
		if(major){
			for(int octave = 0; octave<=numberOfOctaves; octave++){
				for (int i = 0; i < majorScale.Length-1; i++){
					try{
						availableNotes.Add(allNotes[majorScale[i%majorScale.Length]+(12*octave)+(offset)]);
					}catch (System.Exception){
						Debug.Log("Run out of notes for key");
						break;
						
					}
				}
				
			}
			try{
				availableNotes.Add(allNotes[majorScale[majorScale.Length-1]+(12*numberOfOctaves)+(offset)]);
			}catch (System.Exception){
				Debug.Log("Run out of notes for key");
				
			}
			
		}else{
			for(int octave = 0; octave<=numberOfOctaves; octave++){
				for (int i = 0; i <= minorScale.Length-1; i++){
					try{
						availableNotes.Add(allNotes[minorScale[i%minorScale.Length]+(12*octave)+(offset)]);
					}catch (System.Exception){
						Debug.Log("Run out of notes for key");
						break;
						
					}
				}
			}	
			try{
				availableNotes.Add(allNotes[majorScale[7%majorScale.Length-1]+(12*numberOfOctaves)+(offset)]);
			}catch (System.Exception){
				Debug.Log("Run out of notes for key");
			}
		}
	}

    /// <summary>
    /// take all of the notes in GridManager.noteToFreq and add them to the allNotes list.
    /// </summary>
    private void populateAllNotes(){
		int i=0;
		foreach (KeyValuePair<GridManager.Notes, float> pair in GridManager.noteToFreq){
			if(pair.Key!=GridManager.Notes.none){
				allNotes[i] = pair.Key;
				i+=1;
			}

		}
	}

    #endregion

    #region Generation Methods
    /// <summary>
    /// given an input, harmonize each note at the keys divisble by keysToHarmonise, add those harmonisations to harmonizedInput, and return that.
    /// </summary>
    /// <param name="input">The notes to be harmonised</param>
    /// <param name="harmonizedInput">the Dictionary to add the harmonised versions of the notes too</param>
    /// <returns></returns>
    private Dictionary<int, List<GridManager.Notes>> harmonizeInput(Dictionary<int, List<GridManager.Notes>> input, Dictionary<int, List<GridManager.Notes>> harmonizedInput){
		int keysToHarmonise = 4;
		wipeDict(harmonizedInput);
		int[] upperOrLower = new int[2]{2,-2};
		Dictionary<int, List<GridManager.Notes>> notesToAdd = new Dictionary<int, List<GridManager.Notes>>();
		
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in input){
			notesToAdd.Add(pair.Key, new List<GridManager.Notes>());
			foreach (GridManager.Notes note in pair.Value){
				if(pair.Key%keysToHarmonise==0){
					try{
					
						int indexInScale = availableNotes.IndexOf(note);
						
						int r = random.Next(upperOrLower.Length);
						GridManager.Notes newHarmonizedNote = getNoteFromInt(indexInScale+upperOrLower[r]);
						
						notesToAdd[pair.Key].Add(newHarmonizedNote);
					}catch (System.Exception){
						Debug.Log("ERROR: Note does not index in available notes!");	
						break;
						throw;
					}
				}
				
			}
		}

		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in notesToAdd){
			foreach (GridManager.Notes note in pair.Value){		
				harmonizedInput[pair.Key].Add(note);
			}
		}		
		return harmonizedInput;
	}


    /// <summary>
    /// given dictionary to populate and what bar the system is currently on, randomly add and randomly remove notes from the passed dictionary.
    /// </summary>
    /// <param name="randomNotes">The dictionary to randomly iterate over</param>
    /// <param name="bar">int representing the "bar" the system is currently on</param>
    /// <returns>Returns the given dictionary after being iterated over</returns>
    private Dictionary<int, List<GridManager.Notes>> generateRandomNotes(Dictionary<int, List<GridManager.Notes>> randomNotes, int bar){
		int numberOfNotesAdded = 0;
		int numberOfNotesRemoved = 0;
		System.Random randomNoteChooser = new System.Random();
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in randomNotes){
			GridManager.Notes newRandomNote = GridManager.Notes.none;
			int r = randomNoteChooser.Next(0,100);
			if(pair.Value.Count!=0){
				if(r>10){
					numberOfNotesRemoved+=1;
					pair.Value.RemoveAt(0);
				}
			}
			if(r>(Mathf.Max(bar,20))){
				numberOfNotesAdded+=1;
				newRandomNote = getRandomNoteInKey();
				pair.Value.Add(newRandomNote);
			}
		}
		print("Number Of Notes Added: " + numberOfNotesAdded);
		print("Number Of Notes Removed: " + numberOfNotesRemoved);
		return randomNotes;
	}

    /// <summary>
    /// generate the corpus/markov-chain that will be used as a finite state machine to generate notes later on,
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private Dictionary<GridManager.Notes, ChainLink> generateMarkovChain(Dictionary<int, List<GridManager.Notes>> input){
		Dictionary<GridManager.Notes, ChainLink> markovChain = new Dictionary<GridManager.Notes, ChainLink>();
		List<GridManager.Notes> encounteredNotes = new List<GridManager.Notes>();
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in input){
			foreach (GridManager.Notes note in pair.Value){
				if(!markovChain.ContainsKey(note)){
					markovChain.Add(note, new ChainLink(note));
				}
				int futureKey = pair.Key+1;
				bool nextNotesNotFound = true;
				while(futureKey<input.Count && nextNotesNotFound){
					if(input[futureKey].Count>0){
						foreach (GridManager.Notes futureNote in input[futureKey]){
							markovChain[note].addToPath(futureNote);
						}
						nextNotesNotFound=false;
					}
					futureKey+=1;

					
				}
			}
		}
		return markovChain;
	}

	//given a markov chain, step through the process of creating news notes like a finite-state-machine.
	private Dictionary<int, List<GridManager.Notes>> generateMarkovNotes(Dictionary<int, List<GridManager.Notes>> input, Dictionary<GridManager.Notes, ChainLink> markovChain, Dictionary<int, List<GridManager.Notes>> markovNotes){
		wipeDict(markovNotes);
		
		//int startingBeat = getEndOfInput(input)+4;
		int startingBeat = 0;
		List<GridManager.Notes> allNotes = getAllNotesOfInput(input);
		GridManager.Notes nextNote = allNotes[random.Next(allNotes.Count)];
		markovNotes[startingBeat].Add(nextNote);
		for (int i = startingBeat+4; i < markovNotes.Count; i+=4){
			nextNote = markovChain[nextNote].getNextNote();
			
			if(markovChordProduction){
				markovNotes[i].Add(nextNote);
				if(i%16==0){
					foreach (GridManager.Notes note in generateChord(nextNote)){
						if(nextNote!=note){
							markovNotes[i].Add(note);
						}
						
					}
				}

			}else{
				markovNotes[i].Add(nextNote);
			}
			
		}
		return markovNotes;
	}
	#endregion

	#region utilities
	private List<GridManager.Notes> generateChord(GridManager.Notes note){
		List<GridManager.Notes> chord = new List<GridManager.Notes>();
		chord.Add(note);
		if(major){
			chord.Add((GridManager.Notes)(((int)note)+4));
			chord.Add((GridManager.Notes)(((int)note)+7));
		}else{
			chord.Add((GridManager.Notes)(((int)note)+3));
			chord.Add((GridManager.Notes)(((int)note)+7));
		}
		return chord;
	}
	
	/*
	private int decideNextBeat(Dictionary<int, List<GridManager.Notes>> input, int timing){

	}*/

	//method called by update to convert the given string into notes, outputs a log warning if this is not possible.
	private List<GridManager.Notes> convertStringToNotes(string notesToInput){
		List<GridManager.Notes> convertedNotes = new List<GridManager.Notes>();
		//notesToInput = notesToInput.ToUpper();
		string[] splitString = notesToInput.Split(',');
		foreach (string S in splitString){
			try{
				convertedNotes.Add((GridManager.Notes)GridManager.Notes.Parse(typeof(GridManager.Notes), S));
			}
			catch (System.Exception)
			{
				Debug.LogWarning("ERROR - Cannot convert string to enum, invalid characters. returning empty.");
				return new List<GridManager.Notes>();
			}
			
		}
		return convertedNotes;
	}

	//given an index for AVAILABLE NOTES, return a note.
	private GridManager.Notes getNoteFromInt(int index){
		int newIndex = (index)%(availableNotes.Count);
		return availableNotes[newIndex];
	}

	//get a random note from available notes.
	private GridManager.Notes getRandomNoteInKey(){
		int r = random.Next(availableNotes.Count);
		return availableNotes[r];
	}

	//return all of the notes that appear inside the given dictionary (note: each note will only be returned once).
	private List<GridManager.Notes> getAllNotesOfInput(Dictionary<int, List<GridManager.Notes>> input){
		List<GridManager.Notes> allNotes = new List<GridManager.Notes>();
		foreach(KeyValuePair<int, List<GridManager.Notes>> pair in input){
			foreach(GridManager.Notes note in pair.Value){
				if(!allNotes.Contains(note)){
					allNotes.Add(note);
				}
			}
		}
		return allNotes;
	}

	//a method to get the last key(int) of the given input.
	private int getEndOfInput(Dictionary<int, List<GridManager.Notes>> input){
		int endOfInput = 0;
		foreach(KeyValuePair<int, List<GridManager.Notes>> pair in input){
			if(pair.Value.Count>0){
				endOfInput=pair.Key;
			}
		}
		return endOfInput;
	}	

	//given a note, play it on Super Collider.
	private void playNote(GridManager.Notes note){
		print("AISystemManager: " + note);
		VertexManager.contactSC(note, 0.75f,0.5f,"VoiceA");	
	}

	//given a dictionary where the values of that dictionary are lists, wipe the lists.
	private Dictionary<int, List<GridManager.Notes>> wipeDict(Dictionary<int, List<GridManager.Notes>> dictToWipe){
		//following code wips previous notes from last loop:
		for (int i = 0; i < dictToWipe.Count; i++){
			dictToWipe[i] = new List<GridManager.Notes>();
		}

		return dictToWipe;
	}

	//concate 2 lists together.
	private List<GridManager.Notes> concat(List<GridManager.Notes> a, List<GridManager.Notes> b){
		List<GridManager.Notes> listToReturn = new List<GridManager.Notes>();
		foreach (GridManager.Notes note in a){
			listToReturn.Add(note);	
		}
		foreach (GridManager.Notes note in b){
			listToReturn.Add(note);	
		}
		return listToReturn;
	}


	#endregion

	#region coroutines
	//main loop, based off the flipswitch variables, play the master score along with all the additional features of the system.
	IEnumerator playInput(){
		Dictionary<int, List<GridManager.Notes>> input =  masterScore;
		Dictionary<int, List<GridManager.Notes>> harmonizedInputNotes = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> harmonizedMarkovNotes = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> randomNotes = new Dictionary<int, List<GridManager.Notes>>();
		Dictionary<int, List<GridManager.Notes>> markovNotes = new Dictionary<int, List<GridManager.Notes>>();
		List<GridManager.Notes> notesToPlay = null;
		for (int i = 0; i < 64; i++){
			randomNotes.Add(i, new List<GridManager.Notes>());
			markovNotes.Add(i, new List<GridManager.Notes>());
			harmonizedInputNotes.Add(i, new List<GridManager.Notes>());
			harmonizedMarkovNotes.Add(i, new List<GridManager.Notes>());
		}
		int count=0;
		while(playButton){
			
			if(randomNoteProduction){
				randomNotes = generateRandomNotes(randomNotes, 100-(count*5));
			}
			if(markovNoteProduction){
				Dictionary<GridManager.Notes, ChainLink> markovChain = generateMarkovChain(input);
				markovNotes = generateMarkovNotes(input, markovChain, markovNotes);
			}
			if(harmonizeInputNoteProduction){
				harmonizedInputNotes = harmonizeInput(input, harmonizedInputNotes); //harmonise users input
			}
			if(harmonizeMarkovNoteProduction && markovNoteProduction){
				harmonizedMarkovNotes = harmonizeInput(markovNotes, harmonizedMarkovNotes);
			}

			foreach (KeyValuePair<int, List<GridManager.Notes>> pair in masterScore){
				
				if(playBaseScore){
					notesToPlay=pair.Value;
				}else{
					notesToPlay=new List<GridManager.Notes>();
				}
				if(randomNoteProduction){
					notesToPlay = concat(notesToPlay, randomNotes[pair.Key]); // concat random
				}
				if(markovNoteProduction){
					notesToPlay = concat(notesToPlay, markovNotes[pair.Key]);
				}
				if(harmonizeInputNoteProduction){
					notesToPlay = concat(notesToPlay, harmonizedInputNotes[pair.Key]); //concat harmonized
				}
				if(harmonizeMarkovNoteProduction){
					notesToPlay = concat(notesToPlay, harmonizedMarkovNotes[pair.Key]); //concat harmonized
				}
				
				foreach (GridManager.Notes note in notesToPlay){
					playNote(note);
					
				}
				yield return new WaitForSeconds(0.1f);
			}

			count+=1;
		}
		coroutine=null;
		
	}

	//play all of the notes in available notes.
	IEnumerator playScale(){
		foreach (GridManager.Notes note in availableNotes){
			
			playNote(note);//only played when playScaleButton is activated.
			yield return new WaitForSeconds(0.1f);
		}
	}

	//given a dicionary, play all of the notes in that dictionary.
	IEnumerator playDict(Dictionary<int, List<GridManager.Notes>> givenDict){
		foreach (KeyValuePair<int, List<GridManager.Notes>> pair in givenDict){
			foreach (GridManager.Notes note in pair.Value){
				playNote(note);
				
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	#endregion



	#region ChainLink Class
	public class ChainLink{
		private System.Random percentChooser = new System.Random();
		GridManager.Notes mainNote;
		Dictionary<GridManager.Notes, float> paths = new Dictionary<GridManager.Notes, float>();

		public ChainLink(GridManager.Notes note){
			mainNote = note;
		}

		public void addToPath(GridManager.Notes newNote){
			if(!paths.ContainsKey(newNote)){
				paths.Add(newNote, 1f);
			}else{
				paths[newNote]+=1f;
			}
		}

		public GridManager.Notes getNextNote(){
			int p = percentChooser.Next(100);
			float totalSum = sumAllValues();
			int index=0;
			float step = 100/totalSum;
			float encountered = 0;

			foreach (KeyValuePair<GridManager.Notes, float> pair in paths){
				if(p<((step*pair.Value)+encountered)){
					return pair.Key;
				}else{
					encountered+=(step*pair.Value);
				}
			}
			Debug.Log("ERROR - No next note found in markov chain");
			return GridManager.Notes.none;
		}

		private float sumAllValues(){
			float totalSum=0;
			foreach (KeyValuePair<GridManager.Notes, float> pair in paths){
				totalSum+=pair.Value;
			}
			return totalSum;
		}

		public void printContents(){
			
			foreach (KeyValuePair<GridManager.Notes, float> pair in paths){
				print(mainNote + " is followed by " + pair.Key + " " + pair.Value + " times." );
			}
		}


		public GridManager.Notes getMainNote(){
			return mainNote;
		}
		
		
	}	
	#endregion

	
}
