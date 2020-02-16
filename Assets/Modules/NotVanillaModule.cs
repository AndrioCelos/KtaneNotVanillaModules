using JetBrains.Annotations;
using NotVanillaModulesLib;
using UnityEngine;

public abstract class NotVanillaModule<TConnector> : MonoBehaviour where TConnector : NotVanillaModuleConnector {
	protected TConnector Connector { get; private set; }
	public bool Solved { get; private set; }

	public virtual void Start() {
		this.Connector = this.GetComponent<TConnector>();
	}

	public virtual void Disarm() {
		this.Solved = true;
		this.Connector.KMBombModule.HandlePass();
	}

	public void Log(string message) { this.Connector.Log(message); }
	[StringFormatMethod("format")]
	public void Log(string format, params object[] args) { this.Connector.Log(format, args); }
}
