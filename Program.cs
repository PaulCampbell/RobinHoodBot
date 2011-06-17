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


            // publish arrivals
           var arrivals = GetArrivals();

            foreach (var a in  arrivals)
            {
                var arrivaltime = DateTime.Parse(a.date + " " + a.scheduled_arrival_time);
                if ((arrivaltime < DateTime.Now) && arrivaltime >  DateTime.Now.AddMinutes(-10) )
                {
                    Tweet(string.Format("Flight {0} from {1} has landed.", a.flight_number, a.from));

                }
            }


            //publish departures
            var departures = GetDepartures();

            foreach (var d in departures)
            {
                var arrivaltime = DateTime.Parse(d.date + " " + d.scheduled_departure_time);
                if ((arrivaltime < DateTime.Now) && arrivaltime > DateTime.Now.AddMinutes(-10))
                {
                    Tweet(string.Format("Flight {0} from {1} has taken off.", d.flight_number, d.destination));

                }
            }

            //respond to tweets
            OAuthTokens tokens = new OAuthTokens();
            tokens.AccessToken = System.Configuration.ConfigurationManager.AppSettings["AccessToken"];
            tokens.AccessTokenSecret = System.Configuration.ConfigurationManager.AppSettings["AccessTokenSecret"];
            tokens.ConsumerKey = System.Configuration.ConfigurationManager.AppSettings["ConsumerKey"];
            tokens.ConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["ConsumerSecret"];

            string query = "@robinhoodbot";

            SearchOptions options = new SearchOptions()
            {
                PageNumber = 1,
                NumberPerPage = 50
            };

            TwitterResponse<TwitterSearchResultCollection> searchResult = TwitterSearch.Search(query, options);

                foreach (var tweet in searchResult.ResponseObject)
                {
                
                    RespondToTweet(tweet);

                }

            
        }
      
        public static void RespondToTweet(TwitterSearchResult tweet)
        {
            var arrive_or_depart = "";
            if (tweet.Text.Contains("arrive"))
            {
                arrive_or_depart = "arrive";
            }
            else if(tweet.Text.Contains("leave") || tweet.Text.Contains("depart"))
            {
                arrive_or_depart = "depart";
            }
            else
            {
                // we're not sure - respond to both...
                arrive_or_depart = "either";
            }

            if (tweet.CreatedDate > DateTime.Now.AddMinutes(-10))
            {
                if (arrive_or_depart != "depart")
                {
                    foreach (var a in GetArrivals())
                    {
                        var tweetWords = tweet.Text.Split(" ".ToCharArray());
                        foreach (var w in tweetWords.Where(w => (w == a.flight_number) || (w == a.from)))
                        {
                            var arrivaltime = DateTime.Parse(a.date + " " + a.scheduled_arrival_time);
                            var bodyText = "will be arriving";
                            if (arrivaltime < DateTime.Now)
                            {
                                bodyText = "arrived";
                            }

                            Tweet(string.Format("@{4}: Flight {0} from {3} {2} at {1}.", a.flight_number,
                                                a.scheduled_arrival_time, bodyText, a.from, tweet.FromUserScreenName));
                        }
                    }
                }

                if (arrive_or_depart != "arrive")
                {
                    foreach (var d in GetDepartures())
                    {
                        var tweetWords = tweet.Text.Split(" ".ToCharArray());
                        foreach (var w in tweetWords.Where(w => (w == d.flight_number) || (w == d.destination)))
                        {
                            var arrivaltime = DateTime.Parse(d.date + " " + d.scheduled_departure_time);
                            var bodyText = "will be leaving";
                            if (arrivaltime < DateTime.Now)
                            {
                                bodyText = "left";
                            }

                            Tweet(string.Format("@{4}: Flight {0} to {3} {2} at {1}.", d.flight_number,
                                                d.scheduled_departure_time, bodyText, d.destination,
                                                tweet.FromUserScreenName));
                        }
                    }
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

        public static void Tweet(string tweet)
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

