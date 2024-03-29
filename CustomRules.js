﻿// Fiddler script for enabling subtitle preview on YouTube videos that have no subtitles yet.
// To use, simply place it in My Documents\Fiddler2\Scripts.
// Should it start to cause errors someday because YouTube changed something,
// you can just delete it again.

import System;
import System.Collections;
import System.Collections.Specialized
import System.Text;
import System.Text.RegularExpressions;
import System.Web;
import System.Windows.Forms;
import Fiddler;
import Fiddler.WebFormats;

class Handlers
{
    static function OnBeforeResponse(oSession: Session): void {
        if (!oSession.fullUrl.StartsWith("https://www.youtube.com/watch") || oSession.responseCode != 200)
            return;
        
        oSession.utilDecodeResponse();
        var html: String = Encoding.UTF8.GetString(oSession.responseBodyBytes);
        var responseMatch: Match = Regex.Match(html, "var ytInitialPlayerResponse = (.+?);var meta = ");
        if (!responseMatch.Success)
            return;
        
        var playerResponse: Hashtable = JSON.JsonDecode(responseMatch.Groups[1].Value).JSONObject;
        if (playerResponse["captions"] != null)
            return;
        
        var videoId: String = playerResponse["videoDetails"]["videoId"];
        playerResponse["captions"] = CreateCaptionsObject(videoId);
        html = html.Substring(0, responseMatch.Groups[1].Index) +
               JSON.JsonEncode(playerResponse) +
               html.Substring(responseMatch.Groups[1].Index + responseMatch.Groups[1].Length);
        
        oSession.utilSetResponseBody(html);
    }
      
    static function CreateCaptionsObject(videoId: String): Object {
        var captionTracks = new ArrayList();
        captionTracks.Add(CreateCaptionTrackObject(videoId));
        
        var audioTracks = new ArrayList();
        audioTracks.Add(CreateAudioTrackObject());
        
        var renderer = new Hashtable();
        renderer["captionTracks"] = captionTracks;
        renderer["audioTracks"] = audioTracks;
        renderer["defaultAudioTrackIndex"] = 0;
        
        var captions = new Hashtable();
        captions["playerCaptionsTracklistRenderer"] = renderer;
        return captions;
    }
        
    static function CreateCaptionTrackObject(videoId: String): Object {
        var captionTrack = new Hashtable();
        captionTrack["baseUrl"] = "https://www.youtube.com/api/timedtext?v=" + Uri.EscapeUriString(videoId);
        captionTrack["vssId"] = ".pr";
        captionTrack["languageCode"] = "pr";
        captionTrack["isTranslatable"] = true;
        
        var run = new Hashtable();
        run["text"] = "Preview";
        var runs = new ArrayList();
        runs.Add(run);
        var name = new Hashtable();
        name["runs"] = runs;
        captionTrack.Add("name", name);
        return captionTrack;
    }
        
    static function CreateAudioTrackObject(): Object {
        var audioTrack = new Hashtable();
        audioTrack.Add("defaultCaptionTrackIndex", 0);
        audioTrack.Add("hasDefaultTrack", true);
        
        var captionTrackIndices = new ArrayList();
        captionTrackIndices.Add(0);
        audioTrack.Add("captionTrackIndices", captionTrackIndices);
        
        return audioTrack;
    }
}