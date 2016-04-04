using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace CAVS.Recording {


	/// <summary>
	/// To actually record what's in the scene you will need the Recording Service Behavior attached to a gameobject in the scene. 
	/// While recording is in progress, if you don't manually stop it by calling a method, it will automaitaclly stop and save whenever you unplay the scene.
	///  All recordings are saved in a folder named "Recordings" in xml format. 
	/// </summary>
	public class RecordingServiceBehavior : MonoBehaviour {


		/// <summary>
		/// Whether or not we should start recording the scene when the scene is initialized.
		/// </summary>
		[SerializeField]
		private bool recordOnPlay = true;


		/// <summary>
		/// The name of the file that we'll save all recorded movement frames to.
		/// </summary>
		[SerializeField]
		private string fileName;


		/// <summary>
		/// The rate at which we'd like to record our scene at.
		/// </summary>
		[SerializeField]
		private float framesPerSecondForRecording = 30f;


		/// <summary>
		/// The current state of the recording service, as to whether or not it's
		/// recording actors.
		/// </summary>
		private RecordingState currentState = RecordingState.Stopped;


		/// <summary>
		/// The actors in scene that we will be recording
		/// </summary>
		private List<ActorBehavior> actorsInScene;


		/// <summary>
		/// The recording object that is built while we're recording a scene.
		/// </summary>
		private Recording currentRecordingBeingBuilt;


		/// <summary>
		/// Unity's time of the last time we took a snap shot of all the actors
		/// in the scene for recording.
		/// </summary>
		private float timeOfLastFrameCapture = 0f;


		/// <summary>
		/// This position is used to represent in our recording that we don't know
		/// where a certain object is.
		/// Ex.  If we had an actor at the beggining of our recording but now it seems
		/// it's been deleted, then we will assign our frames it's position of 
		/// this variable's value
		/// </summary>
		private Vector3 nullObjectPosition = new Vector3(666,666,666);


		/// <summary>
		/// GameObject instance unique IDs for identifying an actor
		/// </summary>
		private int[] actorIdsBeingRecorded;


		/// <summary>
		/// The time in seconds since the scene started when the pause command was
		/// called while we where recording.
		/// </summary>
		private float timePaused;


		/// <summary>
		/// Begins recording the scene if you pass in a name
		/// </summary>
		public void startRecording(string nameOfRecording){

			// Cleans data
			if (nameOfRecording != null && nameOfRecording == "") {
				Debug.Log ("You can't create a recording with no name!");
				return;
			}

			// Can't start a new recording while currentely recording
			if (currentState == RecordingState.Recording || currentState == RecordingState.Paused) {
				Debug.LogError ("Can't record");
			}

			// Change state to recording
			currentState = RecordingState.Recording;

			// Grab all actors in the scene
			actorsInScene = new List<ActorBehavior>(GameObject.FindObjectsOfType<ActorBehavior> ());

			// Grabs all actor unique instance ids
			actorIdsBeingRecorded = new int[actorsInScene.Count];
			for (int i = 0; i < actorIdsBeingRecorded.Length; i++) {
				actorIdsBeingRecorded[i] = actorsInScene [i].gameObject.GetInstanceID ();
			}

			// Grab actor names
			string[] actorNames = new string[actorsInScene.Count];
			for (int i = 0; i < actorsInScene.Count; i++) {
				actorNames[i] = actorsInScene [i].getNameForRecording ();
			}

			// Grab actors prefered playback representation
			string[] actorplaybackRep = new string[actorsInScene.Count];
			for (int i = 0; i < actorsInScene.Count; i++) {
				actorplaybackRep[i] = actorsInScene [i].getObjToRepresentActor ();
			}

			// Create our recording object that we'll add frame data to.
			currentRecordingBeingBuilt = new Recording (nameOfRecording, getFPS(), actorIdsBeingRecorded, actorNames, actorplaybackRep);

			// Capture our first frame
			captureFrame ();

		}


		/// <summary>
		/// Pauses the current recording.
		/// </summary>
		public void pauseRecording(){

			if(currentState == RecordingState.Stopped || currentState == RecordingState.Paused){
				Debug.LogWarning ("Not recording anything! Nothing to pause!");
				return;
			}

			timePaused = Time.time;
			currentState = RecordingState.Paused;

		}


		/// <summary>
		/// If paused, we resume recording the scene
		/// </summary>
		public void resumeRecording(){


			if(currentState == RecordingState.Recording){
				Debug.LogWarning ("Currentely recording! Nothing to resume!");
				return;
			}


			if(currentState == RecordingState.Stopped){
				Debug.LogWarning ("Not recording anything! Nothing to resume!");
				return;
			}
				
			// The time we spent paused
			float timeSpentPaused = Time.time - timePaused;

			// Just shift everything over and pretend like it was recorded that way
			for (int i = 0; i < currentRecordingBeingBuilt.Frames.Length; i++) {
				currentRecordingBeingBuilt.Frames[i].TimeStamp = currentRecordingBeingBuilt.Frames[i].TimeStamp + timeSpentPaused; 
			}

			// Shift over event times
			for (int i = 0; i < currentRecordingBeingBuilt.EventsTranspiredDuringRecording.Length; i++) {
				currentRecordingBeingBuilt.EventsTranspiredDuringRecording[i].Time = currentRecordingBeingBuilt.EventsTranspiredDuringRecording[i].Time + timeSpentPaused;
			}

			timeOfLastFrameCapture += timeSpentPaused;

		}


		/// <summary>
		/// Stops the recording and saves as an XML file with the file name originally specified 
		/// </summary>
		/// <returns>The recording that was saved.</returns>
		public Recording stopAndSaveRecording(){
			return stopAndSaveRecording (new RecordingFileFormat[]{RecordingFileFormat.XML});
		}


		/// <summary>
		/// Stops the recording and saves as specified with the file name originally specified 
		/// </summary>
		/// <returns>The recording that was saved.</returns>
		/// <param name="howToSave">How to save.</param>
		public Recording stopAndSaveRecording(RecordingFileFormat howToSave){
			return stopAndSaveRecording (new RecordingFileFormat[]{howToSave});
		}


		/// <summary>
		/// Stops the recording and saves it as multiple different files with the file name originally specified 
		/// </summary>
		/// <returns>The recording that was saved.</returns>
		/// <param name="howToSave">How to save.</param>
		public Recording stopAndSaveRecording(RecordingFileFormat[] howToSave){

			if (currentState != RecordingState.Paused && currentState != RecordingState.Recording) {
				Debug.LogError ("Can't stop a recording because we're not currentely making one!");
				return null;
			}

			if (currentRecordingBeingBuilt == null) {
				Debug.LogError ("Something very bad has happened.. (x_x)  Figure out how to create this error and tell Eli");
				return null;
			}

			currentState = RecordingState.Stopped;

			for (int i = 0; i < howToSave.Length; i++) {

				switch(howToSave[i]){

				case RecordingFileFormat.XML:
					saveRecordingAsXML (currentRecordingBeingBuilt);
					break;

				case RecordingFileFormat.CSV:
					saveRecordingAsCSV (currentRecordingBeingBuilt);
					break;

				default:
					saveRecordingAsXML (currentRecordingBeingBuilt);
					break;

				}

			}

			// temporary storage for returning
			Recording recordingMade = currentRecordingBeingBuilt;

			// Clear the service's it was building
			currentRecordingBeingBuilt = null;

			return recordingMade;

		}


		/// <summary>
		/// Stops the and trashes the current recording being made.
		/// </summary>
		/// <returns>The recording that was being made before trashing it.</returns>
		public Recording stopAndTrashRecording(){
			
			if (currentState != RecordingState.Paused && currentState != RecordingState.Recording) {
				Debug.LogError ("Can't stop a recording because we're not currentely making one!");
				return null;
			}

			currentState = RecordingState.Stopped;

			// temporary storage for returning
			Recording recordingMade = currentRecordingBeingBuilt;

			// Clear the service's it was building
			currentRecordingBeingBuilt = null;

			return recordingMade;
		}


		/// <summary>
		/// The FPS that the recording service takes snapshots of the state of
		/// the actors in the scene while recording.
		/// </summary>
		/// <returns>The FPS.</returns>
		public float getFPS(){
			return Mathf.Clamp (this.framesPerSecondForRecording, 1, 1000); 
		}


		/// <summary>
		/// Sets the FPS to record by if the service is not already recording
		/// </summary>
		/// <param name="fps">Fps.</param>
		public void setFPSToRecordBy(int fps){
			if (currentelyRecording ()) {
				Debug.LogWarning ("Can't change the fps to record by while recording! Ignoring!");
				return;
			}
			this.framesPerSecondForRecording = Mathf.Clamp (fps, 1, 1000); 
		}


        public void setFileName(string fileName)
        {
            this.fileName = fileName;
        }

		/// <summary>
		/// Whether or not we're currentely recording the scene.
		/// </summary>
		/// <returns><c>true</c>, if recording is currentely occuring, <c>false</c> otherwise.</returns>
		public bool currentelyRecording(){
			
			if (currentState == RecordingState.Recording) {
				return true;
			}

			return false;

		}


		/// <summary>
		/// Logs an event to recording
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="contents">Contents.</param>
		public void logEventToRecording(string name, string contents){

			if (!currentelyRecording ()) {
				Debug.LogError ("You can log an event when theres no recording going on!");
				return;
			}

			currentRecordingBeingBuilt.logEvent (name, contents);

		}


		// Use this for initialization
		void Start () {

			if (!recordOnPlay) {
				return;
			}

			startRecording (this.fileName);
		
		}


		// Update is called once per frame
		void Update () {
		

			// Don't bother doing anything if we're not recording
			if (currentState != RecordingState.Recording) {
				return;
			}


			// If it's time to capture another frame.
			if(Time.time - timeOfLastFrameCapture >= 1f / getFPS()){
				captureFrame ();
			}

		}


		/// <summary>
		/// *Called by the Unity Engine*
		/// 
		/// Raises the disable event.
		/// Called when the object is disabled, such as when you exit play mode.
		/// Used for saving all frame data if we where recording at the time
		/// </summary>
		void OnDisable(){

			if (currentState == RecordingState.Recording || currentState == RecordingState.Paused) {
				stopAndSaveRecording ();
			}

		}


		/// <summary>
		/// Creates a frame containing nessesary information for playback if we're in a recording state
		/// </summary>
		private void captureFrame(){

			// If we're trying to capture a frame while we're not recording throw an error
			if (currentState != RecordingState.Recording) {
				Debug.LogError ("Your trying to capture a frame while your not recording!");
				return;
			}

			// Update the last time we've captured a frame
			timeOfLastFrameCapture = Time.time;

			// Declare all data we'll need for creating a frame
			Vector3[] postions = new Vector3[actorsInScene.Count];
			Vector3[] rotations = new Vector3[actorsInScene.Count];
			int[] ids = new int[actorsInScene.Count];

			// Go through all actors that where put in our roster at the start of the recording
			for (int i = 0; i < this.actorsInScene.Count; i++) {

				// Make sure they still exist in the scene before recording them
				if (actorsInScene [i] != null && actorsInScene [i].gameObject != null) {

					postions [i] = actorsInScene [i].gameObject.transform.position;
					rotations [i] = actorsInScene [i].gameObject.transform.rotation.eulerAngles;
					ids [i] = actorsInScene [i].gameObject.GetInstanceID ();

				} else {
				
					// setting null object position to indicate we don't know where the object is
					postions [i] = nullObjectPosition;
					ids [i] = actorIdsBeingRecorded [i];
				
				}

			}

			// Create our frame
			currentRecordingBeingBuilt.addFrame(new Frame(ids, postions, rotations, Time.time));

		}


		private void saveRecordingAsXML(Recording recordingToSave){

			if (recordingToSave == null) {
				Debug.Log ("Trying to save a null recording!");
				return;
			}

			if(!Directory.Exists("Recordings")){
				Directory.CreateDirectory ("Recordings");
			}

			string path = "Recordings/" + recordingToSave.Name+".xml";

			using(FileStream outputFile = File.Create(path)){

				XmlSerializer formatter = new XmlSerializer (typeof(Recording));
				formatter.Serialize (outputFile, recordingToSave);

			}

		}



		private void saveRecordingAsCSV(Recording recordingToSave){
			
			if (recordingToSave == null) {
				Debug.Log ("Trying to save a null recording!");
				return;
			}

			if(!Directory.Exists("Recordings")){
				Directory.CreateDirectory ("Recordings");
			}

			string path = "Recordings/" + recordingToSave.Name+".csv";

			StringBuilder csv = new StringBuilder ();


			// Header
			csv.AppendLine(string.Format("{0},{1},{2}", "Actor Id", "Name", "Resources Path"));

			// Write the file with all the actors apart of the recording
			for (int i = 0; i < recordingToSave.ActorIds.Length; i++) {

				int id = recordingToSave.ActorIds [i];

				string newLine = string.Format ("{0},{1},{2}",  id.ToString(), 
																recordingToSave.getActorName(id), 
																recordingToSave.getActorPreferedRepresentation(id));
				csv.AppendLine(newLine);
			
			}


			// Line break between tables
			csv.AppendLine ("");


			// row format to represent a position in time
			string frameDataformat = "{0},{1}";


			// Add 7 columns for each action to build table format
			for (int i = 0; i < recordingToSave.ActorIds.Length*7; i++) {
				frameDataformat += "{"+(i+2)+"}";
				if (i != (recordingToSave.ActorIds.Length * 7) - 1) {
					frameDataformat += ",";
				}
			}


			// Write out column names
			string[] header = new string[(recordingToSave.ActorIds.Length*7)+2];

			header [0] = "Time";
			header [1] = "";

			for (int a = 0; a < recordingToSave.ActorIds.Length; a++) {
				
				int id = recordingToSave.ActorIds [a];

				header [(a*7)+2] = id+" x pos";
				header [(a*7)+3] = id+" y pos";
				header [(a*7)+4] = id+" z pos";

				header [(a*7)+5] = id+" x rot";
				header [(a*7)+6] = id+" y rot";
				header [(a*7)+7] = id+" z rot";

				header [(a*7)+8] = "";
			
			}

			// Finally Writting out the column names
			csv.AppendLine (string.Format(frameDataformat, header));


			// Go through every frame and list out data according to the format
			for(int i = 0; i < recordingToSave.Frames.Length; i ++){

				string[] frame = new string[(recordingToSave.ActorIds.Length*7)+2];

				frame [0] = recordingToSave.Frames[i].TimeStamp.ToString();
				frame [1] = "";

				for (int a = 0; a < recordingToSave.ActorIds.Length; a++) {

					int id = recordingToSave.ActorIds [a];
					Vector3 pos = recordingToSave.Frames [i].getPositionOfActor(id);
					Vector3 rot = recordingToSave.Frames [i].getRotationOfActor(id);

					frame [(a*7)+2] = pos.x.ToString();
					frame [(a*7)+3] = pos.y.ToString();
					frame [(a*7)+4] = pos.z.ToString();

					frame [(a*7)+5] = rot.x.ToString();
					frame [(a*7)+6] = rot.y.ToString();
					frame [(a*7)+7] = rot.z.ToString();

					frame [(a*7)+8] = "";

				}

				csv.AppendLine (string.Format(frameDataformat, frame));

			}



			// Finally export the file
			File.WriteAllText (path, csv.ToString());

		}


	}

}