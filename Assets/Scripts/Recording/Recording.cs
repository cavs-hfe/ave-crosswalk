using System.Collections.Generic;
using UnityEngine;
using REvent = CAVS.Recording.Event;

namespace CAVS.Recording {

	/// <summary>
	/// A single recording of what happened in a certain scene with x amount of actors
	/// </summary>
	public class Recording {


		/// <summary>
		/// The name of the recording that will be used for saving
		/// </summary>
		private string name;


		/// <summary>
		/// The frame rate that this recording was created with.
		/// </summary>
		private float frameRateRecordedAt;


		/// <summary>
		/// The actor unique gameobject identifiers.
		/// </summary>
		private int[] actorIds;


		/// <summary>
		/// All our frames of our recording
		/// </summary>
		private List<Frame> frames;


		/// <summary>
		/// Dictionary for getting an actors name by using
		/// it's unique id for lookup
		/// </summary>
		private Dictionary<int, string> actorNames;


		/// <summary>
		/// Dictionary for getting an actors prefered representation by using
		/// it's unique id for lookup
		/// </summary>
		private Dictionary<int, string> actorRepresentations;


		/// <summary>
		/// All events that where logged while we where recording the scene.
		/// </summary>
		private List<REvent> eventsTranspiredDuringRecording;


		/// <summary>
		/// Initializes a new instance of the <see cref="CAVS.Recording.Recording"/> class.
		/// Recordings must have atleast 1 actor, a name, and a framerate greater that 0
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="frameRateRecordedAt">Frame rate recorded at.</param>
		/// <param name="actorIds">Actor unique gameobject identifiers.</param>
		/// <param name="actorNames">Actor names.</param>
		/// <param name="actorRepresentations">Actor representations.</param>
		public Recording(string name, float frameRateRecordedAt, int[] actorIds, string[] actorNames, string[] actorRepresentations){

			if (actorIds == null || actorIds.Length == 0) {
				throw new System.ArgumentException ("You need atleast 1 actor to make a recording!", "actorIds");
			}

			if (name == null || name == "") {
				throw new System.ArgumentException ("You need a valid name for the recording!", "name");
			}

			if (frameRateRecordedAt <= 0) {
				throw new System.ArgumentException ("You need a frame rate greater than 0!", "frameRateRecordedAt");
			}

			this.name = name;
			this.frameRateRecordedAt = frameRateRecordedAt;
			this.actorIds = actorIds;

			// Set up the names!
			this.actorNames = new Dictionary<int, string> ();
			for (int i = 0; i < actorNames.Length; i++) {
				this.actorNames [actorIds[i]] = actorNames[i];
			}

			// Set up their representations! 
			this.actorRepresentations = new Dictionary<int, string> ();
			for (int i = 0; i < actorRepresentations.Length; i++) {
				this.actorRepresentations [actorIds[i]] = actorRepresentations[i];
			}

			this.frames = new List<Frame> ();

			this.eventsTranspiredDuringRecording = new List<REvent> ();
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CAVS.Recording.Recording"/> class.
		/// FOR SEARILZATION PURPOSES
		/// </summary>
		public Recording(){

		}


		/// <summary>
		/// Adds a frame to the recording at hand
		/// </summary>
		/// <param name="frame">Frame.</param>
		public void addFrame(Frame frame){
			frames.Add(frame);
		}


		/// <summary>
		/// Gets the duration of the recording by comparing the timestamps of the first
		/// and last frame
		/// 
		/// THIS WILL NEED TO CHANGE IF WE ALLOW PAUSING IN THE RECORDING
		/// </summary>
		/// <returns>The duration.</returns>
		public float getDuration(){
			return this.frames[this.frames.Count-1].TimeStamp - this.frames [0].TimeStamp;
		}


		/// <summary>
		/// Logs the input along with the time it was given
		/// </summary>
		/// <param name="input">Input.</param>
		public void logEvent(string eventName, string eventContents){
			eventsTranspiredDuringRecording.Add (new REvent(Time.time, eventName, eventContents));
		}


		public string getActorPreferedRepresentation(int actorId){
			return actorRepresentations [actorId];
		}


		public string getActorName(int actorId){
			return actorNames [actorId];
		}


		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public float FrameRateRecordedAt {
			get {
				return frameRateRecordedAt;
			}
			set {
				frameRateRecordedAt = value;
			}
		}

		public int[] ActorIds {
			get {
				return actorIds;
			}
			set {
				actorIds = value;
			}
		}

		public Frame[] Frames {
			get {
				return frames.ToArray();
			}
			set {
				frames = new List<Frame> (value);
			}
		}


		/// <summary>
		/// Gets or sets the actor representations.
		/// For Seralization purposes
		/// </summary>
		/// <value>The actor representations.</value>
		public string[] ActorRepresentations {
			get {

				string[] types = new string[this.actorIds.Length];

				for (int i = 0; i < types.Length; i++) {
					types [i] = actorRepresentations [this.actorIds [i]];
				}

				return types;
			}
			set {

				this.actorRepresentations = new Dictionary<int, string> ();

				for (int i = 0; i < actorIds.Length; i++) {
					actorRepresentations [this.actorIds [i]] = value [i];
				}

			}
		}


		/// <summary>
		/// Gets or sets the actor names.
		/// For serialization purposes
		/// </summary>
		/// <value>The actor names.</value>
		public string[] ActorNames {
			get {
				
				string[] names = new string[this.actorIds.Length];

				for (int i = 0; i < names.Length; i++) {
					names [i] = actorNames [this.actorIds [i]];
				}

				return names;

			}
			set {

				this.actorNames = new Dictionary<int, string> ();

				for (int i = 0;  i < actorIds.Length; i++) {
					this.actorNames  [this.actorIds [i]] = value [i];
				}

			}
		}


		public REvent[] EventsTranspiredDuringRecording {
			get {
				return eventsTranspiredDuringRecording.ToArray();
			}
			set {
				eventsTranspiredDuringRecording = new List<REvent>(value);
			}
		}

	}

}