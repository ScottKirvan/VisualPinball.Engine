using System;
using UnityEditor;
using UnityEngine;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.VPT;
using VisualPinball.Engine.VPT.Bumper;
using VisualPinball.Engine.VPT.Flipper;
using VisualPinball.Engine.VPT.HitTarget;
using VisualPinball.Engine.VPT.Kicker;
using VisualPinball.Engine.VPT.Plunger;
using VisualPinball.Engine.VPT.Ramp;
using VisualPinball.Engine.VPT.Rubber;
using VisualPinball.Engine.VPT.Spinner;
using VisualPinball.Engine.VPT.Surface;
using VisualPinball.Engine.VPT.Table;
using VisualPinball.Engine.VPT.Trigger;
using VisualPinball.Unity.Import;
using VisualPinball.Unity.VPT.Table;
using Texture = UnityEngine.Texture;

namespace VisualPinball.Unity.Editor.Toolbox
{
	public class ToolboxEditor : EditorWindow
	{
		private Texture2D _bumperIcon;
		private Texture2D _surfaceIcon;
		private Texture2D _rampIcon;
		private Texture2D _flipperIcon;
		private Texture2D _plungerIcon;
		private Texture2D _spinnerIcon;
		private Texture2D _triggerIcon;
		private Texture2D _kickerIcon;
		private Texture2D _targetIcon;
		private Texture2D _rubberIcon;

		private static TableBehavior TableBehavior => FindObjectOfType<TableBehavior>();

		private static Table Table {
			get {
				var tb = TableBehavior;
				return tb == null ? null : tb.Item;
			}
		}

		[MenuItem("Visual Pinball/Toolbox", false, 100)]
		public static void ShowWindow()
		{
			GetWindow<ToolboxEditor>("Visual Pinball Toolbox");
		}

		private void OnEnable()
		{
			const string iconPath = "Packages/org.visualpinball.engine.unity/VisualPinball.Unity/VisualPinball.Unity.Editor/Resources/Icons";
			_bumperIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_bumper.png");
			_surfaceIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_surface.png");
			_rampIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_ramp.png");
			_flipperIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_flipper.png");
			_plungerIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_plunger.png");
			_spinnerIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_spinner.png");
			_triggerIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_trigger.png");
			_kickerIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_kicker.png");
			_targetIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_target.png");
			_rubberIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}/icon_rubber.png");
		}

		private void OnGUI()
		{
			if (GUILayout.Button("New Table")) {
				var existingTable = FindObjectOfType<TableBehavior>();
				if (existingTable == null) {
					const string tableName = "Table1";
					var rootGameObj = new GameObject();
					var table = new Table(new TableData {Name = tableName});
					var converter = rootGameObj.AddComponent<VpxConverter>();
					converter.Convert(tableName, table);
					DestroyImmediate(converter);
					Selection.activeGameObject = rootGameObj;
					Undo.RegisterCreatedObjectUndo(rootGameObj, "New Table");

				} else {
					EditorUtility.DisplayDialog("Visual Pinball",
						"Sorry, cannot add multiple tables, and there already is " +
						existingTable.name, "Close");
				}
			}

			if (TableBehavior == null) {
				GUI.enabled = false;
			}

			var iconSize = position.width / 2f - 4.5f;
			var buttonStyle = new GUIStyle(GUI.skin.button) {
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove
			};

			GUILayout.BeginHorizontal();

			if (CreateButton("Wall", _surfaceIcon, iconSize, buttonStyle)) {
				CreateItem(Surface.GetDefault, "Wall");
			}

			if (CreateButton("Ramp", _rampIcon, iconSize, buttonStyle)) {
				CreateItem(Ramp.GetDefault, "New Ramp");
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			if (CreateButton("Flipper", _flipperIcon, iconSize, buttonStyle)) {
				CreateItem(Flipper.GetDefault, "New Flipper");
			}

			if (CreateButton("Plunger", _plungerIcon, iconSize, buttonStyle)) {
				CreateItem(Plunger.GetDefault, "New Plunger");
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			if (CreateButton("Bumper", _bumperIcon, iconSize, buttonStyle)) {
				CreateItem(Bumper.GetDefault, "New Bumper");
			}

			if (CreateButton("Spinner", _spinnerIcon, iconSize, buttonStyle)) {
				CreateItem(Spinner.GetDefault, "New Spinner");
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			if (CreateButton("Trigger", _triggerIcon, iconSize, buttonStyle)) {
				CreateItem(Trigger.GetDefault, "New Trigger");
			}

			if (CreateButton("Kicker", _kickerIcon, iconSize, buttonStyle)) {
				CreateItem(Kicker.GetDefault, "New Kicker");
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			if (CreateButton("Target", _targetIcon, iconSize, buttonStyle)) {
				CreateItem(HitTarget.GetDefault, "New Target");
			}

			if (CreateButton("Rubber", _rubberIcon, iconSize, buttonStyle)) {
				CreateItem(Rubber.GetDefault, "New Rubber");
			}

			GUILayout.EndHorizontal();

			GUI.enabled = true;
		}

		private static bool CreateButton(string label, Texture icon, float iconSize, GUIStyle buttonStyle)
		{
			return GUILayout.Button(
				new GUIContent(label, icon),
				buttonStyle,
				GUILayout.Width(iconSize),
				GUILayout.Height(iconSize)
			);
		}

		private static void CreateItem<TItem>(Func<Table, TItem> create, string actionName) where TItem : IItem
		{
			var table = Table;
			var item = create(table);
			table.Add(item, true);
			Selection.activeGameObject = CreateRenderable(table, item as IRenderable);
			Undo.RegisterCreatedObjectUndo(Selection.activeGameObject, actionName);
		}

		private static GameObject CreateRenderable(Table table, IRenderable renderable)
		{
			var tb = TableBehavior;
			var rog = renderable.GetRenderObjects(table, Origin.Original, false);
			return VpxConverter.ConvertRenderObjects(renderable, rog, GetOrCreateParent(tb, rog), tb);
		}

		private static GameObject GetOrCreateParent(Component tb, RenderObjectGroup rog)
		{
			var parent = tb.gameObject.transform.Find(rog.Parent)?.gameObject;
			if (parent == null) {
				parent = new GameObject(rog.Parent);
				parent.transform.parent = tb.gameObject.transform;
				parent.transform.localPosition = Vector3.zero;
				parent.transform.localRotation = Quaternion.identity;
				parent.transform.localScale = Vector3.one;
			}

			return parent;
		}
	}
}