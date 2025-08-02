using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nest;

public class IndexModel : PageModel
{
    [BindProperty]
    public string SelectedIndex { get; set; }

    [BindProperty]
    public string SearchTerm { get; set; }

    public List<string> SearchResults { get; set; }

    private readonly IElasticClient _elasticClient;

    public IndexModel()
    {
        // For production use, move to dependency injection
        var settings = new ConnectionSettings(new Uri("http://localhost:9200"));
        _elasticClient = new ElasticClient(settings);
    }

    public void OnGet()
    {
        SearchResults = null;
    }

    public void OnPost()
    {
        SearchResults = new List<string>();

        if (!string.IsNullOrEmpty(SearchTerm) && !string.IsNullOrEmpty(SelectedIndex))
        {
            var response = _elasticClient.Search<Dictionary<string, object>>(s => s
                .Index(SelectedIndex)
                .Query(q => q
                    .QueryString(qs => qs
                        .Query(SearchTerm)
                    )
                )
            );

            if (response.IsValid)
            {
                foreach (var hit in response.Hits)
                {
                    SearchResults.Add(hit.Source.ToString());
                }
            }
            else
            {
                SearchResults.Add("Error: " + response.ServerError?.Error?.Reason);
            }
        }
    }
}
