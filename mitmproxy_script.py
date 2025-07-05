# READ THESES INSTRUCTIONS!!!!!!!!!!

# Edit "REPLACE ME" in the "subtitle_location" line to be the location of your subtitles.
# Keep the quotes and the starting R.

# It should look something like this (without the hashtag at the start):
# subtitle_location = r"C:\Users\Administrator\Documents\my cool subtitles.ytt"

subtitle_location = r"REPLACE ME"



































import logging
import re
import json
from typing import Any
from mitmproxy import http

# Returns the subtitle file's contents in bytes.
def read_subtitle_file() -> bytes:
    try:
        with open(subtitle_location, "rb") as subtitle_file:
            return subtitle_file.read()

    except FileNotFoundError:
        logging.error(f"Subtitle file {subtitle_location} not found!")
        return b""

    except Exception as e:
        logging.error(f"Error when reading subtitle file!\n{e}")
        return b""

# This is a dry-run, just to check if there's an actual subtitle there
read_subtitle_file()

def generate_dummy_captions(video_id: str) -> dict[str, Any]:
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

def ensure_subtitle_selector(flow: http.HTTPFlow) -> None:
    if not flow.request.url.startswith("https://www.youtube.com/watch"):
        return

    html: str = flow.response.text
    match = re.search(r"var ytInitialPlayerResponse = (.+?);var meta = ", html)
    if match is None:
        return

    player_response = json.loads(match.group(1))
    if "captions" in player_response:
        return

    video_id = player_response["videoDetails"]["videoId"]
    player_response["captions"] = generate_dummy_captions(video_id)

    # put back the HTML with the new json
    html = html[:match.start(1)] + json.dumps(player_response) + html[match.end(1):]
    flow.response.content = html.encode()

def apply_custom_subtitles(flow: http.HTTPFlow):
    if not flow.request.url.startswith("https://www.youtube.com/api/timedtext"):
        return

    flow.response = http.Response.make(
        200,
        read_subtitle_file(),
        {
            "Host": flow.request.url,
            "Content-Type": "text/html; charset=utf-8",
            "Cache-Control": "no-cache, must-revalidate",
            "Pragma": "no-cache"
        }
    )

# mitmproxy functions
def response(flow: http.HTTPFlow):
    ensure_subtitle_selector(flow)

def request(flow: http.HTTPFlow):
    apply_custom_subtitles(flow)
