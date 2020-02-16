using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NotVanillaModulesLib.TestModel;
using UnityEngine;

namespace NotVanillaModulesLib {
	public class NotMazeConnector : NotVanillaModuleConnector {
		public TestModelButton[] TestModelButtons;
		public Transform GridParent;
		public GameObject SpacePrefab;

		public GameObject WallPrefab;
		public GameObject MarkerPrefab;

		public Material SpaceMaterial;
		public Material DotMaterial;
		public Material GoalMaterial;

		public bool Activated { get; private set; }

		public event EventHandler<MazeButtonEventArgs> ButtonPressed;

		private GameObject[,] spaces;
		private Vector2Int position;
		private Vector2Int goalPosition;

		public bool Initialised { get; private set; }

#if (!DEBUG)
		private IList<KeypadButton> buttons;
		private TempComponentWrapper<InvisibleWallsComponent> tempModuleWrapper;
		private InvisibleWallsComponent tempModuleClone;
		private List<List<MazeCell>> mazeCells;
		private List<List<InvisibleMazeCell>> invisibleMazeCells;
		private Transform playerIndicator;
#endif

		protected override void AwakeLive() {
#if (!DEBUG)
			this.tempModuleWrapper = this.InstantiateComponent<InvisibleWallsComponent>();
			this.tempModuleClone = this.tempModuleWrapper.Component;
			this.playerIndicator = this.tempModuleClone.transform.Find("PlayerIndicator");
			this.playerIndicator.SetParent(this.transform, false);
			this.playerIndicator.GetComponent<Renderer>().material = this.DotMaterial;
			this.GridParent = this.tempModuleClone.Background.transform;
			this.GridParent.SetParent(this.transform, false);
			var layout = this.tempModuleClone.transform.Find("Component_Maze");
			layout.SetParent(this.transform, false);

			// We need to prevent this object from being destroyed as well.
			this.tempModuleClone.transform.Find("WallSegmentPrefab").parent = null;

			this.buttons = this.tempModuleClone.Buttons;

			var keypadEventConnector = new KeypadEventConnector();
			keypadEventConnector.ButtonPressed += this.KeypadEventConnector_ButtonPressed;
			keypadEventConnector.Attach(this.buttons);

			this.mazeCells = new List<List<MazeCell>>(
				Enumerable.Range(0, 6).Select(x => new List<MazeCell>(
					Enumerable.Range(0, 6).Select(y => new MazeCell(x, y))
				))
			);
#endif
		}

		protected override void AwakeTest() {
			this.spaces = new GameObject[6, 6];
			for (int y = 0; y < 6; ++y) {
				for (int x = 0; x < 6; ++x) {
					this.spaces[x, y] = Instantiate(this.SpacePrefab, this.GridParent);
					this.spaces[x, y].GetComponent<Renderer>().enabled = false;
					this.spaces[x, y].transform.localPosition = new Vector3(x * 0.12f - 0.3f, 0, 0.3f - y * 0.12f);
				}
			}
		}
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			for (int i = 0; i < this.buttons.Count; ++i) {
				var selectable1 = this.buttons[i].GetComponent<Selectable>();
				selectable.Children[i * 2 + 1] = selectable1;
				selectable1.Parent = selectable;
			}
			this.GridParent.gameObject.SetActive(false);
#endif
		}
		protected override void StartTest() {
			foreach (var button in this.TestModelButtons)
				button.Pressed += (sender, e) => this.ButtonPressed?.Invoke(this, new MazeButtonEventArgs((MazeDirection) e.ButtonIndex));
		}

		public void Update() {
			if (this.TestMode && this.Initialised)
				this.spaces[this.goalPosition.x, this.goalPosition.y].transform.localEulerAngles += new Vector3(0, 1, 0);
		}

#if (!DEBUG)
		private void KeypadEventConnector_ButtonPressed(object sender, KeypadButtonEventArgs e)
			=> this.ButtonPressed?.Invoke(sender, new MazeButtonEventArgs((MazeDirection) e.ButtonIndex));
#endif

		public void Activate() {
			this.Activated = true;
			if (this.TestMode) {
				foreach (var space in this.spaces) space.GetComponent<Renderer>().enabled = true;
			}
#if (!DEBUG)
			else {
				this.GridParent.gameObject.SetActive(true);
				this.invisibleMazeCells[this.position.x][this.position.y].Select();
			}
#endif
		}

		public void SetGoal(int x, int y) {
			this.goalPosition = new Vector2Int(x, y);
			if (this.TestMode) {
				this.spaces[this.goalPosition.x, this.goalPosition.y].transform.localScale *= 1.5f;
				this.spaces[this.goalPosition.x, this.goalPosition.y].GetComponent<Renderer>().material = this.GoalMaterial;
			}
		}
		public void SetGoal(Vector2Int point) => this.SetGoal(point.x, point.y);

		public void SetPosition(int x, int y) {
			if (this.TestMode) {
				if (this.position != this.goalPosition)
					this.spaces[this.position.x, this.position.y].GetComponent<Renderer>().material = this.SpaceMaterial;
				this.position = new Vector2Int(x, y);
				if (this.position != this.goalPosition)
					this.spaces[this.position.x, this.position.y].GetComponent<Renderer>().material = this.DotMaterial;
			}
#if (!DEBUG)
			else {
				if (this.invisibleMazeCells != null && this.Activated) {
					this.invisibleMazeCells[this.position.x][this.position.y].Deselect();
					if (this.goalPosition == this.position) this.invisibleMazeCells[this.position.x][this.position.y].GetComponent<Renderer>().enabled = false;
					this.position = new Vector2Int(x, y);
					if (this.position != this.goalPosition)
						this.invisibleMazeCells[this.position.x][this.position.y].Select();
				} else
					this.position = new Vector2Int(x, y);
			}
#endif
		}
		public void SetPosition(Vector2Int point) => this.SetPosition(point.x, point.y);

		public void ShowMarker(int x, int y) {
			if (this.TestMode) {
				var name = $"Marker {x} {y}";
				if (this.GridParent.transform.Find(name) == null) {
					var marker = Instantiate(this.MarkerPrefab, this.GridParent);
					marker.name = name;
					marker.transform.localPosition = new Vector3(x * 0.12f - 0.3f, 0, 0.3f - y * 0.12f);
				}
			}
#if (!DEBUG)
			else if (this.Initialised) throw new InvalidOperationException("Cannot set markers after the maze is initialised.");
			else this.mazeCells[x][y].IsIdentifier = true;
#endif
		}
		public void ShowMarker(Vector2Int point) => this.ShowMarker(point.x, point.y);

		public void ShowWall(int x, int y, MazeDirection direction) {
			if (this.TestMode) {
				switch (direction) {
					case MazeDirection.Up: --y; direction = MazeDirection.Down; break;
					case MazeDirection.Left: --x; direction = MazeDirection.Right; break;
				}
				var name = $"Wall {x} {y} {direction}";
				if (this.GridParent.transform.Find(name) == null) {
					var wall = Instantiate(this.WallPrefab, this.GridParent);
					wall.name = name;
					if (direction == MazeDirection.Right) {
						wall.transform.localPosition = new Vector3(x * 0.12f - 0.24f, 0, 0.3f - y * 0.12f);
					} else {
						wall.transform.localPosition = new Vector3(x * 0.12f - 0.3f, 0, 0.24f - y * 0.12f);
						wall.transform.localEulerAngles = new Vector3(0, 90, 0);
					}
				}
			}
#if (!DEBUG)
			else this.invisibleMazeCells[x][y].ShowWalls(-1, direction switch {
				MazeDirection.Up => BombGame.Direction.Up,
				MazeDirection.Left => BombGame.Direction.Left,
				MazeDirection.Right => BombGame.Direction.Right,
				MazeDirection.Down => BombGame.Direction.Down,
				_ => 0 });
#endif
		}
		public void ShowWall(Vector2Int point, MazeDirection direction) => this.ShowWall(point.x, point.y, direction);

		public void InitialiseMaze() {
			if (this.Initialised) throw new InvalidOperationException("The maze has already been initialised.");
#if (!DEBUG)
			if (!this.TestMode) {
				var maze = new Maze { CellGrid = this.mazeCells };
				typeof(InvisibleWallsComponent).GetProperty("Maze").SetValue(this.tempModuleClone, maze, null);
				typeof(InvisibleWallsComponent).GetProperty("GoalCell").SetValue(this.tempModuleClone, this.mazeCells[this.goalPosition.x][this.goalPosition.y], null);
				typeof(InvisibleWallsComponent).GetMethod("InitCells", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this.tempModuleClone, null);
				this.invisibleMazeCells = this.tempModuleClone.Cells;
				foreach (var cell in this.invisibleMazeCells.SelectMany(l => l)) cell.transform.localScale = Vector3.one;

				var goalCell = this.invisibleMazeCells[this.goalPosition.x][this.goalPosition.y];
				goalCell.Goal.GetComponent<Renderer>().material = this.GoalMaterial;

				this.tempModuleWrapper.Dispose();
			}
#endif
			this.Initialised = true;
		}

		public void TwitchPress(MazeDirection direction) {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelButtons[(int) direction]);
#if (!DEBUG)
			else TwitchExtensions.Click(this.buttons[(int) direction]);
#endif
		}
	}
}
