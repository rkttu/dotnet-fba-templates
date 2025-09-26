#:property PublishAot=False

#:package IKVM.Maven.Sdk@1.9.4

// IKVM alias
using JJsoup = org.jsoup.Jsoup;
using JDocument = org.jsoup.nodes.Document;
using JElements = org.jsoup.select.Elements;
using JElement = org.jsoup.nodes.Element;

var url = "https://www.cnn.com/";

try
{
    using var http = new HttpClient(new HttpClientHandler
    {
        AllowAutoRedirect = true
    });
    http.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 " +
        "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

    var html = await http.GetStringAsync(url);
    
    JDocument doc = JJsoup.parse(html, url);

    var sel = "body a[href]";
    var results = new List<(string title, string link)>();

    JElements els = doc.select(sel);
    for (int i = 0; i < els.size(); i++)
    {
        var a = (JElement)els.get(i);
        var title = a.text()?.Trim();
        var link = a.absUrl("href"); // Relative to Absolute path
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
            continue;

        // Remove duplicates
        if (!results.Exists(t => t.link == link))
            results.Add((title, link));
    }

    // Output
    Console.WriteLine("# Sample Links from {0}", url);
    foreach (var (title, link) in results.GetRange(0, Math.Min(20, results.Count)))
        Console.WriteLine($"- {title} :: {link}");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    return 1;
}
