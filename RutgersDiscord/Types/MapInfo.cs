﻿using Dapper.Contrib.Extensions;
[Table("maps")]
public class MapInfo
{
	public int MapID { get; set; }
	public string MapName { get; set; }
	public long? WorkshopID { get; set; }
	public string OfficialID { get; set; }
	public bool OfficialMap { get; set; }

	private MapInfo() { }
	public MapInfo(int mapID, string mapName, long? workshopID, string officialID, bool officialMap)
    {
		MapID = mapID;
		MapName = mapName;
		WorkshopID = workshopID;
		OfficialID = officialID;
		OfficialMap = officialMap;
    }

    public override string ToString()
    {
		string str = $"ID:{MapID} {MapName} ";
		str += OfficialMap ? OfficialID : WorkshopID;
		return str;
    }
}
