using System;
using System.Text.Json.Serialization;

#nullable enable
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


namespace RiotClasses
{
	public class PlatformDataDto
	{
		public string id { get; set; }

		public string name { get; set; }

		public string[] locales { get; set; }
		public StatusDto[] maintenances { get; set; }
		public StatusDto[] incidents { get; set; }
	}

	public class StatusDto
	{
		public string maintenance_status { get; set; }
		public string created_at { get; set; }
		public string archive_at { get; set; }
		public string incident_severity { get; set; }
		public UpdateDto[] updates { get; set; }
		public string[] platforms { get; set; }
		public ContentDto[] titles { get; set; }
		public int id { get; set; }
		public string updated_at { get; set; }
	}

	public class UpdateDto
	{
		public string[] publish_locations { get; set; }
		public string created_at { get; set; }
		public string author { get; set; }
		public int id { get; set; }
		public string updated_at { get; set; }
		public bool publish { get; set; }
		public ContentDto[] translations { get; set; }
	}

	public class ContentDto
	{
		public string locale { get; set; }
		public string content { get; set; }
	}


	public class SummonerDto
	{
		public string id { get; set; }
		public string accountId { get; set; }
		public string puuid { get; set; }
		public string name { get; set; }
		public int profileIconId { get; set; }
		public long revisionDate { get; set; }
		public long summonerLevel { get; set; }
	}

}
