using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.iOS
{
	public class Spookometer : MonoBehaviour {

		public Transform modelTransform;

		public List<Transform> models;

		const float VIBRATION_DISTANCE = 1.5f; // temp
		const float VISIBLE_DISTANCE = 1.0f; // temp

		void Start () {
			modelTransform.gameObject.SetActive (false);
			models = new List<Transform> ();
		}

		// Update is called once per frame
		void Update () {
			// Place (or not) some ghosts...
			if (modelTransform != null)
			{
				var x = Random.Range (0, Camera.current.pixelWidth);
				var y = Random.Range (0, Camera.current.pixelHeight);
				var randomPosition = new Vector2 (x, y);

				var screenPosition = Camera.main.ScreenToViewportPoint(randomPosition);

				ARPoint point = new ARPoint {
					x = screenPosition.x,
					y = screenPosition.y
				};

				// ARHitTestResultType is an ARKit (iOS) class (like ARPoint)
				// basically holds information about what the camera's seeing in the real world (surfaces, objects, etc.)
				// this code prioritize result types
				ARHitTestResultType[] resultTypes = {
					ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
					// if you want to use infinite planes use this:
					//ARHitTestResultType.ARHitTestResultTypeExistingPlane,
					ARHitTestResultType.ARHitTestResultTypeHorizontalPlane, 
					ARHitTestResultType.ARHitTestResultTypeFeaturePoint
				}; 

				foreach (ARHitTestResultType resultType in resultTypes)
				{
					if (HitTestWithResultType (point, resultType))
						return;
				}
			}

			// See if there are any ghosts nearby - if there are, then vibrate and/or make visible.
			bool didVibrate = false;
			foreach (Transform model in models) {
				Vector3 position = model.position;
				Vector3 distance = CalculateDistance (position);

				if (!didVibrate && distance.magnitude < VIBRATION_DISTANCE && !model.gameObject.activeInHierarchy) {
					Handheld.Vibrate ();
					didVibrate = true;
				}

				if (distance.magnitude < VISIBLE_DISTANCE) {
					model.gameObject.SetActive (true);
					//ScreenDebug.DebugToScreen ("made a model visible");
				}
			}
		}

		bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
		{
			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes); // see if any valid places nearby to put model
			if (hitResults.Count > 0) {
				// decide whether to place a model or not
				int n = Random.Range (0, 1000);
				if (n == 1) {
					// pick a random hit test result to put the model at
					int i = Random.Range (0, hitResults.Count);
					Matrix4x4 hitTransform = (hitResults [i]).worldTransform;

					Transform clone = Instantiate(modelTransform);

					clone.position = UnityARMatrixOps.GetPosition (hitTransform);
					//clone.rotation = UnityARMatrixOps.GetRotation (hitTransform);

					Quaternion rotation = new Quaternion ();
					rotation.eulerAngles = new Vector3 (0, Random.Range (0, 360), 0);
					clone.rotation = rotation;

					clone.gameObject.SetActive (false);

					models.Add (clone);

					//ScreenDebug.DebugToScreen ("# models placed: " + models.Count);

					return true;
				}
			}
			return false;
		}

		Vector3 CalculateDistance(Vector3 position) {
			Matrix4x4 camera = UnityARSessionNativeInterface.GetARSessionNativeInterface ().GetCameraPose ();
			Vector3 cameraPosition = UnityARMatrixOps.GetPosition (camera);
			return position - cameraPosition;
		}
	}
}
