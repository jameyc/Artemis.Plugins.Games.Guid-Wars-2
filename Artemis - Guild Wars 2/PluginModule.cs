using Artemis.Core;
using Artemis.Core.Modules;
using Artemis___Guild_Wars_2.DataModels;
using SkiaSharp;
using System.Collections.Generic;

namespace Artemis___Guild_Wars_2
{
	// The core of your module. Hover over the method names to see a description.
	[PluginFeature(Name = "Guild Wars 2", Icon = "ToyBrickPlus")]
	public class PluginModule : Module<PluginDataModel>
	{
		private Gw2Sharp.Gw2Client client;
		private Gw2Sharp.Connection connection;
		private readonly double secondsBetweenFullUpdates = 10;
		private double secondsUntilFullUpdate; // arbitrarily high to force initial full update

		// This is useful if your module targets a specific game or application.
		// If this list is not null and not empty, the data of your module is only available to profiles specifically targeting it
		public override List<IModuleActivationRequirement> ActivationRequirements => new() {
			new ProcessActivationRequirement("Gw2-64")
		};

		// This is the beginning of your plugin feature's life cycle. Use this instead of a constructor.
		public override void Enable()
		{
			// Anything you'd otherwise do in a constructor is done here

		}

		// This is the end of your plugin feature's life cycle.
		public override void Disable()
		{
			// Make sure to clean up resources where needed (dispose IDisposables etc.)

		}

		public override void ModuleActivated(bool isOverride)
		{
			// When this gets called your activation requirements have
			// been met and the module will start displaying
			// You can remove this if you don't need it
			connection = connection is null ? new Gw2Sharp.Connection() : connection;
			client = new Gw2Sharp.Gw2Client(connection);
		}

		public override void ModuleDeactivated(bool isOverride)
		{
			// When this gets called your activation requirements are no longer met and your module will stop displaying
			// You can remove this if you don't need it
			client.Dispose();
		}

		public override void Update(double deltaTime)
		{
			// TODO - invert this timer crap for readability
			var doFullUpdate = false;
			if ((secondsUntilFullUpdate -= deltaTime) <= 0)
			{
				secondsUntilFullUpdate = secondsBetweenFullUpdates;
				doFullUpdate = true;
			}
			client.Mumble.Update();
			DataModel.Update(deltaTime, client, doFullUpdate);
		}
	}
}