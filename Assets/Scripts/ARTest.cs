using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.iOS
{
	public class ARTest : MonoBehaviour {

		public Transform modelTransform;

		// Update is called once per frame
		void Update () {
			if (Input.touchCount > 0 && modelTransform != null) // if there's a touch, & a model has been supplied
			{
				var touch = Input.GetTouch(0);

				var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);

				// guessing an ARPoint represents a point in the real world the computer tracks

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
					{
						return;
					}
				}
			}
		}

		bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes) // problem: you need a point to call this function...
		{
			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes); // see if any valid places nearby to put kitten
			if (hitResults.Count > 0) {
				// decide whether to place a model or not
				int n = Random.Range (0, 100);
				if (n == 1) {
					// pick a random hit test result to put the model at
					int i = Random.Range (0, hitResults.Count);
					Matrix4x4 hitTransform = (hitResults [i]).worldTransform;

					modelTransform.position = UnityARMatrixOps.GetPosition (hitTransform);
					// adjust rotation as necessary
					modelTransform.rotation = UnityARMatrixOps.GetRotation (hitTransform);

					calculateDistance (hitTransform); 
					// TODO: wait, problem! this is only called when a ghost is being placed... 
					// also it's moving the ghost around, not cloning... maybe the tapping mechanic won't work? 
					// maybe just, when doing the ARPoint, specify random x & y coordinates???? experiment...
					// also, distance needs to be constantly updated...

					return true;
				}
			}
			return false;
		}

		Vector3 calculateDistance(Matrix4x4 point) {
			Vector3 pointPosition = UnityARMatrixOps.GetPosition (point);

			Matrix4x4 camera = UnityARSessionNativeInterface.GetARSessionNativeInterface ().GetCameraPose ();
			Vector3 cameraPosition = UnityARMatrixOps.GetPosition (camera);

			ScreenDebug.DebugToScreen ("" + (pointPosition - cameraPosition));

			return pointPosition - cameraPosition;
		}
	}
}
