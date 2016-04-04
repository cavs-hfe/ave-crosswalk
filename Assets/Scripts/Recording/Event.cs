
namespace CAVS.Recording {

	public struct Event {

		/// <summary>
		/// The time the event occured
		/// </summary>
		float time;

		/// <summary>
		/// The name of the event, could be used for categorizing contents
		/// </summary>
		string name;

		/// <summary>
		/// The contents of the event, what went down ya know.
		/// </summary>
		string contents;

		public Event (float time, string name, string contents){

			this.time = time;

			if (this.time < 0) {
				this.time = 0;
			}

			this.name = name;
			this.contents = contents;
		}

		public float Time {
			get {
				return time;
			}
			set {
				time = value;
			}
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public string Contents {
			get {
				return contents;
			}
			set {
				contents = value;
			}
		}
	}

}