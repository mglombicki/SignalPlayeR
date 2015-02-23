using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SignalPlayeR.Models;
using RestSharp;
using System.Web.Script.Serialization;
using System.Timers;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace SignalPlayeR.Hubs
{
    public class SongHub : Hub
    {
        static ConcurrentDictionary<string, string> users = new ConcurrentDictionary<string, string>();
        static ConcurrentQueue<SongModel> songs = new ConcurrentQueue<SongModel>();
        static System.Timers.Timer songTimer = new System.Timers.Timer();
        static SongModel currentSong;
        static bool isPlaying;
        static string echoNestApiKey = ConfigurationManager.AppSettings["EchoNestApiKey"];

        public void RegisterUser(string name)
        {
            users.TryAdd(Context.ConnectionId, name);
        }

        public void tryAddSong(SongModel newSong)
        {
            //Find out how long the song is
            newSong.Duration = getSongLength(newSong);

            Clients.All.addSong(newSong);
            //If this was the only song in the queue, then start playing it
            if (!isPlaying)
            {
                isPlaying = true;
                currentSong = newSong;
                Clients.All.playNewSong(newSong);
                if (newSong.Duration < 0)
                {
                    songTimer.Interval = 200 * 1000; //Default song time of 3:20
                }
                else
                {
                    songTimer.Interval = newSong.Duration * 1000;
                }
                //HACK: make adding the event handler idempotent
                songTimer.Elapsed -= triggerNextSong;
                songTimer.Elapsed += triggerNextSong;
                songTimer.Enabled = true;
                songTimer.AutoReset = false;
            }
            else
            {
                songs.Enqueue(newSong);
            }
        }

        private void triggerNextSong(object source, ElapsedEventArgs e)
        {
            nextSong();
        }

        public void nextSong()
        {
            if (songs.Count >= 1)
            {
                SongModel upcomingSong;
                songs.TryDequeue(out upcomingSong);
                currentSong = upcomingSong;
                Clients.All.playNewSong(upcomingSong);
                if (upcomingSong.Duration < 0)
                {
                    songTimer.Interval = 260 * 1000; //Default song time of ~4 minutes
                }
                else
                {
                    songTimer.Interval = upcomingSong.Duration * 1000;
                }
                songTimer.Enabled = true;
                songTimer.AutoReset = false;
            }
            else
            {
                isPlaying = false;
                currentSong = null;
            }

        }

        public void UpdateSonglist()
        {
            Clients.Caller.updateSonglist(songs, currentSong);
        }

        public override Task OnConnected()
        {
            //UpdateSonglist();
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            var leavingID = Context.ConnectionId;
            string removedName;
            users.TryRemove(leavingID, out removedName);
            return base.OnDisconnected();
        }

        /// <summary>
        /// Makes an API call to find out how long a song is in seconds
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        private double getSongLength(SongModel song)
        {
            var requestPath = "api/v4/song/search?format=json&results=1&bucket=audio_summary&title={title}&artist={artist}&api_key=" + echoNestApiKey;
            var client = new RestClient("http://developer.echonest.com");
            var request = new RestRequest(requestPath, Method.GET);
            request.AddParameter("title", trimSearchString(song.SongName), ParameterType.UrlSegment);
            request.AddParameter("artist", trimSearchString(song.ArtistName), ParameterType.UrlSegment);

            // execute the request
            IRestResponse response = client.Execute(request);
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Content);
            var resp = data.response;
            JArray songs = data.response.songs;
            if (songs.Count > 0)
            {
                return data.response.songs[0].audio_summary.duration;
            }
            else
            {
                return -1;
            }
        }

        private string trimSearchString(string searchString)
        {
            //return searchString.Split('(')[0].Split('-')[0].Split('/')[0];
            return searchString;
        }
    }
}