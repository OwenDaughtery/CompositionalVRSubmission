//initialising server to correct variables (ensure 0 out device is HTC Vive by attempting s.boot and checking output)
Server.killAll;
Server.local.options.device = 0;

//Start server
s.boot;
s.plotTree;

//test variable to ensure audio playback is working
{SinOsc.ar}.play;

OSCdef.new(
	\test,
	{arg msg;
		[msg].postln;
	},
	'/test',
	nil,
	57120
)


//OSDefs to intercept messages from unity
OSCdef.new(
	\playVoiceA,
	{arg msg;

		var volume, noteFreq, length;
		[msg].postln;
		volume = [msg[1]];
		noteFreq = [msg[2]];
		length = [msg[3]];
		Synth.new(\VoiceA, [\noiseHz, 16, \freq, (noteFreq.asFloat), \amp, (volume.asFloat)*0.5, \length, length.asFloat]);
	},
	'/playVoiceA' ,
	nil,
	57120
);

OSCdef.new(
	\playVoiceB,
	{arg msg;
		var volume, noteFreq, length;
		volume = [msg[1]];
		noteFreq = [msg[2]];
		length = [msg[3]];
		Synth.new(\VoiceB, [\noiseHz, 16, \freq, (noteFreq.asFloat), \amp, (volume.asFloat)*0.5, \length, length.asFloat]);
	},
	'/playVoiceB' ,
	nil,
	57120
);

OSCdef.new(
	\playVoiceC,
	{arg msg;
		var volume, noteFreq, length;
		volume = [msg[1]];
		noteFreq = [msg[2]];
		length = [msg[3]];
		Synth.new(\VoiceC, [\noiseHz, 16, \freq, (noteFreq.asFloat), \amp, (volume.asFloat)*0.5, \length, length.asFloat]);
	},
	'/playVoiceC' ,
	nil,
	57120
);

//SynthDefs to be played by the OSDef when it receives a message from unity.
(SynthDef.new(\VoiceA, {
	arg noiseHz = 8, freq, amp, length;
	var sig, env;
	env = XLine.kr(amp,0.01,length, doneAction:2);
	sig = SinOsc.ar(freq) * env;
	Clip.ar(sig, 0.05, 0.06);
	Out.ar(0, sig);

}
).add;
);

(SynthDef.new(\VoiceB, {
	arg noiseHz = 8, freq, amp, length;
	var sig, env;
	env = XLine.kr(amp,0.01,length, doneAction:2);
	sig = Pulse.ar(freq) * env;
	Clip.ar(sig, 0.05, 0.06);
	Out.ar(0, sig);

}
).add;
);

(SynthDef.new(\VoiceC, {
	arg noiseHz = 8, freq, amp, length;
	var sig, env;
	env = XLine.kr(amp,0.01,length, doneAction:2);
	sig = Formant.ar(freq) * env;
	Clip.ar(sig, 0.05, 0.06);
	Out.ar(0, sig);

}
).add;
);