using UnityEngine;
using System.Collections;

namespace CAVS.Recording {

	/// <summary>
	/// The different ways the Recording can be exported
	/// </summary>
	public enum RecordingFileFormat {

		/// <summary>
		/// Used if the file ever wants to be played back again.
		/// </summary>
		XML,

		/// <summary>
		/// Comma Seperated Value meant to look easier on the eyes.
		/// Can NOT be used for playback
		/// </summary>
		CSV,

	}

}