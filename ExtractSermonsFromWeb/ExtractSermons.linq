<Query Kind="Program">
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	var web = new WebClient();
	var baseAddress = @"http://www.crosswayknoxville.org/site/audiodownloads.asp?sec_id=180013269";

	
	for (var page = 1; page <=1; page++)
	{
		var index = (page - 1) * 30;
		var address = $"{baseAddress}";
		if (index > 0)
			address = $"{baseAddress}&index={index}";
			
		var html = web.DownloadString(address);
		var htmlDoc = new HtmlAgilityPack.HtmlDocument();
		htmlDoc.LoadHtml(html);
		htmlDoc.DocumentNode.SelectNodes("//div").Count.Dump("Number of Divs");
		var sermons = htmlDoc.DocumentNode.SelectNodes("//div")
			.Where(node => node.HasClass("file-item"))
			.Select(node => new {
				Title = node.GetTitle(),
				Date = node.GetDate(),
				Speaker = node.GetSpeaker(),
				Text = node.GetScriptureReference(),
				Mp3Url = node.NextSibling.Descendants("a").FirstOrDefault(a => a.HasClass("dlLink color9-font btn"))?.Attributes.FirstOrDefault(att => att.Name == "href")?.Value?.TrimHtml()
			});
		
		sermons.Dump();		
	} 
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
}
