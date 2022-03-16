public class MapInfo
{
	public long MapID { get; set; }
	public string MapName { get; set; }
	public long? WorkshopID { get; set; }
	public string OfficialID { get; set; }
	public bool OfficialMap { get; set; }

	private MapInfo() { }
	public MapInfo(long mapID, string mapName, long? workshopID, string officialID, bool officialMap)
    {
		MapID = mapID;
		MapName = mapName;
		WorkshopID = workshopID;
		OfficialID = officialID;
		OfficialMap = officialMap;
    }

    public override string ToString()
    {
        return $"Map Name: {MapName}\nMap ID: {MapID}\nWorkshop ID: {WorkshopID}\nOfficial ID: {OfficialID}\nOfficial Map: {OfficialMap}";
    }
}
