#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at SourceForge at http://sourceforge.net/projects/subtext
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Data;
using System.Globalization;
using Subtext.Extensibility;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;
using Subtext.Framework.Logging;

//Need to remove Global.X calls ...just seems unclean
//Maybe create a another class formatter ...Format.Entry(ref Entry entry) 
//or, Instead of Globals.PostUrl(int id) --> Globals.PostUrl(ref Entry entry)
//...

namespace Subtext.Framework.Data
{
	/// <summary>
	/// Contains helper methods for getting blog entries from the database 
	/// into objects such as <see cref="EntryDayCollection"/> and <see cref="EntryCollection"/>.
	/// </summary>
	public sealed class DataHelper
	{
		private DataHelper() {}

		#region Statisitics

		public static ViewStat LoadSingleViewStat(IDataReader reader)
		{
			ViewStat vStat = new ViewStat();


			if (reader["Title"] != DBNull.Value)
			{
				vStat.PageTitle = (string) reader["Title"];
			}

			if (reader["Count"] != DBNull.Value)
			{
				vStat.ViewCount = (int) reader["Count"];
			}

			if (reader["Day"] != DBNull.Value)
			{
				vStat.ViewDate = (DateTime) reader["Day"];
			}

			if (reader["PageType"] != DBNull.Value)
			{
				vStat.PageType = (PageType)((byte)reader["PageType"]);
			}

            return vStat;
		}

		public static Referrer LoadSingleReferrer(IDataReader reader)
		{
			Referrer refer = new Referrer();


			if (reader["URL"] != DBNull.Value)
			{
				refer.ReferrerURL = (string) reader["URL"];
			}

			if (reader["Title"] != DBNull.Value)
			{
				refer.PostTitle = (string) reader["Title"];
			}

			if (reader["EntryID"] != DBNull.Value)
			{
				refer.EntryID = (int) reader["EntryID"];
			}

			if (reader["LastUpdated"] != DBNull.Value)
			{
				refer.LastReferDate = (DateTime) reader["LastUpdated"];
			}

			if (reader["Count"] != DBNull.Value)
			{
				refer.Count = (int) reader["Count"];
			}

			return refer;
		}

		#endregion

		#region EntryDayCollection

		private static bool IsNewDay(DateTime dtCurrent, DateTime dtDay)
		{
			return !(dtCurrent.DayOfYear == dtDay.DayOfYear && dtCurrent.Year == dtDay.Year);
		}

		public static EntryDayCollection LoadEntryDayCollection(IDataReader reader)
		{
			DateTime dt = new DateTime(1900,1,1);
			EntryDayCollection edc = new EntryDayCollection();
			EntryDay day = null;

			while(reader.Read())
			{
				if(IsNewDay(dt,(DateTime)reader["DateAdded"]))
				{
					dt = (DateTime)reader["DateAdded"];
					day = new EntryDay(dt);
					edc.Add(day);
				}
				day.Add(DataHelper.LoadSingleEntry(reader));
			}
			return edc;

		}


		#endregion

		#region EntryCollection

		public static EntryCollection LoadEntryCollection(IDataReader reader)
		{
			EntryCollection ec = new EntryCollection();
			while(reader.Read())
			{
				ec.Add(LoadSingleEntry(reader));
			}
			return ec;	
		}

		#endregion

		#region Single Entry
		//Crappy. Need to clean up all of the entry references
		public static EntryStatsView LoadSingleEntryStatsView(IDataReader reader)
		{
			EntryStatsView entry = new EntryStatsView();

			entry.PostType = ((PostType)(int)reader["PostType"]);

			if(reader["WebCount"] != DBNull.Value)
			{
				entry.WebCount = (int)reader["WebCount"];	
			}

			if(reader["AggCount"] != DBNull.Value)
			{
				entry.AggCount = (int)reader["AggCount"];	
			}

			if(reader["WebLastUpdated"] != DBNull.Value)
			{
				entry.WebLastUpdated = (DateTime)reader["WebLastUpdated"];	
			}
			
			if(reader["AggLastUpdated"] != DBNull.Value)
			{
				entry.AggLastUpdated = (DateTime)reader["AggLastUpdated"];	
			}

			if(reader["Author"] != DBNull.Value)
			{
				entry.Author = (string)reader["Author"];
			}
			if(reader["Email"] != DBNull.Value)
			{
				entry.Email = (string)reader["Email"];
			}
			entry.DateCreated = (DateTime)reader["DateAdded"];
			
			if(reader["DateUpdated"] != DBNull.Value)
			{
				entry.DateUpdated = (DateTime)reader["DateUpdated"];
			}

			entry.EntryID = (int)reader["ID"];

			if(reader["TitleUrl"] != DBNull.Value)
			{
				entry.TitleUrl = (string)reader["TitleUrl"];
			}
			
			if(reader["SourceName"] != DBNull.Value)
			{
				entry.SourceName = (string)reader["SourceName"];
			}
			if(reader["SourceUrl"] != DBNull.Value)
			{
				entry.SourceUrl = (string)reader["SourceUrl"];
			}

			if(reader["Description"] != DBNull.Value)
			{
				entry.Description = (string)reader["Description"];
			}

			if(reader["EntryName"] != DBNull.Value)
			{
				entry.EntryName = (string)reader["EntryName"];
			}

			if(reader["FeedBackCount"] != DBNull.Value)
			{
				entry.FeedBackCount = (int)reader["FeedBackCount"];
			}

			if(reader["Text"] != DBNull.Value)
			{
				entry.Body = (string)reader["Text"];
			}

			if(reader["Title"] != DBNull.Value)
			{
				entry.Title =(string)reader["Title"];
			}

			if(reader["PostConfig"] != DBNull.Value)
			{
				entry.PostConfig = (PostConfig)((int)reader["PostConfig"]);
			}

			if(reader["ParentID"] != DBNull.Value)
			{
				entry.ParentID = (int)reader["ParentID"];
			}

			SetUrlPattern(entry);

			return entry;
		}


		public static Entry LoadSingleEntry(IDataReader reader)
		{
			return LoadSingleEntry(reader, true);
		}

		public static Entry LoadSingleEntry(IDataReader reader, bool buildLinks)
		{
			Entry entry = new Entry((PostType)(int)reader["PostType"]);
			LoadSingleEntry(reader, entry, buildLinks);
			return entry;
		}

		public static void LoadSingleEntry(IDataReader reader, Entry entry)
		{
			LoadSingleEntry(reader, entry, true);
		}

		private static void LoadSingleEntry(IDataReader reader, Entry entry, bool buildLinks)
		{
			if(reader["Author"] != DBNull.Value)
			{
				entry.Author = (string)reader["Author"];
			}

			if(reader["Email"] != DBNull.Value)
			{
				entry.Email = (string)reader["Email"];
			}

			entry.DateCreated = (DateTime)reader["DateAdded"];
			if(reader["DateUpdated"] != DBNull.Value)
			{
				entry.DateUpdated = (DateTime)reader["DateUpdated"];
			}
	
			entry.EntryID = (int)reader["ID"];
	
			if(reader["TitleUrl"] != DBNull.Value)
			{
				entry.TitleUrl = (string)reader["TitleUrl"];
			}
	
			if(reader["SourceName"] != DBNull.Value)
			{
				entry.SourceName = (string)reader["SourceName"];
			}

			if(reader["SourceUrl"] != DBNull.Value)
			{
				entry.SourceUrl = (string)reader["SourceUrl"];
			}
	
			if(reader["Description"] != DBNull.Value)
			{
				entry.Description = (string)reader["Description"];
			}
	
			if(reader["EntryName"] != DBNull.Value)
			{
				entry.EntryName = (string)reader["EntryName"];
			}
	
			if(reader["FeedBackCount"] != DBNull.Value)
			{
				entry.FeedBackCount = (int)reader["FeedBackCount"];
			}

			if(reader["Text"] != DBNull.Value)
			{
				entry.Body = (string)reader["Text"];
			}
	
			if(reader["Title"] != DBNull.Value)
			{
				entry.Title =(string)reader["Title"];
			}
	
			if(reader["PostConfig"] != DBNull.Value)
			{
				entry.PostConfig = (PostConfig)((int)reader["PostConfig"]);
			}

			if(reader["ContentChecksumHash"] != DBNull.Value)
			{
				entry.ContentChecksumHash = (string)reader["ContentChecksumHash"];
			}
	
			if(reader["ParentID"] != DBNull.Value)
			{
				entry.ParentID = (int)reader["ParentID"];
			}
	
			if(buildLinks)
			{
				SetUrlPattern(entry);
			}
		}

		private static void SetUrlPattern(Entry entry)
		{
			switch(entry.PostType)
			{
				case PostType.BlogPost:
					entry.Link = Config.CurrentBlog.UrlFormats.EntryUrl(entry);
					break;

				case PostType.Story:
					entry.Link = Config.CurrentBlog.UrlFormats.ArticleUrl(entry);
					break;

				case PostType.Comment:
				case PostType.PingTrack:
					entry.Link = Config.CurrentBlog.UrlFormats.CommentUrl(entry);
					break;
			}
		}



		public static Entry LoadSingleEntry(DataRow dr)
		{
			Entry entry = new Entry((PostType)(int)dr["PostType"]);

			if(dr["Author"] != DBNull.Value)
			{
				entry.Author = (string)dr["Author"];
			}
			if(dr["Email"] != DBNull.Value)
			{
				entry.Email = (string)dr["Email"];
			}

			// Not null.
			entry.DateCreated = (DateTime)dr["DateAdded"];
			
			if(dr["DateUpdated"] != DBNull.Value)
			{
				entry.DateUpdated = (DateTime)dr["DateUpdated"];
			}
			entry.EntryID = (int)dr["ID"];

			if(dr["TitleUrl"] != DBNull.Value)
			{
				entry.TitleUrl = (string)dr["TitleUrl"];
			}
			
			if(dr["SourceName"] != DBNull.Value)
			{
				entry.SourceName = (string)dr["SourceName"];
			}
			if(dr["SourceUrl"] != DBNull.Value)
			{
				entry.SourceUrl = (string)dr["SourceUrl"];
			}

			if(dr["Description"] != DBNull.Value)
			{
				entry.Description = (string)dr["Description"];
			}

			if(dr["EntryName"] != DBNull.Value)
			{
				entry.EntryName = (string)dr["EntryName"];
			}

			if(dr["FeedBackCount"] != DBNull.Value)
			{
				entry.FeedBackCount = (int)dr["FeedBackCount"];
			}

			if(dr["Text"] != DBNull.Value)
			{
				entry.Body = (string)dr["Text"];
			}

			// Title cannot be null.
			entry.Title =(string)dr["Title"];

			if(dr["PostConfig"] != DBNull.Value)
			{
				entry.PostConfig = (PostConfig)((int)dr["PostConfig"]);
			}

			if(dr["ParentID"] != DBNull.Value)
			{
				entry.ParentID = (int)dr["ParentID"];
			}

			SetUrlPattern(entry);
			return entry;
		}

		public static void LoadSingleEntry(ref Entry entry, DataRow dr)
		{
			entry.PostType = ((PostType)(int)dr["PostType"]);

			if(dr["Author"] != DBNull.Value)
			{
				entry.Author = (string)dr["Author"];
			}
			if(dr["Email"] != DBNull.Value)
			{
				entry.Email = (string)dr["Email"];
			}
			
			entry.DateCreated = (DateTime)dr["DateAdded"];
			if(dr["DateUpdated"] != DBNull.Value)
			{
				entry.DateUpdated = (DateTime)dr["DateUpdated"];
			}
			entry.EntryID = (int)dr["ID"];

			if(dr["TitleUrl"] != DBNull.Value)
			{
				entry.TitleUrl = (string)dr["TitleUrl"];
			}
			
			if(dr["SourceName"] != DBNull.Value)
			{
				entry.SourceName = (string)dr["SourceName"];
			}
			if(dr["SourceUrl"] != DBNull.Value)
			{
				entry.SourceUrl = (string)dr["SourceUrl"];
			}

			if(dr["Description"] != DBNull.Value)
			{
				entry.Description = (string)dr["Description"];
			}

			if(dr["EntryName"] != DBNull.Value)
			{
				entry.EntryName = (string)dr["EntryName"];
			}

			if(dr["FeedBackCount"] != DBNull.Value)
			{
				entry.FeedBackCount = (int)dr["FeedBackCount"];
			}

			if(dr["Text"] != DBNull.Value)
			{
				entry.Body = (string)dr["Text"];
			}

			// Title cannot be null.
			entry.Title = (string)dr["Title"];
			
			if(dr["PostConfig"] != DBNull.Value)
			{
				entry.PostConfig = (PostConfig)((int)dr["PostConfig"]);
			}

			if(dr["ParentID"] != DBNull.Value)
			{
				entry.ParentID = (int)dr["ParentID"];
			}

			if(dr["DateSyndicated"] != DBNull.Value)
			{
				entry.DateSyndicated = (DateTime)dr["DateSyndicated"];
			}

			SetUrlPattern(entry);
		}		

		/// <summary>
		/// Returns a single CategoryEntry from a DataReader. Expects the data reader to have
		/// two sets of results. Should only be used to load 1 ENTRY
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static CategoryEntry LoadSingleCategoryEntry(IDataReader reader)
		{
			CategoryEntry entry = new CategoryEntry();
			LoadSingleEntry(reader, entry);

			reader.NextResult();

			System.Collections.ArrayList al = new System.Collections.ArrayList();
			while(reader.Read())
			{
				al.Add((string)reader["Title"]);
			}

			if(al.Count > 0)
			{
				entry.Categories = (string[])al.ToArray(typeof(string));
			}

			return entry;
		}


		public static CategoryEntry LoadSingleCategoryEntry(DataRow dr)
		{
			Entry entry = new CategoryEntry();
			LoadSingleEntry(ref entry, dr);

			DataRow[] child = dr.GetChildRows("cats");
			if(child != null && child.Length > 0)
			{
				int count = child.Length;
				string[] cats = new string[count];
				for(int i=0;i<count;i++)
				{
					cats[i] = (string)child[i]["Title"];
				}
				((CategoryEntry)entry).Categories = cats;	
			}

			return (CategoryEntry)entry;
		}

		public static int GetMaxItems(IDataReader reader)
		{
			reader.Read();
			return (int)reader["TotalRecords"];
		}

		#endregion

		#region Categories

		public static LinkCategory LoadSingleLinkCategory(IDataReader reader)
		{
			LinkCategory lc = new LinkCategory((int)reader["CategoryID"], (string)reader["Title"]);
			lc.IsActive = (bool)reader["Active"];
			if(reader["CategoryType"] != DBNull.Value)
			{
				lc.CategoryType = (CategoryType)((byte)reader["CategoryType"]);
			}
			if(reader["Description"] != DBNull.Value)
			{
				lc.Description = (string)reader["Description"];
			}
			return lc;
		}

		public static LinkCategory LoadSingleLinkCategory(DataRow dr)
		{
			LinkCategory lc = new LinkCategory((int)dr["CategoryID"], (string)dr["Title"]);
			
			// Active cannot be null.
			lc.IsActive = (bool)dr["Active"];

			if(dr["CategoryType"] != DBNull.Value)
			{
				lc.CategoryType = (CategoryType)((byte)dr["CategoryType"]);
			}
			if(dr["Description"] != DBNull.Value)
			{
				lc.Description = (string)dr["Description"];
			}
			return lc;
		}

		#endregion

		#region Links

		public static Link LoadSingleLink(IDataReader reader)
		{
			Link link = new Link();
			// Active cannot be null
			link.IsActive = (bool)reader["Active"];

			if(reader["NewWindow"] != DBNull.Value)
			{
				link.NewWindow = (bool)reader["NewWindow"];
			}

			// LinkID cannot be null
			link.LinkID = (int)reader["LinkID"];
			
			if(reader["Rss"] != DBNull.Value)
			{
				link.Rss = (string)reader["Rss"];
			}
			
			if(reader["Url"] != DBNull.Value)
			{
				link.Url = (string)reader["Url"];
			}
			
			if(reader["Title"] != DBNull.Value)
			{
				link.Title = (string)reader["Title"];
			}

			if(reader["CategoryID"] != DBNull.Value)
			{
				link.CategoryID = (int)reader["CategoryID"];
			}
			
			if(reader["PostID"] != DBNull.Value)
			{
				link.PostID = (int)reader["PostID"];
			}
			return link;
		}

		public static Link LoadSingleLink(DataRow dr)
		{
			Link link = new Link();
			// Active cannot be null
			link.IsActive = (bool)dr["Active"];
			
			if(dr["NewWindow"] != DBNull.Value)
			{
				link.NewWindow = (bool)dr["NewWindow"];
			}

			//LinkID cannot be null.
			link.LinkID = (int)dr["LinkID"];
			
			if(dr["Rss"] != DBNull.Value)
			{
				link.Rss = (string)dr["Rss"];
			}
			
			if(dr["Url"] != DBNull.Value)
			{
				link.Url = (string)dr["Url"];
			}
			
			if(dr["Title"] != DBNull.Value)
			{
				link.Title = (string)dr["Title"];
			}
			
			if(dr["CategoryID"] != DBNull.Value)
			{
				link.CategoryID = (int)dr["CategoryID"];
			}
			
			if(dr["PostID"] != DBNull.Value)
			{
				link.PostID = (int)dr["PostID"];
			}
			return link;
		}

		#endregion

		#region Config

		public static BlogInfo LoadConfigData(IDataReader reader)
		{
			BlogInfo info = new BlogInfo();
			info.Author = (string)reader["Author"];
			info.BlogId = (int)reader["BlogId"];
			info.Email = (string)reader["Email"];
			info.Password = (string)reader["Password"];

			info.SubTitle = (string)reader["SubTitle"];
			info.Title = (string)reader["Title"];
			info.UserName = (string)reader["UserName"];
			info.TimeZone = (int)reader["TimeZone"];
			info.ItemCount = (int)reader["ItemCount"];
			info.Language = (string)reader["Language"];
			

			info.PostCount = (int)reader["PostCount"];
			info.CommentCount = (int)reader["CommentCount"];
			info.StoryCount = (int)reader["StoryCount"];
			info.PingTrackCount = (int)reader["PingTrackCount"];

			if(reader["News"] != DBNull.Value)
			{
				info.News = (string)reader["News"];
			}

			if(reader["LastUpdated"] != DBNull.Value)
			{
				info.LastUpdated = (DateTime)reader["LastUpdated"];
			}
			else
			{
				info.LastUpdated = new DateTime(2003,1,1);
			}
			info.Host = (string)reader["Host"];
			// The Subfolder property is stored in the Application column. 
			// This is a result of the legacy schema.
			info.Subfolder = (string)reader["Application"];

			info.Flag = (ConfigurationFlag)((int)reader["Flag"]);

			info.Skin = new SkinConfig();
			info.Skin.SkinName = (string)reader["Skin"];

			if(reader["SkinCssFile"] != DBNull.Value)
			{
				info.Skin.SkinCssFile = (string)reader["SkinCssFile"];
			}
		
			if(reader["SecondaryCss"] != DBNull.Value)
			{
				info.Skin.SkinCssText = (string)reader["SecondaryCss"];
			}

			if(reader["LicenseUrl"] != DBNull.Value)
			{
				info.LicenseUrl = (string)reader["LicenseUrl"];
			}

			if(reader["DaysTillCommentsClose"] != DBNull.Value)
			{
				info.DaysTillCommentsClose = (int)reader["DaysTillCommentsClose"];
			}
			else
			{
				info.DaysTillCommentsClose = int.MaxValue;
			}

			if(reader["CommentDelayInMinutes"] != DBNull.Value)
			{
				info.CommentDelayInMinutes = (int)reader["CommentDelayInMinutes"];
			}
			else
			{
				info.CommentDelayInMinutes = NullValue.NullInt32;
			}

			if(reader["NumberOfRecentComments"] != DBNull.Value)
			{
				info.NumberOfRecentComments = (int)reader["NumberOfRecentComments"];
			}
			else
			{
				info.NumberOfRecentComments = NullValue.NullInt32;
			}

			if(reader["RecentCommentsLength"] != DBNull.Value)
			{
				info.RecentCommentsLength = (int)reader["RecentCommentsLength"];
			}
			else
			{
				info.RecentCommentsLength = NullValue.NullInt32;
			}
			

			return info;
		}

		#endregion

		#region Archive

		public static ArchiveCountCollection LoadArchiveCount(IDataReader reader)
		{
			const string dateformat = "{0:00}/{1:00}/{2:0000}";
			string dt = null; //
			ArchiveCount ac =null;// new ArchiveCount();
			ArchiveCountCollection acc = new ArchiveCountCollection();
			while(reader.Read())
			{
				ac = new ArchiveCount();
				dt = string.Format(CultureInfo.InvariantCulture, dateformat, (int)reader["Month"],(int)reader["Day"],(int)reader["Year"]);
				// FIX: BUG SF1423271 Archives Links
				ac.Date = DateTime.ParseExact(dt,"MM/dd/yyyy",CultureInfo.InvariantCulture);

				ac.Count = (int)reader["Count"];
				acc.Add(ac);
			}
			return acc;
	
		}

		//Needs to be handled else where!
		public static Link LoadArchiveLink(IDataReader reader)
		{
			Link link = new Link();
			int count = (int)reader["Count"];
			DateTime dt = new DateTime((int)reader["Year"],(int)reader["Month"],1);
			link.NewWindow = false;
			link.Title = dt.ToString("y", CultureInfo.InvariantCulture) + " (" + count.ToString(CultureInfo.InvariantCulture) + ")";
			//link.Url = Globals.ArchiveUrl(dt,"MMyyyy");
			link.NewWindow = false;
			link.IsActive = true;
			return link;
		}

		#endregion

		#region Image

		public static Image LoadSingleImage(IDataReader reader)
		{
			Image _image = new Image();
			_image.CategoryID = (int)reader["CategoryID"];
			_image.File = (string)reader["File"];
			_image.Height = (int)reader["Height"];
			_image.Width = (int)reader["Width"];
			_image.ImageID = (int)reader["ImageID"];
			_image.IsActive = (bool)reader["Active"];
			_image.Title = (string)reader["Title"];
			return _image;
		}

		#endregion

		#region Keywords

		public static KeyWord LoadSingleKeyWord(IDataReader reader)
		{
			KeyWord kw = new KeyWord();
			kw.KeyWordID = (int)reader["KeyWordID"];
			kw.BlogId = (int)reader["BlogId"];
			kw.OpenInNewWindow = (bool)reader["OpenInNewWindow"];
			kw.ReplaceFirstTimeOnly = (bool)reader["ReplaceFirstTimeOnly"];
			kw.CaseSensitive = (bool)reader["CaseSensitive"];
			kw.Text = (string)reader["Text"];
			if(reader["Title"] != DBNull.Value)
			{
				kw.Title = SqlHelper.CheckNullString(reader["Title"]);
			}
			kw.Url = (string)reader["Url"];
			kw.Word = (string)reader["Word"];
			return kw;
		}



		#endregion

		#region Host
		/// <summary>
		/// Loads the host from the data reader.
		/// </summary>
		/// <param name="reader">Reader.</param>
		/// <returns></returns>
		public static void LoadHost(IDataReader reader, HostInfo info)
		{
			info.HostUserName = (string)reader["HostUserName"];
			info.Password = (string)reader["Password"];
			info.Salt = (string)reader["Salt"];
			info.DateCreated = (DateTime)reader["DateCreated"];
		}
		#endregion

		#region Log Entries
		/// <summary>
		/// Loads the single log entry.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <returns></returns>
		public static LogEntry LoadSingleLogEntry(IDataReader reader)
		{
			LogEntry entry = new LogEntry();
			entry.Id = ReadInt(reader, "Id");
			entry.BlogId = ReadInt(reader, "BlogId");
			entry.Date = ReadDate(reader, "Date");
			entry.Thread = ReadString(reader, "Thread");
			entry.Level = ReadString(reader, "Level");
			entry.Context = ReadString(reader, "Context");
			entry.Logger = ReadString(reader, "Logger");
			entry.Message = ReadString(reader, "Message");
			entry.Exception = ReadString(reader, "Exception");
			return entry;
		}
		#endregion

		/// <summary>
		/// Reads the int from the data reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public static int ReadInt(IDataReader reader, string columnName)
		{
			if(reader[columnName] != DBNull.Value)
				return (int)reader[columnName];
			else
				return NullValue.NullInt32;
		}

		/// <summary>
		/// Reads the string.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="columnName">Name of the coumn.</param>
		/// <returns></returns>
		public static string ReadString(IDataReader reader, string columnName)
		{
			if(reader[columnName] != DBNull.Value)
				return (string)reader[columnName];
			else
				return null;
		}

		/// <summary>
		/// Reads the date.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public static DateTime ReadDate(IDataReader reader, string columnName)
		{
			if(reader[columnName] != DBNull.Value)
				return (DateTime)reader[columnName];
			else
				return NullValue.NullDateTime;
		}
	}

	/// <summary>
	/// Sort direction.
	/// </summary>
	public enum SortDirection
	{
		None = 0,
		/// <summary>Sort ascending</summary>
		Ascending,
		/// <summary>Sort descending</summary>
		Descending
	}
}