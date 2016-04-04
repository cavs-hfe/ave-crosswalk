using UnityEngine;
using System.Collections.Generic;

namespace CAVS.Recording {


	/// <summary>
	/// A frame represents a single capture of the scene of all the actors positions and rotations
	/// </summary>
	public struct Frame {


		int[] actorIds;


		Dictionary<int, Vector3> positions;


		Dictionary<int, Vector3> rotations;


		float timeStamp;


		public Frame(int[] actorIds, Vector3[] positions, Vector3[] rotations, float timeStamp){

			this.actorIds = actorIds;

			this.positions = new Dictionary<int, Vector3> ();
			this.rotations = new Dictionary<int, Vector3> ();

			for (int i = 0; i < actorIds.Length; i++) {
				this.positions[actorIds[i]] = positions[i];
			}

			for (int i = 0; i < actorIds.Length; i++) {
				this.rotations[actorIds[i]] = rotations[i];
			}

			this.timeStamp = timeStamp;

		}


		public Vector3 getPositionOfActor(int actor){
			return positions [actor];
		}


		public Vector3 getRotationOfActor(int actor){
			return rotations [actor];
		}


		public int[] ActorIds {
			get {
				return actorIds;
			}
			set {
				actorIds = value;
			}
		}


		public Vector3[] Positions {
			get {
				
				Vector3[] pos = new Vector3[this.actorIds.Length];

				for (int i = 0; i < actorIds.Length; i++) {
					pos [i] = this.positions [actorIds [i]];
				}

				return pos;

			}
			set {
				
				this.positions = new Dictionary<int, Vector3> ();

				for (int i = 0; i < this.actorIds.Length; i++) {
					this.positions [actorIds [i]] = value [i];
				}

			}
		}


		/// <summary>
		/// Gets or sets all the rotations of the actors
		/// For serialization purposes only
		/// </summary>
		/// <value>The rotations.</value>
		public Vector3[] Rotations {
			get {
				
				Vector3[] rots = new Vector3[this.actorIds.Length];

				for (int i = 0; i < actorIds.Length; i++) {
					rots [i] = this.rotations [actorIds [i]];
				}

				return rots;

			}
			set {

				this.rotations = new Dictionary<int, Vector3> ();

				for (int i = 0; i < this.actorIds.Length; i++) {
					this.rotations [actorIds [i]] = value [i];
				}

			}
		}


		public float TimeStamp {
			get {
				return timeStamp;
			}
			set {
				timeStamp = value;
			}
		}

	}

}