using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace NotVanillaModulesLib {
	/// <summary>Connects a Not Vanilla module with the vanilla game's model or the test model.</summary>
	// This class deals only with manipulating the test or vanilla model components and does not contain puzzle logic.
	// Puzzle logic is in the Unity project scripts.
	public abstract class NotVanillaModuleConnector : MonoBehaviour {
		/// <summary>Returns a value indicating whether the test model is being used. The value returned is not valid during Awake.</summary>
		public bool TestMode { get; protected set; }
		/// <summary>A <see cref="GameObject"/> containing the test model, which will be hidden outside test mode. May be null.</summary>

		/// <summary>Returns the sequential numeric ID for this module, unique within the module type and game instance.</summary>
		public int ModuleID { get; private set; }

		/// <summary>Returns the <see cref="global::KMBombModule"/> component attached to the GameObject. The value returned is not valid during Awake.</summary>
		public KMBombModule KMBombModule { get; private set; }
		/// <summary>Returns the <see cref="global::KMNeedyModule"/> component attached to the GameObject. The value returned is not valid during Awake.</summary>
		public KMNeedyModule KMNeedyModule { get; private set; }

		public GameObject TestModel;

		private static readonly Dictionary<string, int> moduleIndices = new Dictionary<string, int>();

		public void Awake() {
			string moduleType;
			this.KMBombModule = this.GetComponent<KMBombModule>();
			if (this.KMBombModule != null) moduleType = this.KMBombModule.ModuleType;
			else {
				this.KMNeedyModule = this.GetComponent<KMNeedyModule>();
				if (this.KMNeedyModule != null) moduleType = this.KMNeedyModule.ModuleType;
				else {
					moduleType = this.gameObject.name;
					Debug.LogError($"[{moduleType}] Module is missing a KMBombModule or KMNeedyModule component.");
				}
			}

			moduleIndices.TryGetValue(moduleType, out var id);
			this.ModuleID = ++id;
			moduleIndices[moduleType] = id;

#if (DEBUG)
			this.Log("Assembly was compiled in debug mode. Activating test model.");
			this.TestMode = true;
			this.TestModel?.SetActive(true);
			this.AwakeTest();
#else
			try {
				this.AwakeLive();
				this.TestModel?.SetActive(false);
			} catch (TypeLoadException) {
				this.Log("Can't load BombGenerator. Activating test model.");
				this.TestMode = true;
				this.TestModel?.SetActive(true);
				this.AwakeTest();
			}
#endif
		}

		/// <summary>Attempts to instantiate the game's model for this module.</summary>
		/// <exception cref="TypeLoadException">The game components cannot be loaded because the module is running in the test harness.</exception>
		protected abstract void AwakeLive();
		/// <summary>Prepares the module for test mode.</summary>
		protected abstract void AwakeTest();

		public virtual void Start() {
			if (this.TestMode) this.StartTest();
			else this.StartLive();
		}

		/// <summary>Called by the Unity <see cref="Start"/> method in live mode.</summary>
		protected abstract void StartLive();
		/// <summary>Called by the Unity <see cref="Start"/> method in test mode.</summary>
		protected abstract void StartTest();

#if (!DEBUG)
		/// <summary>Returns the prefab of the specified type from the <see cref="BombGenerator"/>.</summary>
		protected static T GetComponentPrefab<T>() where T : BombComponent
			=> FindObjectOfType<BombGenerator>().componentPrefabs.OfType<T>().First();

		/// <summary>Instantiates an inactive <see cref="BombComponent"/> of the specified type using the <see cref="BombGenerator"/> prefab.</summary>
		/// <remarks>
		/// Unless it is activated or its parent is changed to an active GameObject, the instance will not receive an Awake or Start call.
		///	Unless its parent is changed, the instance will be automatically destroyed with this GameObject.
		///	</remarks>
		// We avoid Awaking the new instance because that would interfere with module numbers and possibly cause other unwanted side effects.
		protected TempComponentWrapper<T> InstantiateComponent<T>() where T : BombComponent {
			var modulePrefab = GetComponentPrefab<T>();
			var tempParent = new GameObject(modulePrefab.name + " Temp Parent").transform;
			InstanceDestroyer.AddObjectToDestroy(this.gameObject, tempParent);
			tempParent.gameObject.SetActive(false);
			return new TempComponentWrapper<T>(tempParent, Instantiate(modulePrefab, tempParent, false));
		}
#endif

		/// <summary>Writes the specified message to the log file with a prefix identifying this module.</summary>
		public void Log(string message) {
			string name = this.KMBombModule?.ModuleDisplayName ?? this.KMNeedyModule?.ModuleDisplayName ?? this.GetType().Name;
			Debug.Log($"[{name} #{this.ModuleID}] {message}");
		}
		/// <summary>Writes the specified message to the log file with a prefix identifying this module.</summary>
		/// <seealso cref="string.Format(string, object)"/>
		[StringFormatMethod("format")]
		public void Log(string format, params object[] args) => this.Log(string.Format(format, args));

#if (!DEBUG)
		/// <summary>
		/// Wraps a temporary <see cref="BombComponent"/> instance and parent <see cref="GameObject"/> using the Disposable pattern.
		/// This class is not available in debug builds.
		/// </summary>
		protected sealed class TempComponentWrapper<T> : IDisposable where T : BombComponent {
			public Transform TempParent { get; }
			public T Component { get; }

			public TempComponentWrapper(Transform tempParent, T component) {
				this.TempParent = tempParent ?? throw new ArgumentNullException(nameof(tempParent));
				this.Component = component ?? throw new ArgumentNullException(nameof(component));
			}

			public void Dispose() => Destroy(this.TempParent.gameObject);
		}
#endif
	}
}
