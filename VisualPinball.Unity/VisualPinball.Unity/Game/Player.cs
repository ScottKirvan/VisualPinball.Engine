﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.Math;
using VisualPinball.Engine.VPT.Flipper;
using VisualPinball.Engine.VPT.Kicker;
using VisualPinball.Engine.VPT.Surface;
using VisualPinball.Engine.VPT.Table;
using VisualPinball.Unity.VPT.Ball;
using VisualPinball.Unity.VPT.Flipper;
using VisualPinball.Unity.VPT.Kicker;
using VisualPinball.Unity.VPT.Table;

namespace VisualPinball.Unity.Game
{
	public class Player : MonoBehaviour
	{
		private readonly TableApi _tableApi = new TableApi();

		private readonly Dictionary<int, FlipperApi> _flippers = new Dictionary<int, FlipperApi>();
		private FlipperApi Flipper(int entityIndex) => _flippers.Values.FirstOrDefault(f => f.Entity.Index == entityIndex);

		#if FLIPPER_LOG
		public static StreamWriter DebugLog;
		#endif

		private Table _table;
		private BallManager _ballManager;

		public Matrix4x4 TableToWorld => transform.localToWorldMatrix;

		public void RegisterFlipper(Flipper flipper, Entity entity, GameObject go)
		{
			//AttachToRoot(entity, go);
			var flipperApi = new FlipperApi(flipper, entity, this);
			_tableApi.Flippers[flipper.Name] = flipperApi;
			_flippers[entity.Index] = flipperApi;
			// World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FlipperSystem>().OnRotated +=
			// 	(sender, e) => flipperApi.HandleEvent(e);
		}

		public void RegisterKicker(Kicker kicker, Entity entity, GameObject go)
		{
			var kickerApi = new KickerApi(kicker, entity, this);
			_tableApi.Kickers[kicker.Name] = kickerApi;
		}

		public void RegisterSurface(Surface item, Entity entity, GameObject go)
		{
		}

		public BallApi CreateBall(IBallCreationPosition ballCreator, float radius = 25, float mass = 1)
		{
			// todo callback and other stuff
			return _ballManager.CreateBall(this, ballCreator, radius, mass);
		}

		public float3 GetGravity()
		{
			var slope = _table.Data.AngleTiltMin + (_table.Data.AngleTiltMax - _table.Data.AngleTiltMin) * _table.Data.GlobalDifficulty;
			var strength = _table.Data.OverridePhysics != 0 ? PhysicsConstants.DefaultTableGravity : _table.Data.Gravity;
			return new float3(0,  math.sin(math.radians(slope)) * strength, -math.cos(math.radians(slope)) * strength);
		}

		private void Awake()
		{
			var tableComponent = gameObject.GetComponent<TableBehavior>();
			_table = tableComponent.CreateTable();
			_ballManager = new BallManager(_table);
			#if FLIPPER_LOG
			DebugLog = File.CreateText("flipper.log");
			#endif
		}

		private void Start()
		{
			// bootstrap table script(s)
			var tableScripts = GetComponents<VisualPinballScript>();
			foreach (var tableScript in tableScripts) {
				tableScript.OnAwake(_tableApi);
			}

			// trigger init events now
			foreach (var i in _tableApi.Initializables) {
				i.Init();
			}
		}

		private void Update()
		{
			// flippers will be handled via script later, but until scripting works, do it here.
			if (Input.GetKeyDown("left shift")) {
				_tableApi.Flipper("LeftFlipper")?.RotateToEnd();
			}
			if (Input.GetKeyUp("left shift")) {
				_tableApi.Flipper("LeftFlipper")?.RotateToStart();
			}
			if (Input.GetKeyDown("right shift")) {
				_tableApi.Flipper("RightFlipper")?.RotateToEnd();
			}
			if (Input.GetKeyUp("right shift")) {
				_tableApi.Flipper("RightFlipper")?.RotateToStart();
			}
		}

		#if FLIPPER_LOG
		private void OnDestroy()
		{
			DebugLog.Dispose();
		}
		#endif

	}
}
