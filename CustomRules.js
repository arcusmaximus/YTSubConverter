// Fiddler script for enabling subtitle preview on YouTube videos that have no subtitles yet.
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
import Newtonsoft.Json;
import Newtonsoft.Json.Linq; 

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
        
        var playerResponse: JObject = JObject.Parse(responseMatch.Groups[1].Value);
        if (playerResponse["captions"] != null)
            return;
        
        var videoId: String = playerResponse["videoDetails"]["videoId"];
        playerResponse["captions"] = CreateCaptionsObject(videoId);
        html = html.Substring(0, responseMatch.Groups[1].Index) +
               JsonConvert.SerializeObject(playerResponse) +
               html.Substring(responseMatch.Groups[1].Index + responseMatch.Groups[1].Length);
        
        oSession.utilSetResponseBody(html);
    }
      
    static function CreateCaptionsObject(videoId: String): JObject {
        var captionTracks = new JArray();
        captionTracks.Add(CreateCaptionTrackObject(videoId));
        
        var audioTracks = new JArray();
        audioTracks.Add(CreateAudioTrackObject());
        
        var renderer = new JObject();
        renderer["captionTracks"] = captionTracks;
        renderer["audioTracks"] = audioTracks;
        renderer["defaultAudioTrackIndex"] = 0;
        
        var captions = new JObject();
        captions["playerCaptionsTracklistRenderer"] = renderer;
        return captions;
    }
        
    static function CreateCaptionTrackObject(videoId: String): JObject {
        var captionTrack = new JObject();
        captionTrack["baseUrl"] = "https://www.youtube.com/api/timedtext?v=" + Uri.EscapeUriString(videoId);
        captionTrack["vssId"] = ".pr";
        captionTrack["languageCode"] = "pr";
        captionTrack["isTranslatable"] = true;
        
        var run = new JObject();
        run["text"] = "Preview";
        var runs = new JArray();
        runs.Add(run);
        var name = new JObject();
        name["runs"] = runs;
        captionTrack["name"] = name;
        return captionTrack;
    }
        
    static function CreateAudioTrackObject(): JObject {
        var audioTrack = new JObject();
        audioTrack["defaultCaptionTrackIndex"] = 0;
        audioTrack["hasDefaultTrack"] = true;
        
        var captionTrackIndices = new JArray();
        captionTrackIndices.Add(0);
        audioTrack["captionTrackIndices"] = captionTrackIndices;
        
        return audioTrack;
    }
}