
using System.Reflection;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace ImBlindedByTheLights {
	internal class Config {
		public static Config Instance { get; set; }

		public virtual bool enablePlugin { get; set; } = true;
		
		public virtual Color staticColor { get; set; } = Color.gray;

		public virtual bool staticInHeadset { get; set; } = true;
		public virtual bool staticOnDesktop { get; set; } = false;

		public virtual bool keepRingSpinsInStatic { get; set; } = false;
		public virtual bool disableBackLasers { get; set; } = false;
		public virtual bool disableRingLights { get; set; } = false;
		public virtual bool disableRotatingLasers { get; set; } = false;
		public virtual bool disableCenterLights { get; set; } = false;

		/// <summary>
		/// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
		/// </summary>
		public virtual void OnReload() {
			// Do stuff after config is read from disk.
		}

		/// <summary>
		/// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
		/// </summary>
		public virtual void Changed() {
			// Do stuff when the config is changed.
		}

		/// <summary>
		/// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
		/// </summary>
		public virtual void CopyFrom(Config other) {
			// This instance's members populated from other
		}
	}
}
