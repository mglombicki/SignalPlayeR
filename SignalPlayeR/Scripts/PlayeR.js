/// <reference path="jquery.signalR-2.0.3.js" />
$(function () {

    var songlist = new Object();
    var muted = false;
    var currentSong;
    var username;

    var muteSetting = JSON.parse(localStorage.getItem('mute'));
    if (muteSetting) {
        muted = muteSetting;
        if (muted) {
            $("#mute-button").text("Unmute");
        }
    }
    var usernameSetting = JSON.parse(localStorage.getItem('user'));
    if (usernameSetting) {
        username = usernameSetting;
    }

    /*
    * UI Listeners
    */

    $("#mute-button").click(function () {
        muted = !muted;
        localStorage.setItem("mute", JSON.stringify(muted));
        $(".player").first().remove();//Get rid of the old Grooveshark player
        if (muted) {
            $("#playlist").append('<div class="player"><object type="application/x-shockwave-flash" data="http://grooveshark.com/songWidget.swf" width="100%" height="40"><param name="wmode" value="window" /><param name="allowScriptAccess" value="always" /><param name="flashvars" value="hostname=grooveshark.com&songID=' + currentSong.SongID + '&p=0" /></object></div>');
            $(this).text("Unmute");
        }
        else {
            $("#playlist").append('<div class="player"><object type="application/x-shockwave-flash" data="http://grooveshark.com/songWidget.swf" width="100%" height="40"><param name="wmode" value="window" /><param name="allowScriptAccess" value="always" /><param name="flashvars" value="hostname=grooveshark.com&songID=' + currentSong.SongID + '&p=1" /></object></div>');
            $(this).text("Mute");
        }
    });

    $("#about-link").click(function () {
        $("#aboutModal").modal('show');
    });

    //Add the enter key-listener to the search field
    $("#search").keypress(function (event) {
        if (event.which == 13) {
            event.preventDefault();

            //Send out an API call to fetch the song data
            $.ajax({
                url: "/search/songs/" + encodeURIComponent($(this).val())
            }).then(function (data) {
                //if the data is good
                if (data.length > 0) {
                    hub.server.tryAddSong(data[0]);
                }
                else {
                    alert("No songs found");// TODO: Do this in a less ugly way
                }
            });
        }
    });

    var formatTime = function (seconds) {
        var min = parseInt(seconds / 60);
        var sec = zeroFill(parseInt(seconds % 60), 2);
        return min + ":" + sec;
    }

    var zeroFill = function (number, width) {
        width -= number.toString().length;
        if (width > 0) {
            return new Array(width + (/\./.test(number) ? 2 : 1)).join('0') + number;
        }
        return number + ""; // always return a string
    }


    /*
    * SignalR Connection Methods
    */
    var hub = $.connection.songHub;

    hub.client.playNewSong = function (song) {
        currentSong = song;
        $(".player").first().remove();//Get rid of the old Grooveshark player
        if (muted) {
            $("#playlist").append('<div class="player"><object type="application/x-shockwave-flash" data="http://grooveshark.com/songWidget.swf" width="100%" height="40"><param name="wmode" value="window" /><param name="allowScriptAccess" value="always" /><param name="flashvars" value="hostname=grooveshark.com&songID=' + song.SongID + '&p=0" /></object></div>');
        } else {
            $("#playlist").append('<div class="player"><object type="application/x-shockwave-flash" data="http://grooveshark.com/songWidget.swf" width="100%" height="40"><param name="wmode" value="window" /><param name="allowScriptAccess" value="always" /><param name="flashvars" value="hostname=grooveshark.com&songID=' + song.SongID + '&p=1" /></object></div>');
        }
        $("#songlist li").first().remove();//Remove the top song from the list
    };

    hub.client.addSong = function (songData) {
        var $li = $('<li class="list-group-item">');
        var $albumArt = $('<img class="album-art" />');
        $albumArt.attr("src", "http://images.gs-cdn.net/static/albums/40_" + songData.AlbumID + ".jpg");
        var additionalClass = "warning";
        if (songData.Duration > 0) {
            additionalClass = "";
        }
        var $duration = $('<span class="duration badge ' + additionalClass + '">Error!</span>');
        $duration.text(formatTime(songData.Duration));
        var $title = $('<div class="song"></div>');
        $title.text(songData.SongName);
        var $artist = $('<div class="artist"></div>');
        $artist.text(songData.ArtistName);

        $li.append($albumArt);
        $li.append($duration);
        $li.append($title);
        $li.append($artist);

        $("#songlist").append($li);
    };

    hub.client.updateSonglist = function (songlist, serversCurrentSong) {
        if (songlist && songlist.length > 0) {
            $("#songlist").html("");//clear whatever is there
            for (var i = 0; i < songlist.length; i++) {
                hub.client.addSong(songlist[i]);
            }
            if (serversCurrentSong) {
                currentSong = serversCurrentSong;
                $(".player").first().remove();//Get rid of the old Grooveshark player
                if (muted) {
                    $("#playlist").append('<div class="player"><object type="application/x-shockwave-flash" data="http://grooveshark.com/songWidget.swf" width="100%" height="40"><param name="wmode" value="window" /><param name="allowScriptAccess" value="always" /><param name="flashvars" value="hostname=grooveshark.com&songID=' + currentSong.SongID + '&p=0" /></object></div>');
                } else {
                    $("#playlist").append('<div class="player"><object type="application/x-shockwave-flash" data="http://grooveshark.com/songWidget.swf" width="100%" height="40"><param name="wmode" value="window" /><param name="allowScriptAccess" value="always" /><param name="flashvars" value="hostname=grooveshark.com&songID=' + currentSong.SongID + '&p=1" /></object></div>');
                }
            }
        }
        else {
            console.warn("songlist is empty!");
        }
    }

    $.connection.hub.logging = true;
    $.connection.hub.start().done(function () {
        //TODO User registration
        //$('#registerModal').modal({
        //    keyboard: false,
        //    backdrop: 'static'
        //});
        //$("#registerModal").keypress(function (event) {
        //    if (event.which == 13) {
        //        event.preventDefault();
        //        var name = $('#register-field').val();
        //        if (name) {
        //            hub.server.registerUser(name);
        //            $(this).modal('hide');
        //        }
        //    }
        //});
        //$('#register-button').click(function () {
        //    var name = $('#register-field').val();
        //    hub.server.registerUser(name);
        //})

        $("#skip-button").click(function () {
            hub.server.nextSong();
        })

        hub.server.updateSonglist();
    });

});