using Dapper.Contrib.Extensions;
[Table("maps")]
public class MapInfo
{
	[ExplicitKey]
	public int MapID { get; set; }
	public string MapName { get; set; }
	public long? WorkshopID { get; set; }
	public string OfficialID { get; set; }
	public bool OfficialMap { get; set; }
	public string MapImage { get; set; }

	private MapInfo() { }
	public MapInfo(int mapID, string mapName, long? workshopID, string officialID, bool officialMap, string mapImage)
    {
		MapID = mapID;
		MapName = mapName;
		WorkshopID = workshopID;
		OfficialID = officialID;
		OfficialMap = officialMap;
		MapImage = mapImage;
    }

    public override string ToString()
    {
		string str = $"ID:{MapID} {MapName} ";
		str += OfficialMap ? OfficialID : WorkshopID;
		return str;
    }
}
