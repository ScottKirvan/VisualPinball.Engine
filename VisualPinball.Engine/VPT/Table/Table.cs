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
using System.IO;
using System.Linq;
using NLog;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.Math;

namespace VisualPinball.Engine.VPT.Table
{
	/// <summary>
	/// The root object for everything table related. <p/>
	///
	/// A table contains all the playfield elements, as well as a set of
	/// global data.
	/// </summary>
	public class Table : Item<TableData>, IRenderable
	{
		public override string ItemName { get; } = "Table";
		public override string ItemGroupName { get; } = "Playfield";
		public override ItemType ItemType { get; } = ItemType.Table;

		public Vertex3D Position { get => new Vertex3D(0, 0, 0); set { } }
		public float RotationY { get => 0; set { } }

		public CustomInfoTags CustomInfoTags { get; set; }
		public int FileVersion { get; set; }
		public byte[] FileHash { get; set; }

		public float Width => Data.Right - Data.Left;
		public float Height => Data.Bottom - Data.Top;

		public float TableHeight => Data.TableHeight;

		public float GlassHeight => Data.GlassHeight;
		public Rect3D BoundingBox => new Rect3D(Data.Left, Data.Right, Data.Top, Data.Bottom, TableHeight, GlassHeight);

		public bool HasMeshAsPlayfield => _meshGenerator.HasMeshAsPlayfield;

		public readonly Dictionary<string, string> TableInfo = new Dictionary<string, string>();
		public ITableResourceContainer<Texture> Textures = new DefaultTableResourceContainer<Texture>();
		public ITableResourceContainer<Sound.Sound> Sounds = new DefaultTableResourceContainer<Sound.Sound>();
		public readonly Dictionary<string, Collection.Collection> Collections = new Dictionary<string, Collection.Collection>();
		public Mappings.Mappings Mappings = new Mappings.Mappings();

		#region GameItems

		private readonly Dictionary<string, Bumper.Bumper> _bumpers = new Dictionary<string, Bumper.Bumper>();
		private readonly List<Decal.Decal> _decals = new List<Decal.Decal>();
		private readonly Dictionary<string, DispReel.DispReel> _dispReels = new Dictionary<string, DispReel.DispReel>();
		private readonly Dictionary<string, Flipper.Flipper> _flippers = new Dictionary<string, Flipper.Flipper>();
		private readonly Dictionary<string, Gate.Gate> _gates = new Dictionary<string, Gate.Gate>();
		private readonly Dictionary<string, HitTarget.HitTarget> _hitTargets = new Dictionary<string, HitTarget.HitTarget>();
		private readonly Dictionary<string, Kicker.Kicker> _kickers = new Dictionary<string, Kicker.Kicker>();
		private readonly Dictionary<string, Light.Light> _lights = new Dictionary<string, Light.Light>();
		private readonly Dictionary<string, LightSeq.LightSeq> _lightSeqs = new Dictionary<string, LightSeq.LightSeq>();
		private readonly Dictionary<string, Plunger.Plunger> _plungers = new Dictionary<string, Plunger.Plunger>();
		private readonly Dictionary<string, Flasher.Flasher> _flashers = new Dictionary<string, Flasher.Flasher>();
		private readonly Dictionary<string, Primitive.Primitive> _primitives = new Dictionary<string, Primitive.Primitive>();
		private readonly Dictionary<string, Ramp.Ramp> _ramps = new Dictionary<string, Ramp.Ramp>();
		private readonly Dictionary<string, Rubber.Rubber> _rubbers = new Dictionary<string, Rubber.Rubber>();
		private readonly Dictionary<string, Spinner.Spinner> _spinners = new Dictionary<string, Spinner.Spinner>();
		private readonly Dictionary<string, Surface.Surface> _surfaces = new Dictionary<string, Surface.Surface>();
		private readonly Dictionary<string, TextBox.TextBox> _textBoxes = new Dictionary<string, TextBox.TextBox>();
		private readonly Dictionary<string, Timer.Timer> _timers = new Dictionary<string, Timer.Timer>();
		private readonly Dictionary<string, Trigger.Trigger> _triggers = new Dictionary<string, Trigger.Trigger>();
		private readonly Dictionary<string, Trough.Trough> _troughs = new Dictionary<string, Trough.Trough>();

		public Bumper.Bumper Bumper(string name) => _bumpers[name];
		public Decal.Decal Decal(int i) => _decals[i];
		public DispReel.DispReel DispReel(string name) => _dispReels[name];
		public Flipper.Flipper Flipper(string name) => _flippers[name];
		public Gate.Gate Gate(string name) => _gates[name];
		public HitTarget.HitTarget HitTarget(string name) => _hitTargets[name];
		public Kicker.Kicker Kicker(string name) => _kickers[name];
		public Light.Light Light(string name) => _lights[name];
		public LightSeq.LightSeq LightSeq(string name) => _lightSeqs[name];
		public Plunger.Plunger Plunger(string name = null) => name == null ? _plungers.Values.FirstOrDefault() : _plungers[name];
		public Flasher.Flasher Flasher(string name) => _flashers[name];
		public Primitive.Primitive Primitive(string name) => _primitives[name];
		public Ramp.Ramp Ramp(string name) => _ramps[name];
		public Rubber.Rubber Rubber(string name) => _rubbers[name];
		public Spinner.Spinner Spinner(string name) => _spinners[name];
		public Surface.Surface Surface(string name) => _surfaces[name];
		public TextBox.TextBox TextBox(string name) => _textBoxes[name];
		public Timer.Timer Timer(string name) => _timers[name];
		public Trigger.Trigger Trigger(string name) => _triggers[name];
		public Trough.Trough Trough(string name) => _troughs[name];

		public IEnumerable<IRenderable> Renderables => new IRenderable[] { this }
			.Concat(_bumpers.Values)
			.Concat(_flippers.Values)
			.Concat(_gates.Values)
			.Concat(_hitTargets.Values)
			.Concat(_kickers.Values)
			.Concat(_lights.Values)
			.Concat(_plungers.Values)
			.Concat(_primitives.Values)
			.Concat(_ramps.Values)
			.Concat(_rubbers.Values)
			.Concat(_spinners.Values)
			.Concat(_surfaces.Values)
			.Concat(_triggers.Values);

		/// <summary>
		/// Game items that need to be converted but aren't rendered.
		/// </summary>
		public IEnumerable<IItem> NonRenderables => new IItem[0]
			.Concat(_troughs.Values);

		public IEnumerable<IItem> GameItems => new IItem[] { }
			.Concat(_bumpers.Values)
			.Concat(_decals.Select(i => i))
			.Concat(_dispReels.Values)
			.Concat(_flippers.Values)
			.Concat(_flashers.Values)
			.Concat(_gates.Values)
			.Concat(_hitTargets.Values)
			.Concat(_kickers.Values)
			.Concat(_lights.Values)
			.Concat(_lightSeqs.Values)
			.Concat(_plungers.Values)
			.Concat(_primitives.Values)
			.Concat(_ramps.Values)
			.Concat(_rubbers.Values)
			.Concat(_spinners.Values)
			.Concat(_surfaces.Values)
			.Concat(_textBoxes.Values)
			.Concat(_timers.Values)
			.Concat(_triggers.Values)
			.Concat(_troughs.Values);

		public IEnumerable<ItemData> ItemDatas => new ItemData[] { }
			.Concat(_bumpers.Values.Select(i => i.Data))
			.Concat(_decals.Select(i => i.Data))
			.Concat(_dispReels.Values.Select(i => i.Data))
			.Concat(_flippers.Values.Select(i => i.Data))
			.Concat(_flashers.Values.Select(i => i.Data))
			.Concat(_gates.Values.Select(i => i.Data))
			.Concat(_hitTargets.Values.Select(i => i.Data))
			.Concat(_kickers.Values.Select(i => i.Data))
			.Concat(_lights.Values.Select(i => i.Data))
			.Concat(_lightSeqs.Values.Select(i => i.Data))
			.Concat(_plungers.Values.Select(i => i.Data))
			.Concat(_primitives.Values.Select(i => i.Data))
			.Concat(_ramps.Values.Select(i => i.Data))
			.Concat(_rubbers.Values.Select(i => i.Data))
			.Concat(_spinners.Values.Select(i => i.Data))
			.Concat(_surfaces.Values.Select(i => i.Data))
			.Concat(_textBoxes.Values.Select(i => i.Data))
			.Concat(_timers.Values.Select(i => i.Data))
			.Concat(_triggers.Values.Select(i => i.Data))
			.Concat(_troughs.Values.Select(i => i.Data));

		public IEnumerable<ISwitchable> Switchables => new ISwitchable[0]
			.Concat(_bumpers.Values)
			.Concat(_flippers.Values)
			.Concat(_gates.Values)
			.Concat(_hitTargets.Values)
			.Concat(_kickers.Values)
			.Concat(_spinners.Values)
			.Concat(_triggers.Values);

		public IEnumerable<ISwitchableDevice> SwitchableDevices => new ISwitchableDevice[0]
			.Concat(_troughs.Values);

		public IEnumerable<ICoilable> Coilables => new ICoilable[0]
			.Concat(_bumpers.Values)
			.Concat(_flippers.Values)
			.Concat(_kickers.Values);

		public IEnumerable<ICoilableDevice> CoilableDevices => new ICoilableDevice[0]
			.Concat(_troughs.Values)
			.Concat(_plungers.Values);

		public IEnumerable<ILightable> Lightables => new ILightable[0]
			.Concat(_lights.Values)
			.Concat(_flashers.Values);

		private void AddItem<TItem>(string name, TItem item, IDictionary<string, TItem> d, bool updateStorageIndices) where TItem : IItem
		{
			if (updateStorageIndices) {
				item.StorageIndex = ItemDatas.Count();
				Data.NumGameItems = item.StorageIndex + 1;
			}
			d[name] = item;
		}

		private void AddItem<TItem>(TItem item, ICollection<TItem> d, bool updateStorageIndices) where TItem : IItem
		{
			if (updateStorageIndices) {
				item.StorageIndex = ItemDatas.Count();
			}
			d.Add(item);
		}

		private Dictionary<string, T> GetItemDictionary<T>() where T : IItem
		{
			if (typeof(T) == typeof(Bumper.Bumper)) {
				return _bumpers as Dictionary<string, T>;
			}
			if (typeof(T) == typeof(DispReel.DispReel)) {
				return _dispReels as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Flipper.Flipper)) {
				return _flippers as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Gate.Gate)) {
				return _gates as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(HitTarget.HitTarget)) {
				return _hitTargets as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Kicker.Kicker)) {
				return _kickers as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Light.Light)) {
				return _lights as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(LightSeq.LightSeq)) {
				return _lightSeqs as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Plunger.Plunger)) {
				return _plungers as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Flasher.Flasher)) {
				return _flashers as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Primitive.Primitive)) {
				return _primitives as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Ramp.Ramp)) {
				return _ramps as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Rubber.Rubber)) {
				return _rubbers as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Spinner.Spinner)) {
				return _spinners as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Surface.Surface)) {
				return _surfaces as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(TextBox.TextBox)) {
				return _textBoxes as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Timer.Timer)) {
				return _timers as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Trigger.Trigger)) {
				return _triggers as Dictionary<string, T>;
			}

			if (typeof(T) == typeof(Trough.Trough)) {
				return _troughs as Dictionary<string, T>;
			}

			return null;
		}

		private List<T> GetItemList<T>() {
			if (typeof(T) == typeof(Decal.Decal)) {
				return _decals as List<T>;
			}

			return null;
		}

		#endregion

		/// <summary>
		/// Adds a game item to the table.
		/// </summary>
		/// <param name="item">Game item instance</param>
		/// <param name="updateStorageIndices">If set, re-computes the storage indices. Only needed when adding game items via the editor.</param>
		/// <typeparam name="T">Game item type</typeparam>
		/// <exception cref="ArgumentException">Whe type of game item is unknown</exception>
		public void Add<T>(T item, bool updateStorageIndices = false) where T : IItem
		{
			var dict = GetItemDictionary<T>();
			if (dict != null) {
				AddItem(item.Name, item, dict, updateStorageIndices);

			} else {
				var list = GetItemList<T>();
				if (list != null) {
					AddItem(item, list, updateStorageIndices);

				} else {
					throw new ArgumentException("Unknown item type " + typeof(T) + ".");
				}
			}
		}

		/// <summary>
		/// Replaces all game items of a list with new game items.
		/// </summary>
		///
		/// <remarks>
		/// This only applied to Decals, because they are the only game items
		/// that don't have a name.
		/// </remarks>
		/// <param name="items">New list of game items</param>
		/// <typeparam name="T">Game item type (only Decals)</typeparam>
		/// <exception cref="ArgumentException">If not decals</exception>
		public void ReplaceAll<T>(IEnumerable<T> items) where T : IItem
		{
			var list = GetItemList<T>();
			if (list == null) {
				throw new ArgumentException("Cannot set all " + typeof(T) + "s (only Decals so far).");
			}
			list.Clear();
			list.AddRange(items);
		}

		/// <summary>
		/// Checks whether a game item of a given type exists.
		/// </summary>
		/// <param name="name">Name of the game item</param>
		/// <typeparam name="T">Type of the game item</typeparam>
		/// <returns>True if the game item exists, false otherwise</returns>
		public bool Has<T>(string name) where T : IItem => GetItemDictionary<T>().ContainsKey(name);

		/// <summary>
		/// Returns all game items of a given type.
		/// </summary>
		/// <typeparam name="TItem">Game item type</typeparam>
		/// <returns>All game items stored in the table</returns>
		/// <exception cref="ArgumentException">If invalid game type</exception>
		public TItem[] GetAll<TItem>() where TItem : IItem
		{
			var dict = GetItemDictionary<TItem>();
			if (dict != null) {
				return dict.Values.ToArray();
			}
			var list = GetItemList<TItem>();
			if (list != null) {
				return list.ToArray();
			}
			throw new ArgumentException("Unknown item type " + typeof(TItem) + ".");
		}

		/// <summary>
		/// Computes a new name for a game item.
		/// </summary>
		/// <param name="prefix">Prefix</param>
		/// <typeparam name="T">Type of the game item</typeparam>
		/// <returns>New name, a concatenation of the prefix and the next free index</returns>
		public string GetNewName<T>(string prefix) where T : IItem
		{
			var n = 0;
			var dict = GetItemDictionary<T>();
			do {
				var elementName = $"{prefix}{++n}";
				if (!dict.ContainsKey(elementName)) {
					return elementName;
				}
			} while (true);
		}

		/// <summary>
		/// Removes a game item from the table.
		/// </summary>
		/// <param name="name">Name of the game item</param>
		/// <typeparam name="T">Type of the game item</typeparam>
		public void Remove<T>(string name) where T : IItem
		{
			var dict = GetItemDictionary<T>();
			if (!dict.ContainsKey(name)) {
				return;
			}
			var removedStorageIndex = dict[name].StorageIndex;
			var gameItems = ItemDatas;
			foreach (var gameItem in gameItems) {
				if (gameItem.StorageIndex > removedStorageIndex) {
					gameItem.StorageIndex--;
				}
			}

			Data.NumGameItems = gameItems.Count() - 1;
			dict.Remove(name);
		}

		public TData[] GetAllData<TItem, TData>() where TItem : Item<TData> where TData : ItemData
		{
			var dict = GetItemDictionary<TItem>();
			if (dict != null) {
				return dict.Values.Select(d => d.Data).ToArray();
			}
			var list = GetItemList<TItem>();
			if (list != null) {
				return list.Select(d => d.Data).ToArray();
			}
			throw new ArgumentException("Unknown item type " + typeof(TItem) + ".");
		}

		#region Table Info
		public string InfoAuthorEmail => TableInfo.ContainsKey("AuthorEmail") ? TableInfo["AuthorEmail"] : null;
		public string InfoAuthorName => TableInfo.ContainsKey("AuthorName") ? TableInfo["AuthorName"] : null;
		public string InfoAuthorWebsite => TableInfo.ContainsKey("AuthorWebSite") ? TableInfo["AuthorWebSite"] : null;
		public string InfoReleaseDate => TableInfo.ContainsKey("ReleaseDate") ? TableInfo["ReleaseDate"] : null;
		public string InfoBlurb => TableInfo.ContainsKey("TableBlurb") ? TableInfo["TableBlurb"] : null;
		public string InfoDescription => TableInfo.ContainsKey("TableDescription") ? TableInfo["TableDescription"] : null;
		public string InfoName => TableInfo.ContainsKey("TableName") ? TableInfo["TableName"] : null;
		public string InfoRules => TableInfo.ContainsKey("TableRules") ? TableInfo["TableRules"] : null;
		public string InfoVersion => TableInfo.ContainsKey("TableVersion") ? TableInfo["TableVersion"] : null;

		#endregion

		private readonly TableMeshGenerator _meshGenerator;

		/// <summary>
		/// The API to load the table from a file.
		/// </summary>
		/// <param name="filename">Path to the VPX file</param>
		/// <param name="loadGameItems">If false, game items are not loaded. Useful when loading them on multiple threads.</param>
		/// <returns>The parsed table</returns>
		public static Table Load(string filename, bool loadGameItems = true)
		{
			return TableLoader.Load(filename, loadGameItems);
		}

		public Table(TableData data) : base(data)
		{
			_meshGenerator = new TableMeshGenerator(this);
		}

		public Table(BinaryReader reader) : this(new TableData(reader)) { }

		#region IRenderable

		Matrix3D IRenderable.TransformationMatrix(Table table, Origin origin) => Matrix3D.Identity;

		public RenderObject GetRenderObject(Table table, string id = null, Origin origin = Origin.Global, bool asRightHanded = true)
		{
			return _meshGenerator.GetRenderObject(asRightHanded);
		}

		public RenderObjectGroup GetRenderObjects(Table table, Origin origin = Origin.Global, bool asRightHanded = true)
		{
			return _meshGenerator.GetRenderObjects(table, origin, asRightHanded);
		}

		#endregion

		public bool IsCollidable => true;
		public bool HasTrough => _troughs.Count > 0;

		public void Save(string fileName)
		{
			new TableWriter(this).WriteTable(fileName);
			Logger.Info("File successfully saved to {0}.", fileName);
		}

		public Material GetMaterial(string name)
		{
			if (Data.Materials == null || name == null) {
				return null;
			}

			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var t in Data.Materials) {
				if (t.Name == name) {
					return t;
				}
			}

			return null;
		}

		public void SetTextureContainer(ITableResourceContainer<Texture> container)
		{
			Textures = container;
		}

		public Texture GetTexture(string name)
		{
			var tex = name == null
				? null
				: Textures[name.ToLower()];
			return tex;
		}

		public void SetSoundContainer(ITableResourceContainer<Sound.Sound> container)
		{
			Sounds = container;
		}

		public float GetScaleZ()
		{
			return Data.BgScaleZ?[Data.BgCurrentSet] ?? 1.0f;
		}

		public void SetupPlayfieldMesh()
		{
			if (_primitives.ContainsKey("playfield_mesh")) {
				_meshGenerator.SetFromPrimitive(_primitives["playfield_mesh"]);
				_primitives.Remove("playfield_mesh");
			}
		}

		public float GetSurfaceHeight(string surfaceName, float x, float y)
		{
			if (string.IsNullOrEmpty(surfaceName)) {
				return TableHeight;
			}

			if (_surfaces.ContainsKey(surfaceName)) {
				return TableHeight + _surfaces[surfaceName].Data.HeightTop;
			}

			if (_ramps.ContainsKey(surfaceName)) {
				return TableHeight + _ramps[surfaceName].GetSurfaceHeight(x, y, this);
			}

			// Logger.Warn(
			// 	"[Table.getSurfaceHeight] Unknown surface {0}.\nAvailable surfaces: [ {1} ]\nAvailable ramps: [ {2} ]",
			// 	surfaceName,
			// 	string.Join(", ", _surfaces.Keys),
			// 	string.Join(", ", _ramps.Keys)
			// );
			return TableHeight;
		}

		public int GetDetailLevel()
		{
			return 10; // TODO
		}

		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
	}
}

