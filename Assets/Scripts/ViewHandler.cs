using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class for holding the functionality of buttons.
/// </summary>
public class ViewHandler : MonoBehaviour {
	public MarkovManager markovManager;

	public void approveMarkov(){
		markovManager.approveMarkovPair();
	}

	public void disapproveMarkov(){
        markovManager.disapproveMarkovPair();
	}
}
