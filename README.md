# YTSubConverter
A tool for creating styled YouTube subtitles.

![Sample image](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/sample.png)

## About this tool
YouTube's built-in subtitle editor does not support styling of any kind. If you want formatting such as bold, italic and coloring, you need to upload a subtitle file instead. The site accepts a number of file formats such as RealText, WebVTT and TTML, but all of these come with their own limitations - and most importantly, none of them give access to the full array of features offered by the YouTube player. For that, you need to use a YouTube-specific format called YTT (YouTube Timed Text). It supports the following:
* Text coloring and transparency
* Background coloring and transparency (including hiding the background box completely)
* Glowing text
* Text with a drop shadow
* Bold/italic/underline
* Fonts
* Positioning (place your subtitles anywhere on the video)
* Karaoke timing (make each word of a lyric appear right as it's sung)

YTSubConverter can produce this file format for you.

## Usage
At its core, YTSubConverter is an .ass -> .ytt converter. You can create .ass subtitles using e.g. [Aegisub](http://www.aegisub.org/), which allows you to set up and preview the styling before uploading.

Conversion is straightforward: launch the program, open your .ass file and click Convert. You'll get a .ytt file that's ready for upload.

Some things to keep in mind while creating your subtitles:
* By default, Aegisub subtitles don't have a background box, meaning the resulting YouTube subtitles also won't have one. If you want a background box, open Aegisub's Style Editor and check "Opaque box" in the "Outline" section.
* A regular (non-box) outline in the .ass will result in a glow effect in the .ytt.
* A shadow in the .ass will result in a soft shadow in the .ytt. If you want to change this to a hard shadow or a glow effect, you can do so in YTSubConverter's "Style options."
* You can't have a regular outline and a shadow at the same time - if you specify both, YTSubConverter will use the outline and ignore the shadow. Having a box outline and a shadow at the same time, however, is perfectly possible.

![Style options](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/style-options.png)

Apart from converting from .ass to .ytt, the tool can also convert from .sbv (the format you get when downloading subs from YouTube's built-in editor) to .srt. This allows you to download existing, unstyled subs and add styling to them. Simply drag the .sbv file onto the window, click Convert, and open the .srt file in Aegisub.

## ASS feature support
YTSubConverter supports .ass styles as well as the following override tags:
* `{\b}` - bold
* `{\i}` - italic
* `{\u}` - underline
* `{\fn}` - font. YouTube only allows the following fonts:
** Carrois Gothic SC
** Comic Sans MS
** Courier New
** Deja Vu Sans Mono
** Monotype Corsiva
** Times New Roman
** YouTube Noto (default)
* `{\c}`/`{\1c}` - text color
* `{\3c}` - glow/background color
* `{\4c}` - glow/shadow color
* `{\1a}` - text transparency
* `{\3a}` - background transparency
* `{\pos}` - position
* `{\k}` - karaoke segment duration
* `{\r}` - reset to current style
* `{\fad}` - simple fade
* `{\fade}` - complex fade

Unsupported tags will be ignored.

## Details about uploading
Styled subtitles work on your own videos, but also on those made by others: if a content creator enabled community subtitles on a video, you can upload styled subtitles for it. In both cases, it's important to immediately submit the subtitles after uploading; if you make any change in the built-in editor, all styling information will be lost.

Note that the subtitle preview in the built-in editor *does not* show the styling; it'll only become visible on the actual video once the subtitles are published/approved.

## Example workflow for creating subtitles that are color-coded by speaker
* Download the video using e.g. [youtube-dl](http://yt-dl.org). (Tip: because YT-DL picks the highest resolution by default, you can save time by using `-F` to discover the available video resolutions and then downloading with `-f <number>` to download a smaller file.)
* Open the locally saved video in a player that supports global hotkeys (e.g. VLC). If you haven't yet, set up hotkeys for pausing, resuming and rewinding the video.
* Open Notepad and type out the subtitles, using the global hotkeys to control the video without having to switch between windows.
* While typing, prefix each line/section with a "special" character (such as `*`, `+`...) to identify the speaker. Use `\N` for manual line breaks. (Example: `*Huh, that sign has an evil rabbit on it\N+That does look evil`)
* When done, do a search/replace of each special character by the corresponding .ass color code (e.g. `*` -> `{\c&HA92EED&}`); note that the color is in the format BBGGRR, that is, in the opposite order as it would be in HTML.
* Copy the text document and paste it into Aegisub's subtitle grid.
* Set up the timings and (if needed) additional formatting.
* Save the subtitles as an .ass file.
* Convert the .ass to .ytt using YTSubConverter.
* Upload the .ytt to YouTube.
