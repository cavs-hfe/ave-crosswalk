using UnityEngine;
using System.Collections;

namespace CAVS.Recording {

	/// <summary>
	/// Any monobehavior class that needs be disabled while a playback is happening of a 
	/// recording should extend from this class to the playback service knows what to
	/// disable and send events to.
	/// </summary>
	public abstract class PlaybackActorBehavior: MonoBehaviour {

		[SerializeField]
		private bool disableOnPlayback = false;


		/// <summary>
		/// Whether or not the component should be disabled on playback
		/// </summary>
		/// <returns><c>true</c>, if we should disable the component on playback, <c>false</c> otherwise.</returns>
		public bool shouldDisableOnPlayback(){
			return disableOnPlayback;
		}


		/// <summary>
		/// Set's whether or not
		/// </summary>
		/// <param name="disable">If set to <c>true</c>, disable.</param>
		public void setDisableOnPlayback(bool disable){
			this.disableOnPlayback = disable;
		}


		/// <summary>
		/// When ever a recorded event occurs during a playback, a message is sent out to
		/// all MonoBehavior components that extend this class.
		/// </summary>
		/// <param name="name">Name of event</param>
		/// <param name="contents">Contents of the event upon recording</param>
		public virtual void handleEvent(string name, string contents){

		}

	}

}