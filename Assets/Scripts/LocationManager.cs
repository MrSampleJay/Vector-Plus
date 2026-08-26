using System;
using System.Collections.Generic;
using System.Xml;
using Core._Common;

public class LocationManager : AbstractManager<LocationManager>
{
	private Dictionary<string, Dictionary<int, int>> _rewards = new Dictionary<string, Dictionary<int, int>>();

	private Dictionary<string, Dictionary<LocationModeType, LocationInfo>> _locationInfos = new Dictionary<string, Dictionary<LocationModeType, LocationInfo>>();

	private Dictionary<string, StoryInfo> _allStoryInfos = new Dictionary<string, StoryInfo>();

	private CoinsSystem _coinsSystem;

	private string ListFilename => "List_Payed.xml";

	public Dictionary<string, Dictionary<LocationModeType, LocationInfo>> locations => _locationInfos;

	public CoinsSystem coinsSystem => _coinsSystem;

	protected override void InitInternal()
	{
		var doc = XmlUtils.OpenXMLDocument(VectorPaths.Commons, ListFilename);
		ParseRewards(doc);
		ParseLocations(doc);
		ParseCoinsSystem(doc);
	}

	private void ParseRewards(XmlDocument doc)
	{
		foreach	(XmlNode node in doc["LocationList"]["Templates"])
		{
			if (node.Name == "Reward")
			{
				string name = node.Attributes["Name"].Value;
				Dictionary<int, int> dicto = new Dictionary<int, int>();
				foreach (XmlNode stars in node)
				{
					dicto.Add(stars.Attributes["Count"].ParseInt(), stars.Attributes["Coins"].ParseInt());
				}
				_rewards[name] = dicto;
			}
		}
	}

	private void ParseLocations(XmlDocument doc)
	{
		foreach (XmlNode node in doc["LocationList"]["Locations"])
		{
			string name = node.Attributes["Name"].Value;
			int unlockPrice = node.Attributes["UnlockPrice"].ParseInt();
			var info = ParseLocationId(name);
			UnlockInfo condition = null;
			if (node["Conditions"] != null)
			{
				condition = ParseCondition(node["Conditions"], unlockPrice);
			}
			var locationInfo = new LocationInfo(name, name, condition, info.Item2, info.Item1, unlockPrice > 0);

			foreach (XmlNode group in node["Groups"])
			{
				List<StoryInfo> stories = new List<StoryInfo>();
				StoryType type = StoryType.Story;
				if (group.Attributes["Name"].Value == "BONUS")
				{
					type = StoryType.Bonus;
				}
				foreach (XmlNode story in group)
				{
					string storyName = story.Attributes["Name"].Value;
                    string storyDescription = story.Attributes["Description"].ParseString();
                    int storyPrice = story.Attributes["UnlockPrice"].ParseInt();
					UnlockInfo storyCondition = null;
                    if (story["Conditions"] != null)
                    {
                        storyCondition = ParseCondition(story["Conditions"], storyPrice);
                    }
					List<string> trickIDs = new List<string>();
					if (story["Tricks"] != null)
					{
                        foreach (XmlNode trick in story["Tricks"])
                        {
                            trickIDs.Add(trick.Attributes["Name"].Value);
                        }
                    }
					string rewardTemplate = story["Reward"].Attributes["Template"].Value;

					var cutsceneStart = story.Attributes["VideoStart"].ParseString();
                    var cutsceneEnd = story.Attributes["VideoEnd"].ParseString();

                    var storyInfo = new StoryInfo(storyName, type, trickIDs, storyCondition, rewardTemplate, cutsceneStart, cutsceneEnd, storyDescription);
					_allStoryInfos[storyInfo.Name] = storyInfo;
                    stories.Add(storyInfo);
                }
				locationInfo.AddStories(type, stories);
			}

			if (!_locationInfos.ContainsKey(locationInfo.LocationType))
			{
				Dictionary<LocationModeType, LocationInfo> dicto = new Dictionary<LocationModeType, LocationInfo>();
				dicto.Add(locationInfo.LocationModeType, locationInfo);
				_locationInfos.Add(locationInfo.LocationType, dicto);
			}
			else
			{
				_locationInfos[locationInfo.LocationType].Add(locationInfo.LocationModeType, locationInfo);
			}
		}
	}

	private UnlockInfo ParseCondition(XmlNode conditionNode, int unlockPrice)
	{
		if (conditionNode["Stars"] == null)
		{
			if (conditionNode["Payment"] == null)
			{
				return null;
			}
			var price = Convert.ToInt32(conditionNode["Payment"].Attributes["Required"].Value);
			return new UnlockInfo(null, price);
		}
		XmlNode starsNode = conditionNode["Stars"];
		var required = Convert.ToInt32(starsNode.Attributes["Required"].Value);
		string name = string.Empty;
		if (starsNode.Attributes["Subject"] != null)
		{
            if (starsNode.Attributes["Subject"].Value == "Track")
            {
                name = starsNode.Attributes["Name"].Value;
            }
        }
		var location = string.Empty;
		if (starsNode.Attributes["Name"] != null)
		{
            location = starsNode.Attributes["Name"].Value;
        }
		var tuple = ParseLocationId(location);

		LocationLocator locationLocator = null;

		if (tuple != null)
		{
			locationLocator = new LocationLocator();
            locationLocator.LocationType = tuple.Item1;
            locationLocator.LocationModeType = tuple.Item2;
        }

		StarsLocationUnlockInfo startsLocation = new StarsLocationUnlockInfo(required, name, locationLocator);
		return new UnlockInfo(startsLocation, unlockPrice);

	}

	private void ParseCoinsSystem(XmlDocument doc)
	{
		if (doc["LocationList"]["CoinSettings"] == null)
		{
			_coinsSystem = new CoinsSystem();
			return;
		}
		_coinsSystem = new CoinsSystem(doc["LocationList"]["CoinSettings"]);
	}

	private static Tuple<string, LocationModeType> ParseLocationId(string locationId)
	{
		if (!string.IsNullOrEmpty(locationId))
		{
            LocationModeType locationModeType = LocationModeType.Classic;
            if (locationId.Contains("_HUNTER"))
            {
                locationModeType = LocationModeType.Hunter;
            }
            return new Tuple<string, LocationModeType>(locationId.Replace("_HUNTER", ""), locationModeType);
        }
		return null;
	}

	public IEnumerable<string> GetLocationIds(LocationModeType locationModeType, string locationType)
	{
		var infos = _locationInfos[locationType][locationModeType].CurrentStoryInfos;
		string[] array = new string[infos.Count];
		for (int i = 0; i < infos.Count; i++)
		{
			array[i] = infos[i].Name;
		}
		return array;
	}

	public LocationInfo GetLocationInfo(string locationType, LocationModeType locationModeType)
	{
		return _locationInfos[locationType][locationModeType];
	}

	public StoryInfo GetStoryInfo(string id)
	{
		return _allStoryInfos[id];
	}

	public Dictionary<int, int> GetRewards(string rewardTemplate)
	{
		return _rewards[rewardTemplate];
	}
}
