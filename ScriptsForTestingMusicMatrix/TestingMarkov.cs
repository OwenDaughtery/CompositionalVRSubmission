using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    /// <summary>
    /// NOTE: This script is only included in the final submission to act as evidence of the testing of the backend part of the project
    ///  which is described on page 31 section 9.3.1 on the "C.V.R" report for candidate 164743.
    ///  The code will not compile successfully if uncommented due to not being configured to test the front end, and, as stated previously,
    ///  is only included in the submission to act as evidence that rigurous testing protols were implemented.
    /// </summary>
    public class TestingMarkov
    {


        /*
        GameObject mmObject;
        GameObject kmObject;
        GameObject tmObject;

        MarkovManager markovManager;
        KeyManager keyManager;
        TrackManager trackManager;


        [TestFixtureSetUp]
        public void Setup()
        {
            //initialising scripts and objects
            mmObject = new GameObject();
            markovManager = mmObject.AddComponent(typeof(MarkovManager)) as MarkovManager;

            kmObject = new GameObject();
            keyManager = kmObject.AddComponent(typeof(KeyManager)) as KeyManager;

            tmObject = new GameObject();
            //track manager will throw error of not knowing readhead, no need for concern as that is outside this testing suites scope (It's a view related test).
            trackManager = tmObject.AddComponent(typeof(TrackManager)) as TrackManager;

            //setting script variables
            markovManager.trackManager = trackManager;
            trackManager.key = NoteManager.Notes.C2;
        }

        [Test]
        public void TestSetUpCorrectly()
        {
            Assert.NotNull(mmObject);
            Assert.NotNull(kmObject);

            Assert.NotNull(markovManager);
            Assert.NotNull(keyManager);

            Assert.NotNull(markovManager.trackManager);
        }

        //ensuring markov manager can start up correctly and create markov pairs.
        [Test]
        public void TestGetMarkovPair()
        {
            markovManager.startMarkovManaging();

            MarkovManager.MarkovPair markovPair = markovManager.getMarkovPair();
            Assert.NotNull(markovPair);
        }

        //ensuring markov manager can start up correctly and get markov chain
        [Test]
        public void TestGetMarkovChain()
        {
            markovManager.startMarkovManaging();

            MarkovManager.MarkovChain markovChain = markovManager.getMarkovChain();
            Assert.NotNull(markovChain);
        }

        //ensuring markov manager can start up correctly and get rhythm markov chain
        [Test]
        public void TestGetRhythmMarkovChain()
        {
            markovManager.startMarkovManaging();

            MarkovManager.RhythmMarkovChain rhythmMarkovChain = markovManager.getRhythmMarkovChain();
            Assert.NotNull(rhythmMarkovChain);
        }

        //ensure phase is initialised correctly
        [Test]
        public void TestGetPhase()
        {
            Assert.AreEqual(-1, markovManager.getPhase());
        }

        //ensure phase can be incremented correctly.
        [Test]
        public void TestPhaseIncrement()
        {
            Assert.AreEqual(-1, markovManager.getPhase());
            markovManager.advancePhase();
            Assert.AreEqual(0, markovManager.getPhase());
        }

        //ensure markov chains are populated with markov states.
        [Test]
        public void TestGetMarkovState()
        {
            markovManager.startMarkovManaging();

            MarkovManager.MarkovChain markovChain = markovManager.getMarkovChain();
            MarkovManager.MarkovState markovState = markovChain.getState(NoteManager.Notes.C2);
            Assert.NotNull(markovState);
        }

        //ensure rhythm markov chains are populated with rhythm markov states.
        [Test]
        public void TestGetRhythmMarkovState()
        {
            markovManager.startMarkovManaging();

            MarkovManager.RhythmMarkovChain rhythmMarkovChain = markovManager.getRhythmMarkovChain();
            MarkovManager.RhythmMarkovState rhythmMarkovState = rhythmMarkovChain.getState(1);
            Assert.NotNull(rhythmMarkovState);
        }

        //ensure markov state is initialised to correct transition weight
        [Test]
        public void TestMarkovStateTransition()
        {
            markovManager.startMarkovManaging();

            MarkovManager.MarkovChain markovChain = markovManager.getMarkovChain();
            MarkovManager.MarkovState markovState = markovChain.getState(NoteManager.Notes.C2);
            Assert.AreEqual(markovState.getTransition(NoteManager.Notes.C2), 0.135714293f);
        }

        //ensure rhythm markov state is initialised to correct transition weight
        [Test]
        public void TestRhythmMarkovStateTransition()
        {
            markovManager.startMarkovManaging();

            MarkovManager.RhythmMarkovChain rhythmMarkovChain = markovManager.getRhythmMarkovChain();
            MarkovManager.RhythmMarkovState rhythmMarkovState = rhythmMarkovChain.getState(1);
            Assert.AreEqual(rhythmMarkovState.getTransition(1), 0.09375f);
        }

    }*/
}
