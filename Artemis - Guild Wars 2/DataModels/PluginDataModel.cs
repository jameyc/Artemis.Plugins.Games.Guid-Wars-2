using Artemis.Core.Modules;
using System.Collections.Generic;
using Gw2Sharp;
using System.Linq;
using SkiaSharp;
using System;

namespace Artemis___Guild_Wars_2.DataModels
{
	/*
	public struct MapColors
	{
		public MapColors(string sky, string ground, string ambiant)
		{
			this.Sky = SKColor.Parse(sky);
			this.Ground = SKColor.Parse(ground);
			this.Ambiant = SKColor.Parse(ambiant);
		}

		public SkiaSharp.SKColor Ambiant { get; set; }
		public SkiaSharp.SKColor Sky { get; set; }
		public SkiaSharp.SKColor Ground { get; set; }
		// TODO - effect or similar? Per color? 
	}
	*/

	public class PluginDataModel : DataModel
	{ 
		public PluginDataModel()
		{
			CharacterInfo = new CharacterInfo();
			CompetitiveInfo = new CompetitiveInfo();
			MapInfo = new MapInfo();
			UiInfo = new UiInfo();
			TyrianTime = new TyrianTime();

			/*
			IDictionary<int, MapColors> mapColors = new Dictionary<int, MapColors>()
			{
				{15, new MapColors("0000ff", "553311", "dddddd") }
			};
			*/

		}

		public void Update(double deltaTime, Gw2Client client, bool doFullUpdate)
		{
			IsAvailable = client.Mumble.IsAvailable;

			// Subdatamodels
			CharacterInfo.Update(deltaTime, client, doFullUpdate);
			MapInfo.Update(deltaTime, client, doFullUpdate);
			UiInfo.Update(deltaTime, client, doFullUpdate);
			CompetitiveInfo.Update(deltaTime, client, doFullUpdate);
			TyrianTime.Update(deltaTime, client, doFullUpdate);
		}

		[DataModelProperty(Name = "Available", Description = "Mumble interface is up. Should always be true if module active.")]
		public bool IsAvailable { get; set; }

		// Subdatamodels
		[DataModelProperty(Name = "Character")]
		public CharacterInfo CharacterInfo { get; set; }

		[DataModelProperty(Name = "Competitive")]
		public CompetitiveInfo CompetitiveInfo { get; set; }

		[DataModelProperty(Name = "Map")]
		public MapInfo MapInfo { get; set; }

		[DataModelProperty(Name = "Tyrian Time")]
		public TyrianTime TyrianTime { get; set; }

		[DataModelProperty(Name = "UI")]
		public UiInfo UiInfo { get; set; }
	}

	public class CharacterInfo
	{
		public CharacterInfo() {
			ProfessionInfo = new ProfessionInfo();
		}

		public void Update(double deltaTime, Gw2Client client, bool doFullUpdate)
		{
			bool hasInfo = client.Mumble.IsAvailable;
			Mount = hasInfo ? client.Mumble.Mount.ToString() : "None";
			IsInCombat = hasInfo && client.Mumble.IsInCombat;

			if (doFullUpdate)
			{
				Name = hasInfo ? client.Mumble.CharacterName : "";
				Commander = hasInfo ? client.Mumble.IsCommander : false;
				Race = hasInfo ? client.Mumble.Race.ToString() : "";
			}

			ProfessionInfo.Update(deltaTime, client, doFullUpdate);

		}

		[DataModelProperty(Name = "Name", Description = "Character name")]
		public string Name { get; set; }

		[DataModelProperty(Name = "Race", Description = "Character race")]
		public string Race { get; set; }

		[DataModelProperty(Name = "Commanding", Description = "Is character displaying commander tag?")]
		public bool Commander { get; set; }

		[DataModelProperty(Name = "Current mount", Description = "Current mount type")]
		public string Mount { get; set; }

		[DataModelProperty(Name = "In combat", Description = "Is character in combat?")]
		public bool IsInCombat { get; set; }

		// Subdatamodels
		[DataModelProperty(Name = "Profession")]
		public ProfessionInfo ProfessionInfo { get; set; }
	}

	public class ProfessionInfo
	{
		private Gw2Sharp.WebApi.V2.IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Specialization> specializations;
		private string lastProfession;

		public void Update(double deltaTime, Gw2Client client, bool doFullUpdate) {
			bool hasInfo = client.Mumble.IsAvailable;
			if (hasInfo)
			{
				specializations = specializations is not null ? specializations : client.WebApi.V2.Specializations.AllAsync().Result;

				Specialization = specializations.FirstOrDefault(spec => spec.Id == client.Mumble.Specialization).Name;
				if (doFullUpdate)
				{
					Profession = hasInfo ? client.Mumble.Profession.ToString() : "";
				}

				if (hasInfo && Profession != lastProfession)
				{
					// initialize colors
					ProfessionColorLighter = SkiaSharp.SKColor.Parse(colors[Profession][0]);
					ProfessionColorLight = SkiaSharp.SKColor.Parse(colors[Profession][1]);
					ProfessionColorMedium = SkiaSharp.SKColor.Parse(colors[Profession][2]);
					ProfessionColorDark = SkiaSharp.SKColor.Parse(colors[Profession][3]);

					lastProfession = Profession;
				}
			}
		}

		// Exposing the wiki's color schemes here https://wiki.guildwars2.com/wiki/Guild_Wars_2_Wiki:Color_schemes
		private Dictionary<string, List<string>> colors = new Dictionary<string, List<string>>(){
			{"Guardian", new List<string>() {"CFEEFD", "BCE8FD", "72C1D9", "186885"} },
			{"Revenant", new List<string>() {"EBC9C2", "E4AEA3", "D16E5A", "A66356"}},
			{"Warrior",  new List<string>() {"FFF5BB", "FFF2A4", "FFD166", "CAAA2A"}},

			{"Engineer", new List<string>() {"E8C89F", "E8BC84", "D09C59", "87581D"}},
			{"Ranger",   new List<string>() {"E2F6D1", "D2F6BC", "8CDC82", "67A833"}},
			{"Thief",    new List<string>() { "E6D5D7", "E6D5D7", "E6D5D7", "974550"}},

			{"Elementalist", new List<string>() {"F6D2D1", "F6BEBC", "F68A87", "DC423E"}},
			{"Mesmer",       new List<string>() { "D7B2EA", "D09EEA", "B679D5", "69278A"}},
			{"Necromancer",  new List<string>() { "D5EDE1", "BFE6D0", "52A76F", "2C9D5D"}},

			// TODO - use this default
			{"Default", new List<string>() { "EEEEEE", "DDDDDD", "BBBBBB", "666666" } }
		};

		[DataModelProperty(Name = "Profession", Description = "Character profession")]
		public string Profession { get; set; }

		[DataModelProperty(Name = "Specialization", Description = "Character specialiazation")]
		public string Specialization { get; set; }

		[DataModelProperty(Name = "Profession Color - Lighter", Description = "Character profession color, lighter")]
		public SkiaSharp.SKColor ProfessionColorLighter { get; set; }

		[DataModelProperty(Name = "Profession Color - Light", Description = "Character profession color, light")]
		public SkiaSharp.SKColor ProfessionColorLight { get; set; }

		[DataModelProperty(Name = "Profession Color - Medium", Description = "Character profession color, medium")]
		public SkiaSharp.SKColor ProfessionColorMedium { get; set; }

		[DataModelProperty(Name = "Profession Color - Dark", Description = "Character profession color, dark")]
		public SkiaSharp.SKColor ProfessionColorDark { get; set; }

	}

	public class CompetitiveInfo
	{
		private Gw2Sharp.WebApi.V2.IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Color> colors;
		public CompetitiveInfo() {}

		public void Update(double deltaTime, Gw2Client client, bool doFullUpdate)
		{
			bool hasInfo = client.Mumble.IsAvailable;
			IsCompetitiveMode = hasInfo && client.Mumble.IsCompetitiveMode;
			if (hasInfo && doFullUpdate)
			{
				TeamColorId = hasInfo ? client.Mumble.TeamColorId : -1;

				if (TeamColorId >= 0)
				{
					colors = colors is not null ? colors : client.WebApi.V2.Colors.AllAsync().Result;
					Gw2Sharp.WebApi.V2.Models.Color color = colors.FirstOrDefault(color => color.Id == TeamColorId);

					TeamColorName = TeamColorId > 0 ? color.Name : "";
					TeamColorRGB = TeamColorId > 0 ? color.BaseRgb.ToList() : new List<int>() { 0, 0, 0 };
					string colorHex = string.Join("", TeamColorRGB.Select(channelDec => channelDec.ToString("X")));
					TeamColorHex = SkiaSharp.SKColor.Parse(colorHex);
				}
			}
		}


		[DataModelProperty(Name = "In competitive mode", Description = "Is client in competitive mode such as PvP or WvW?")]
		public bool IsCompetitiveMode { get; set; }

		[DataModelProperty(Name = "Team color Name", Description = "Current team color name")]
		public string TeamColorName { get; set; }

		[DataModelProperty(Name = "Team color ID", Description = "Current team color id (debugging purposes)")]
		public int TeamColorId { get; set; }

		[DataModelProperty(Name = "Team color RGB", Description = "Current team color as [r, g, b]")]
		public List<int> TeamColorRGB { get; set; }

		[DataModelProperty(Name = "Team color", Description = "Current team color")]
		public SkiaSharp.SKColor TeamColorHex { get; set; }

	}
	public class MapInfo
	{
		private Gw2Sharp.WebApi.V2.IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.Map> mapNames = null;
		private List<double> lastLocation;

		public MapInfo() {}

		public void Update(double deltaTime, Gw2Client client, bool doFullUpdate)
		{
			bool hasInfo = client.Mumble.IsAvailable;
			mapNames = mapNames is not null ? mapNames : client.WebApi.V2.Maps.AllAsync().Result;

			CompassRotation = hasInfo ? client.Mumble.CompassRotation : 0;
			MapCenter = hasInfo ? client.Mumble.MapCenter.ToList() : new List<double>() { 0, 0 };
			MapId = hasInfo ? client.Mumble.MapId : -1;
			MapName = MapId >= 0 ? mapNames.FirstOrDefault(map => map.Id == MapId).Name : "";
			MapScale = hasInfo ? client.Mumble.MapScale : 1;
			MapType = hasInfo ? client.Mumble.MapType.ToString() : "";
			PlayerLocationMap = hasInfo ? client.Mumble.PlayerLocationMap.ToList() : new List<double>() { 0, 0 };

			List<double> currentLocation = client.Mumble.PlayerLocationMap.ToList();

			string TimeOfDay = TyrianTime.GetTimeOfDay();
			// Could probably store these in a json for the user to load with defs, that way there was
			// external loading. Use defaults or error if it's not loaded.
			SkyColor = SKColor.Parse(TimeOfDay == "Night" ? "ff000011" : TimeOfDay == "Day" ? "ff3333bb" : "ff553344");
			AmbientColor = SKColor.Parse(TimeOfDay == "Night" ? "ff022205" : TimeOfDay == "Day" ? "ff11aa22" : "ff007722");
			GroundColor = SKColor.Parse(TimeOfDay == "Night" ? "ff443311" : TimeOfDay == "Day" ? "ff666644" : "ff554411");
			/*
			if (fuzzyCompareLocations(lastLocation, currentLocation))
			{
				SecondsAtCurrentLocation += deltaTime;
			} else
			{
				SecondsAtCurrentLocation = 0;
			}
			lastLocation = currentLocation;
			*/
			// SecondsAtCurrentLocation = fuzzyCompareLocations(lastLocation, currentLocation) ? SecondsAtCurrentLocation + deltaTime : 0;
			// lastLocation = currentLocation;
		}

		// TODO - Eventually this might tell if the user is standing still for a while
		private bool fuzzyCompareLocations(List<double> locationA, List<double> locationB)
		{
			bool fuzzyCompareNumbers(double a, double b, double t = 1) { return System.Math.Abs(a - b) < t; }

			IEnumerable<bool> matches = locationA.Zip(locationB, (a, b) => { return fuzzyCompareNumbers(a, b); } );
			return matches.All(m => m == true); // matches.Aggregate(true, (bool acc, bool coordsMatched) => acc && coordsMatched);
		}

		[DataModelProperty(Name = "Compass Rotation", Description = "The compass rotation")]
		public double CompassRotation { get; set; }

		[DataModelProperty(Name = "Map center", Description = "Center of current map")]
		public List<double> MapCenter { get; set; }

		[DataModelProperty(Name = "Map ID", Description = "Current map ID")]
		public int MapId { get; set; }

		[DataModelProperty(Name = "Map Name", Description = "Current map Name")]
		public string MapName { get; set; }

		[DataModelProperty(Name = "Map scale", Description = "Scale of current map")]
		public double MapScale { get; set; }

		[DataModelProperty(Name = "Sky color", Description = "Current map's sky color")]
		public SkiaSharp.SKColor SkyColor { get; set; }

		[DataModelProperty(Name = "Ambient color", Description = "Current map's ambient color")]
		public SkiaSharp.SKColor AmbientColor { get; set; }

		[DataModelProperty(Name = "Ground color", Description = "Current map's ground color")]
		public SkiaSharp.SKColor GroundColor { get; set; }

		// See https://github.archomeda.eu/Gw2Sharp/master/api/Gw2Sharp.Models.MapType.html for types
		[DataModelProperty(Name = "Map type", Description = "Current map type, mostly used by WvW")]
		public string MapType { get; set; }

		[DataModelProperty(Name = "Player's map location", Description = "Player's location in map")]
		public List<double> PlayerLocationMap { get; set; }

		[DataModelProperty(Name = "Seconds at current location", Description = "How long has the player been in this spot? Used to detect standing still")]
		public double SecondsAtCurrentLocation { get; set; }
	}

	public class TyrianTime
	{
		public System.DateTime moment;

		public TyrianTime() { }

		public void Update(double deltaTime, Gw2Client client, bool doFullUpdate)
		{
			// This may need to be offset by an hour
			moment = System.DateTime.UtcNow;

			// 5 mins per Tyrian hour
			Hour = GetTyrianHour();
			Minute = GetTyrianMinute();
			TimeOfDay = GetTimeOfDay();

		}

		public static int GetTyrianHour()
		{
			// This may need to be inverted if times are
			// offset by half a day (add 1 to hr to invert)
			System.DateTime moment = System.DateTime.UtcNow;
			return (moment.Hour % 2) * 12 + moment.Minute / 5;
		}

		public static int GetTyrianMinute()
		{
			// Every real world minute = 20 Tyrian minutes.
			// Because the 2 hour cycle is asymmetric...
			// real world minutes
			// 1 minute = 12 mins
			// 30 seconds = 6 mins
			// 15 seconds = 3 mins
			// 5 seconds = 1 min
			System.DateTime moment = System.DateTime.UtcNow;
			return (moment.Minute % 5) * 12 + (moment.Second / 5);

			// or maybe
			// TimeSpan test = DateTime.UtcNow - new DateTime.UtcNow; // TODO zero out second utcnow's hh:mm:ss
			// int TyrianMinutes = test.TotalSeconds / 3;
		}

		public static string GetTimeOfDay()
		{
			int Hour = System.DateTime.UtcNow.Hour;
			return (Hour > 21 || Hour < 5) ? "Night" : Hour > 20 ? "Dusk" : Hour > 6 ? "Day" : "Dawn";
		}

		[DataModelProperty(Name = "Hour", Description = "")]
		public int Hour { get; set; }

		[DataModelProperty(Name = "Minute", Description = "")]
		public int Minute { get; set; }

		[DataModelProperty(Name = "Time of Day", Description = "Time of day (Dawn, Day, Dusk, Night)")]
		public string TimeOfDay { get; set; }

	}

	public class UiInfo
	{
		public UiInfo() {}

		public void Update(double deltaTime, Gw2Client client, bool doFullUpdate)
		{
			bool hasInfo = client.Mumble.IsAvailable;

			DoesGameHaveFocus = hasInfo && client.Mumble.DoesGameHaveFocus;
			IsMapOpen = hasInfo && client.Mumble.IsMapOpen;
			DoesAnyInputHaveFocus = hasInfo && client.Mumble.DoesAnyInputHaveFocus;
			IsCompassRotationEnabled = hasInfo && client.Mumble.IsCompassRotationEnabled;
			IsCompassTopRight = hasInfo && client.Mumble.IsCompassTopRight;

			FieldOfView = hasInfo && doFullUpdate ? client.Mumble.FieldOfView : FieldOfView;
			UiSize = hasInfo && doFullUpdate ? client.Mumble.UiSize.ToString() : UiSize;

		}

		[DataModelProperty(Name = "UI size", Description = "UI size setting")]
		public string UiSize { get; set; }

		[DataModelProperty(Name = "Compass size", Description = "")]
		public bool Compass { get; set; }

		[DataModelProperty(Name = "An input has focus", Description = "Does a textbox have focus?")]
		public bool DoesAnyInputHaveFocus { get; set; }

		[DataModelProperty(Name = "Game has focus", Description = "Does the game have focus?")]
		public bool DoesGameHaveFocus { get; set; }

		[DataModelProperty(Name = "Field Of view", Description = "Vertical field Of view")]
		public double FieldOfView { get; set; }

		[DataModelProperty(Name = "Compass rotation enabled", Description = "Is compass rotation enabled?")]
		public bool IsCompassRotationEnabled { get; set; }

		[DataModelProperty(Name = "Compass at top right", Description = "Is compass at top right?")]
		public bool IsCompassTopRight { get; set; }

		[DataModelProperty(Name = "Map open", Description = "Is the map open on the UI?")]
		public bool IsMapOpen { get; set; }
	}

	public class ProcessInfo
	{
		public ProcessInfo() {}

		public void update(double deltaTime, Gw2Client client, bool doFullUpdate)
		{
			bool hasInfo = client.Mumble.IsAvailable;

			BuildId = hasInfo ? client.Mumble.BuildId : 0;
			ProcessId = hasInfo ? client.Mumble.ProcessId : 0;
			RawIdentity = hasInfo ? client.Mumble.RawIdentity : "";
			ServerAddress = hasInfo ? client.Mumble.ServerAddress : "";
			ServerPort = (ushort)(hasInfo ? client.Mumble.ServerPort : 0);
			ShardId = hasInfo ? client.Mumble.ShardId : 0;
		}

		[DataModelProperty(Name = "Build ID", Description = "Current Build ID")]
		public int BuildId { get; set; }

		[DataModelProperty(Name = "Process ID", Description = "Current running GW2 process ID")]
		public uint ProcessId { get; set; }

		[DataModelProperty(Name = "Raw identity", Description = "Raw json debugging output")]
		public string RawIdentity { get; set; }

		[DataModelProperty(Name = "Server address", Description = "Curretly connected GW2 server address")]
		public string ServerAddress { get; set; }

		[DataModelProperty(Name = "Server port", Description = "Currently connected GW2 server port")]
		public ushort ServerPort { get; set; }

		[DataModelProperty(Name = "Shard ID", Description = "Currently connected GW2 shard ID")]
		public double ShardId { get; set; }

	}
}