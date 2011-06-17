using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Twitterizer;

namespace BotTask
{
    class Program
    {
        static void Main(string[] args)
        {

           var arrivals = GetArrivals();

            foreach (var a in  arrivals)
            {
                var arrivaltime = DateTime.Parse(a.date + " " + a.scheduled_arrival_time);
                if ((arrivaltime < DateTime.Now) && arrivaltime >  DateTime.Now.AddMinutes(-10) )
                {
                    tweet(string.Format("Flight {0} from {1} has landed.", a.flight_number, a.from));

                }
            }


            var departures = GetDepartures();

            foreach (var d in departures)
            {
                var arrivaltime = DateTime.Parse(d.date + " " + d.scheduled_departure_time);
                if ((arrivaltime < DateTime.Now) && arrivaltime > DateTime.Now.AddMinutes(-10))
                {
                    tweet(string.Format("Flight {0} from {1} has taken off.", d.flight_number, d.destination));

                }
            }
        }
      

        public static IList<arrival> GetArrivals()
        {

            IList<arrival> arrivals;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.scraperwiki.com/api/1.0/datastore/sqlite?format=jsondict&name=robin_hood_airport_arrivals&query=select%20*%20from%20swdata%20");
                    httpWebRequest.ContentType = "text/json";
                    httpWebRequest.Method = "GET";


                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                    var responseText = streamReader.ReadToEnd();
                     arrivals = new JavaScriptSerializer()
                            .Deserialize<IList<arrival>>(responseText);
        
                    }

            return arrivals;
        }


        public static IList<departure> GetDepartures()
        {

            IList<departure> departures;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.scraperwiki.com/api/1.0/datastore/sqlite?format=jsondict&name=robin_hood_airport_departures&query=select%20*%20from%20swdata%0A");
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "GET";


            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                departures = new JavaScriptSerializer()
                       .Deserialize<IList<departure>>(responseText);
               
            }

            return departures;
        }

        public static void tweet(string tweet)
        {
          
            OAuthTokens tokens = new OAuthTokens();
            tokens.AccessToken = System.Configuration.ConfigurationManager.AppSettings["AccessToken"];
            tokens.AccessTokenSecret = System.Configuration.ConfigurationManager.AppSettings["AccessTokenSecret"];
            tokens.ConsumerKey = System.Configuration.ConfigurationManager.AppSettings["ConsumerKey"];
            tokens.ConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["ConsumerSecret"];

            TwitterResponse<TwitterStatus> tweetResponse = TwitterStatus.Update(tokens, tweet);
            if (tweetResponse.Result == RequestResult.Success)
            {
                // Tweet posted successfully!
            }
            
        }

    }
}

