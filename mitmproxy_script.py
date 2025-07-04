# READ THESES INSTRUCTIONS!!!!!!!!!!

# Edit "REPLACE ME" in the "subtitle_location" line to be the location of your subtitles.
# Keep the quotes and the starting R.

# It should look something like this (without the hashtag at the start):
# subtitle_location = r"C:\Users\Administrator\Documents\my cool subtitles.ytt"

subtitle_location = r"REPLACE ME"


# Here's a bunch of spaces so you don't have to stare at code (some people are really scared of it!)
































import logging
from sys import exit
from json import dumps as jsondump, loads as jsonload

from mitmproxy import http

from os import system

# the function splits the website between pre-json[0], json[1], and post-json[2]
# this makes it easier to put it all back together when we're done messing with the json
def split_ytInitialPlayerResponse(website_data):
    first_split = website_data.split("var ytInitialPlayerResponse = ")
    second_split = first_split[1].split(";var meta")

    first_split[0] += "var ytInitialPlayerResponse = "
    second_split[1] = ";var meta" + second_split[1]

    del first_split[1]

    first_split.append(second_split[0])
    first_split.append(second_split[1])

    return first_split

# returns the subtitle file's contents in bytes
# crashes the proxy if it fails to do so
def get_subtitle_file():
    try: 
        with open(subtitle_location, "rb") as subtitle_file:
            return subtitle_file.read()

    except FileNotFoundError:
        logging.error(f"Subtitle file at {subtitle_location} not found! Closing proxy...")
        exit(1)

    except Exception as e:
        logging.error(f"Error when reading subtitle file!\n{type(e).__name__}: {e.message}")
        exit(1)

# this is a dry-run, just to check if there's an actual subtitle there
_ = get_subtitle_file()

# `generate_dummy_captions` and `makeSubtitlesVisible` are the two functions I lifted from the original script
# they make a dummy subtitle entry for "Preview", so even videos with no subtitles available can show subtitles

def generate_dummy_captions(video_id):
    # generate text track
    captionTrack = {
        "baseUrl": "https://www.youtube.com/api/timedtext?v=" + video_id,
        "vssId": ".pr",
        "languageCode": "pr",
        "isTranslatable": True,
        "name": {
            "runs": [{
                "text": "Preview"
            }]
        }        
    }

    # generate audio track
    audioTrack = {
        "defaultCaptionTrackIndex": 0,
        "hasDefaultTrack": True,
        "captionTrackIndices": [ 0 ],
    }

    #put it all together
    renderer = {
        "captionTracks": [ captionTrack ],
        "audioTracks": [ audioTrack ],
        "defaultAudioTrackIndex": 0,
    }
    return { "playerCaptionsTracklistRenderer": renderer }



def makeSubtitlesVisible(flow: http.HTTPFlow):
    if not flow.request.url.startswith("https://www.youtube.com/watch"): return

    response_body = flow.response.text
    
    split_website = split_ytInitialPlayerResponse(response_body)
    youtube_json = jsonload(split_website[1])
    
    if not "captions" not in youtube_json: return

    video_id = youtube_json["videoDetails"]["videoId"]
    youtube_json["captions"] = generate_dummy_captions(video_id) 

    # put back the HTML with the new json
    amended_response = split_website[0] + jsondump(youtube_json) + split_website[2]
    flow.response.content = amended_response.encode()


def replaceSubtitles(flow: http.HTTPFlow):
    if not flow.request.url.startswith("https://www.youtube.com/api/timedtext"): return
    
    flow.response = http.Response.make(
        200,
        get_subtitle_file(),
        { 
         "Host": flow.request.url,
         "Content-Type": "text/html; charset=utf-8",
         "Cache-Control": "no-cache, must-revalidate",
         "Pragma": "no-cache"
        }
    )

# mitmproxy functions
def response(flow: http.HTTPFlow):
    makeSubtitlesVisible(flow)

def request(flow: http.HTTPFlow):
    replaceSubtitles(flow)
