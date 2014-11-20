using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadtripGo.Twitter
{
	public class SearchParams
	{
		public List<string> FuelType { get; set; }

		public List<string> SearchType { get; set; }

		public List<string> Brands { get; set; }

		public string Latitude { get; set; }

		public string Longitude { get; set; }
	}
}
