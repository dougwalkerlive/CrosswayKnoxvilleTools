<Query Kind="Program">
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

public string BaseUrl { get; set; } = @"http://www.crosswayknoxville.org";
public string WorkingFolder { get; set; } = Path.GetDirectoryName(Util.CurrentQueryPath);
public string FileName { get; set; } = "sermon-data.json";

void Main()
{
	//var sermonData = ExtractBaseSermonData(1, 17);
	//SaveSermonData(sermonData);
	
	var sermonData = LoadSermonData();
	//sermonData.ForEach(s => ExtractSermonDetails(s));
	//sermonData.ForEach(s => CleanSpeakerNames(s));

	//SaveSermonData(sermonData);

	sermonData
		.Where(s => !String.IsNullOrEmpty(s.Speaker))
		.Select(s => s.Speaker)
		.Distinct()
		.OrderBy(s => s)
		.Dump("Speakers");
		
	sermonData
		.Where(s => !String.IsNullOrWhiteSpace(s.SeriesName))
		.Select(s => s.SeriesName.Trim())
		.Distinct()
		.OrderBy(s => s)
		.Dump("Series");
		
}

List<Sermon> ExtractBaseSermonData(int? startingPage = 1, int? throughPage = null)
{
	
	var sermonListUrl = $@"{BaseUrl}/site/audiodownloads.asp?sec_id=180013269";
	var sermonPageUrl = $@"{BaseUrl}/site/file.asp?sec_id=180013269&table=file_downloads&file_id=";
	var startPage = startingPage ?? 1;
	var endPage = throughPage ?? startPage;

	var allSermons = new List<Sermon>();

	for (var page = startPage; page <= endPage; page++)
	{
		var index = (page - 1) * 30;
		var address = $"{sermonListUrl}";
		if (index > 0)
			address = $"{sermonListUrl}&index={index}";

		var htmlDoc = GetPage(address);
		var sermons = htmlDoc.GetSermonNodes()
			.Select(node => new Sermon
			{
				SermonId = node.GetId(),
				Title = node.GetTitle(),
				Date = node.GetDate(),
				Speaker = node.GetSpeaker(),
				Text = node.GetScriptureReference(),
				Mp3Url = node.GetMp3Url(BaseUrl)
			});

		allSermons.AddRange(sermons);
	}
	return allSermons;
}

Sermon ExtractSermonDetails(Sermon sermon) 
{
	Console.Write($"Processing {sermon.Title}...");
	var page = GetPage($"{BaseUrl}{sermon.SermonPageUrl}");
	sermon.SeriesName = page.GetSermonDetailSeries();
	Console.WriteLine(sermon.SeriesName);
	
	return sermon;
}

Sermon CleanSpeakerNames(Sermon sermon)
{
	if (sermon.Speaker == "Greg Picke" 
		|| sermon.Speaker == "Greg PIckle"
		|| sermon.Speaker == "What We Teach")
		sermon.Speaker = "Greg Pickle";
		
	if (sermon.Speaker == "Brian Beemer and Doug Walker")
		sermon.Speaker = "Brian Beemer, Doug Walker";
		
	return sermon;
}

HtmlDocument GetPage(string url) 
{
	using (var webClient = new WebClient())
	{
		var html = webClient.DownloadString(url);
		var htmlDoc = new HtmlAgilityPack.HtmlDocument();
		htmlDoc.LoadHtml(html);
		return htmlDoc;
	}
}

private void SaveSermonData(IEnumerable<Sermon> sermons) 
{
	var json = JsonConvert.SerializeObject(sermons);

	var filename = Path.Combine(WorkingFolder, FileName);
	File.WriteAllText(filename, json);
}

private List<Sermon> LoadSermonData()
{
	var filename = Path.Combine(WorkingFolder, FileName);
	var json = File.ReadAllText(filename);
	
	return JsonConvert.DeserializeObject<List<Sermon>>(json);
}

public class Sermon
{
	public int SermonId { get; set; }
	public string Title { get; set; }
	public string Speaker { get; set; }
	public DateTime? Date { get; set; }
	public string Text { get; set; }
	public string Mp3Url { get; set; }
	public string Description { get; set; }
	public string SeriesName { get; set; }

	public string SermonPageUrl => $"/site/file.asp?sec_id=180013269&table=file_downloads&file_id={SermonId}";
}

public static class ExtensionMethods 
{
	// Replaces Html Non-breaking spaces (&nbsp;) with regular
	// spaces and trims the string.
	public static string TrimHtml(this string htmlText)
	{
		if (htmlText == null) return null;
		var cleanText = Regex.Replace(htmlText, "&nbsp;", " ", RegexOptions.IgnoreCase)?.Trim();
		return cleanText;
	}
	
	public static IEnumerable<HtmlNode> GetSermonNodes(this HtmlDocument htmlDoc)
	{
		return htmlDoc.DocumentNode.SelectNodes("//div")
			.Where(node => node.HasClass("file-item"));
	}
	
	public static string GetSermonDetailSeries(this HtmlDocument htmlDoc)
	{
		var leftNavNode = htmlDoc.GetElementbyId("file-left");
		var seriesLink = leftNavNode.Descendants("a")
			.Where(a => a.Attributes.AttributesWithName("href").FirstOrDefault()?.Value?.Contains("&dlcat") ?? false)
			.FirstOrDefault();
			
		var seriesName = seriesLink?.InnerText;
		if (seriesName == null) return null;
		
		var seriesRegex = new Regex(@"^\<\<\s+(?<seriesName>.*)Series$");
		var match = seriesRegex.Match(seriesName);
		if (match != null)
			seriesName = match.Groups["seriesName"].Value;
			
		return seriesName;
	}
	
	public static int GetId(this HtmlNode sermonNode) 
	{
		var id = sermonNode
			.Descendants("div")
			.FirstOrDefault()
			?.Attributes["id"]
			?.Value;
			
		return id == null 
			? 0
			: Int32.Parse(id);
	}
	
	// Given the sermon node in the html, find and extract the title
	// of the sermon
	public static string GetTitle(this HtmlNode sermonNode)
	{
		return sermonNode
			.Descendants("h4")
			.FirstOrDefault()
			?.InnerText
			?.TrimHtml();
	}
	
	// Given the sermon node in the html, find and extract the 
	// Date on which the sermon was given
	public static DateTime? GetDate(this HtmlNode sermonNode) 
	{
		var dateText = sermonNode
			.Descendants("span")
			.FirstOrDefault(span => span.HasClass("file_date"))
			?.InnerText
			?.TrimHtml();
		
		return dateText == null
			? null as DateTime?
			: DateTime.Parse(dateText);
		
	}
	
	public static string GetSpeaker(this HtmlNode sermonNode) 
	{
		return sermonNode
			.Descendants("span")
			.FirstOrDefault(span => span.HasClass("mp3_speaker"))
			?.InnerText
			?.TrimHtml();
	}
	
	public static string GetScriptureReference(this HtmlNode sermonNode) 
	{
		return sermonNode
			.Descendants("span")
			.FirstOrDefault(span => span.HasClass("mp3_text"))
			?.InnerText
			?.TrimHtml();
	}
	
	public static string GetMp3Url(this HtmlNode sermonNode, string baseUrl)
	{
		var parsedUrl = sermonNode
			.NextSibling	// Text node (that contains only whitespace)
			.NextSibling	// Div that contains the download stuff
			.Descendants("a")
			.FirstOrDefault(a => a.HasClass("dlLink"))
			.Attributes["href"]
			.Value;
		
		if (parsedUrl == null) return null;

		if (parsedUrl.StartsWith("/"))
			parsedUrl = $"{baseUrl}{parsedUrl}";
			
		return parsedUrl;
	}
}