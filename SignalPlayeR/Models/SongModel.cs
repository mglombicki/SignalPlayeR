using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalPlayeR.Models
{
    public class SongModel
    {
        public string SongName { get; set; }
        public string SongID { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string AlbumID { get; set; }
        public double Duration { get; set; }
    }
}