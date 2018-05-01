<Query Kind="Program">
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	var baseUrl = @"http://www.crosswayknoxville.org";
	var sermonPageUrl = $@"{baseUrl}/site/audiodownloads.asp?sec_id=180013269";
	var startPage = 15;
	var endPage = 16;
	
	var allSermons = new List<Sermon>();
	
	for (var page = startPage; page <= endPage; page++)
	{
		var index = (page - 1) * 30;
		var address = $"{sermonPageUrl}";
		if (index > 0)
			address = $"{sermonPageUrl}&index={index}";
			
		var htmlDoc = GetPage(address);
		var sermons = htmlDoc.GetSermonNodes()
			.Select(node => new Sermon {
				Title = node.GetTitle(),
				Date = node.GetDate(),
				Speaker = node.GetSpeaker(),
				Text = node.GetScriptureReference(),
				Mp3Url = node.GetMp3Url(baseUrl)
			});

		sermons.Dump($"Page {page}: {address}");
		allSermons.AddRange(sermons);
	}
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

public class Sermon
{
	public string Title { get; set; }
	public string Speaker { get; set; }
	public DateTime? Date { get; set; }
	public string Text { get; set; }
	public string Mp3Url { get; set; }
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
