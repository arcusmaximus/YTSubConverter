# YTSubConverter
A tool for creating styled YouTube subtitles.

![Sample image](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/images/sample.png)

## About this tool
YouTube's built-in subtitle editor doesn't support styling of any kind. If you want formatting such as bold, italic and coloring, you need to upload a subtitle file instead. The site accepts a number of file formats such as RealText, WebVTT and TTML, but all of these come with their own limitations - and most importantly, none of them give access to the full array of features offered by the YouTube player. For that, you need to use a YouTube-specific format called YTT (YouTube Timed Text). It supports the following:
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
* By default, Aegisub subtitles don't have a background box, meaning the resulting YouTube subtitles also won't have one. If you want a background box, open Aegisub's Style Editor and check “Opaque box” in the “Outline” section.
* A regular (non-box) outline in the .ass will result in a glow effect in the .ytt.
* A shadow in the .ass will result in a soft shadow in the .ytt. If you want to change this to a hard shadow or a glow effect, you can do so in YTSubConverter's “Style options.”

![Outlines](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/images/outlines.png)

![Style options](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/images/style-options.png)

Apart from converting from .ass to .ytt, the tool can also convert from .sbv (the format you get when downloading subs from YouTube's built-in editor) to .srt. This allows you to download existing, unstyled subs and add styling to them. Simply open the .sbv file, click Convert, and open the resulting .srt file in Aegisub.

## ASS feature support
YTSubConverter supports the following .ass style features:
* Font name. YouTube only allows the following fonts:
  * Carrois Gothic SC
  * Comic Sans MS
  * Courier New
  * Deja Vu Sans Mono
  * Monotype Corsiva
  * Times New Roman
  * Roboto (default)
* Bold, italic, underline
* Primary, outline and shadow color
* Alignment
* Outline and shadow thickness (only checking whether the value is 0 or greater than 0)

It also supports the following override tags:
* `{\b}` - bold
* `{\i}` - italic
* `{\u}` - underline
* `{\fn}` - font. (See above for list of allowed fonts)
* `{\c}` or `{\1c}` - regular text color
* `{\2c}` - unsung karaoke text color
* `{\3c}` - outline color
* `{\4c}` - shadow color
* `{\1a}` - regular text transparency
* `{\2c}` - unsung karaoke text transparency
* `{\3a}` - background transparency
* `{\pos}` - position
* `{\k}` - karaoke segment duration
* `{\r}` - reset to current style
* `{\fad}` - simple fade
* `{\fade}` - complex fade

Unsupported tags are ignored.

## Example
The repository contains a sample .ass file which uses the most common styling features.
* [Sample .ass file](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/sample.ass)
* [YouTube video with these subtitles](https://www.youtube.com/watch?v=4HsiIqcHsRs)

## Limitations
YouTube has some bugs and limitations when it comes to styled subtitles. Please be aware of the following:
* In general, you can only use one style per line of text. For example, while you can make an entire line bold or italic, you can't do this for a single word within a line. In other words, this works: `What's happening?\N{\b1}Nononono!` (only the second line is bold) but this doesn't: `I {\b1}told{\b0} you not to go there!` (nothing will be bold).
  * As an exception to the above, multiple colors within a line *are* possible, but only on desktop. For example, the “MAAAN” in `Devil{\c&H0000FF&}MAAAN{\r}!` will be red on desktop; on mobile, however, it'll have the same color as the rest of the line.
* Subtitles positioned off-center will move out towards the sides in theater mode, possibly even hanging out of the video frame.

## Details about uploading
Styled subtitles work on your own videos, but also on those made by others: if a content creator enabled community subtitles on a video, you can upload styled subtitles for it. In both cases, it's important to immediately submit the subtitles after uploading; if you make any change in the built-in editor, all styling information will be lost.

Note that the subtitle preview in the built-in editor *does not* show the styling; it'll only become visible on the actual video once the subtitles are published/approved.

## Example workflow for creating subtitles that are color-coded by speaker
* Download the video using e.g. [youtube-dl](http://yt-dl.org). (Tip: because YT-DL picks the highest resolution by default, you can save time by using `-F` to discover the available video resolutions and then downloading with `-f <number>` to download a smaller file.)
* Open the locally saved video in a player that supports global hotkeys (e.g. VLC). If you haven't yet, set up hotkeys for pausing, resuming and rewinding the video.
* Open Notepad and type out the subtitles, using the global hotkeys to control the video without having to switch between windows.
* While typing, prefix each line with a “special” character (such as `*`, `+`...) to identify the speaker. Use `\N` for manual line breaks. (Example: `*Huh, that sign has an evil rabbit on it\N+That does look evil`)
* When done, do a search/replace of each special character by the corresponding .ass color code (e.g. `*` -> `{\c&HA92EED&}`); note that the color is in the format BBGGRR, that is, in the opposite order as it would be in HTML.
* Copy the text document and paste it into Aegisub's subtitle grid.
* Set up the timings and (if needed) additional formatting.
* Save the subtitles as an .ass file.
* Convert the .ass to .ytt using YTSubConverter.
* Upload the .ytt to YouTube.
