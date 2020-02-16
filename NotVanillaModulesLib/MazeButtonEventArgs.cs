using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotVanillaModulesLib {
	public class MazeButtonEventArgs : EventArgs {
		public MazeDirection Direction { get; }

		public MazeButtonEventArgs(MazeDirection direction) => this.Direction = direction;
	}
}
