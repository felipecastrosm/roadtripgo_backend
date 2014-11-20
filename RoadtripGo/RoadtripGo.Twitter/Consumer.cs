using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using Tweetinvi;
using Tweetinvi.Core.Events.EventArguments;

namespace RoadtripGo.Twitter
{
    public class Consumer
    {
		private Dictionary<string, List<string>> FuelTypeDictionary { get; set; }

		private Dictionary<string, List<string>> SearchTypeDictionary { get; set; }

		private Dictionary<string, List<string>> BrandsDictionary { get; set; }

		private string NodeApiAddress { get; set; }

        public Consumer()
        {
            TwitterCredentials.SetCredentials("2885315867-eIceSrertrpjG3tt3Q3KrQYthviHOi1s1tTWAOP", "EZkWNdvqXIuOqGHg7KD7TINjGm7yTC0FnfccC2OpjpPNN", "QDunwbZOBO8feuKitbDvrqzyT", "fzsFzCFTBQUQKLAE3NEUyP23GlBbr7v10DX7ab0A9IOhqTBpLO");

			this.Initialize();
        }

	    public void Initialize()
	    {
		    this.PopulateFuelDictionary();
			this.PopulateSearchDictionary();
		    this.PopulateBrandsDictionary();

		    this.NodeApiAddress = "http://192.168.0.100/";
	    }

        public void Start()
        {
            var userStream = Stream.CreateUserStream();
            userStream.TweetCreatedByAnyoneButMe += (s, a) => this.HandleTweet(a);

            Console.WriteLine("Twitter Consumer Starting!");
            userStream.StartStreamAsync();
            Console.WriteLine("Twitter Consumer Started!");
        }

	    private void PopulateFuelDictionary()
	    {
		    var gasolineAliasList = new List<string> {"gasolina", "gas", "gasoline"};
		    var alcoholAliasList = new List<string> {"alcool", "etanol", "ethanol", "alc"};
		    var naturalGasAliasList = new List<string> {"gnv", "natural gas", "gas natural"};
		    var dieselAliasList = new List<string> {"diesel"};

			this.FuelTypeDictionary = new Dictionary<string, List<string>> { { "gasoline", gasolineAliasList }, { "alcohol", alcoholAliasList }, { "gnv", naturalGasAliasList }, { "diesel", dieselAliasList } };
	    }

	    private void PopulateSearchDictionary()
	    {
		    var nearestAliasList = new List<string> {"perto", "proximo", "nearby", "adjacente"};
		    var cheapestAliasList = new List<string> {"barato", "preco", "preço"};
		    var fastestAliasList = new List<string> {"rapido", "rápido"};

			this.SearchTypeDictionary = new Dictionary<string, List<string>> {{"nearest", nearestAliasList}, {"cheapest", cheapestAliasList}, {"fastest", fastestAliasList}};
	    }

	    private void PopulateBrandsDictionary()
	    {
		    var shellAliasList = new List<string> {"shell"};
		    var essoAliasList = new List<string> {"esso", "cosan"};
		    var brAliasList = new List<string> {"br", "petrobras"};
		    var ipirangaAliasList = new List<string> {"ipiranga"};

			this.BrandsDictionary = new Dictionary<string, List<string>> {{"shell", shellAliasList}, {"esso", essoAliasList}, {"br", brAliasList}, {"ipiranga", ipirangaAliasList}};
	    }

        public void HandleTweet(TweetReceivedEventArgs tweetReceivedEventArgs)
        {
            if (tweetReceivedEventArgs.Tweet.Text.Contains("@roadtripgo"))
            {
				//Console.WriteLine("{0} posted {1}", tweetReceivedEventArgs.Tweet.Creator.Name, tweetReceivedEventArgs.Tweet.Text);
	            var searchParams = new SearchParams();

	            string tweetText = tweetReceivedEventArgs.Tweet.Text;

	            searchParams.Brands = this.SearchByAliasDictionary(tweetText, this.BrandsDictionary);
	            searchParams.FuelType = this.SearchByAliasDictionary(tweetText, this.FuelTypeDictionary);
	            searchParams.SearchType = this.SearchByAliasDictionary(tweetText, this.SearchTypeDictionary);

				LocationData locationData = this.ParseLocation(tweetText);

				//TODO: locationData might be null here - tweet back asking for proper url

	            searchParams.Latitude = locationData.Latitude;
	            searchParams.Longitude = locationData.Longitude;

	            var client = new HttpClient();

	            var response = client.PostAsJsonAsync(this.NodeApiAddress + "gas/get_gas_station", searchParams).Result;

				//response.Content
            }
        }

	    private LocationData ParseLocation(string originalText)
	    {
		    var regex = new Regex(@"(ftp|https?)://[^\s]+");

		    var urlPart = regex.Match(originalText).Value;

		    if (!String.IsNullOrWhiteSpace(urlPart))
		    {
			    var uri = new Uri(urlPart);

			    var query = HttpUtility.ParseQueryString(uri.Query);

			    var locationPart = query.Get("ll");

			    var locationPartValues = locationPart.Split(',');

			    var locationData = new LocationData {Latitude = locationPartValues[0], Longitude = locationPartValues[1]};

			    return locationData;
		    }

		    return null;
	    }

	    private List<string> SearchByAliasDictionary(string originalString,
		    Dictionary<string, List<string>> aliasDictionary)
	    {
			var resultList = new List<string>();

		    foreach (var entry in aliasDictionary)
		    {
			    if (entry.Value.Any(alias => originalString.ToLower().Contains("#" + alias.ToLower())))
			    {
				    resultList.Add(entry.Key);
			    }
		    }

		    return resultList;
	    }

        public void ParseTweet(string tweetText)
        {
            
        }
    }
}
