// Visual Pinball Engine
// Copyright (C) 2021 freezy and VPE Team
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace VisualPinball.Unity
{
	[ExecuteAlways]
	public class CameraController : MonoBehaviour
	{
		public CameraSetting activeSetting;

		public List<CameraSetting> cameraPresets;

		[NonSerialized]
		public Camera Camera;

		private void OnEnable()
		{
			Camera = GetComponentInChildren<Camera>();

			// validate structure
			var trans = Camera.transform;
			var altitude = trans.transform.parent;
			if (altitude == null) {
				Camera = null;
				return;
			}
			var azimuth = altitude.parent;
			if (azimuth == null) {
				Camera = null;
				return;
			}
			var offset = azimuth.parent;
			if (offset == null) {
				Camera = null;
			}
		}

		public void ApplySetting()
		{
			if (!activeSetting) {
				return;
			}

			if (!TableSelector.Instance.HasSelectedTable) {
				return;
			}

			var table = TableSelector.Instance.SelectedTable;
			var trans = Camera.transform;
			var altitude = trans.transform.parent;
			var azimuth = altitude.parent;
			var offset = azimuth.parent;

			trans.localPosition = new Vector3(0, 0, -activeSetting.distance);
			altitude.localRotation = Quaternion.Euler(new Vector3(activeSetting.angle, 0, 0));
			azimuth.localRotation = Quaternion.Euler(new Vector3(0, activeSetting.orbit, 0));
			offset.localPosition = table.GetTableCenter() + activeSetting.offset;
			Camera.fieldOfView = activeSetting.fov;

			var tb = table.GetTableBounds();
			var p = trans.position;
			var nearPoint = tb.ClosestPoint(p);
			var deltaN = Vector3.Magnitude(p - nearPoint);
			var deltaF = math.max(Vector3.Distance(p, tb.max), Vector3.Distance(p, tb.min));

			//TODO: Replace this with proper frustum distances.
			var nearPlane = math.max(0.001f, math.abs(deltaN * 0.9f));
			var farPlane = math.max(1f, math.abs(deltaF*1.1f));

			Camera.nearClipPlane = nearPlane;
			Camera.farClipPlane = farPlane;
		}
	}
}
