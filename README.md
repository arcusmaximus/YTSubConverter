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

Conversion is straightforward: launch the program, open your .ass file and click Convert. Alternatively, drag the .ass straight onto the .exe. In both cases, you'll get a .ytt file that's ready for upload.

The program tries to approximate the look of the Aegisub subtitles as closely as possible on YouTube:

![Outlines](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/images/outlines.png)

You'll notice that each .ass shadow can turn into one of three different YouTube shadow types: soft shadow, hard shadow and glow (same as outline). You can configure the shadow type in the conversion UI. This is also where you can configure current word highlighting for karaoke ([example](https://www.youtube.com/watch?v=il4cAeVzZwI)).

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
  * Roboto (YouTube default; the tool will automatically pick this if the specified font is not allowed)
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

## Examples
The repository contains two sample .ass files:
* [Color-coded dialogue sample](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/sample1.ass) ([YouTube video](https://www.youtube.com/watch?v=AvBxTdwCfzs))
* [Karaoke sample](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/sample2.ass) ([YouTube video](https://www.youtube.com/watch?v=il4cAeVzZwI))

## Testing
After you upload a subtitle file, YouTube gives you a preview so you can try it out before submitting. This is nice, except that the preview only shows the file's text; it doesn't show the styling. This complicates testing - each time you make a change and want to see the result, you'd have to actually publish the subtitles so you can see them in the “real” player. This is especially bothersome if you're contributing to someone else's channel, as you'd have to get the subtitles approved each time (or make a copy of the video on your own channel).

Fortunately, there's an easier way to test your subtitles - one which doesn't require you to upload them at all. It works by using Fiddler, a program which can intercept web requests from your browser and send back a file from your hard drive (rather than one from YouTube's servers). By redirecting your browser's request for subtitles to your local .ytt file, you can see those local subtitles in your browser *as though* you uploaded them. Since you're not *actually* uploading them, you can test your changes much more quickly.

While this approach can save you a lot of time, it does require some initial setup:
* Download and install [Fiddler](https://www.telerik.com/download/fiddler).
* Launch the program.
* Open the menu Tools → Options.
  * On the “HTTPS” tab, enable “Capture HTTPS CONNECTs” as well as “Decrypt HTTPS traffic.”
  * Allow the program to install the security certificate. (Note: if you're using Firefox, some [additional steps](https://docs.telerik.com/fiddler/Configure-Fiddler/Tasks/FirefoxHTTPS) are needed)
  * Change the dropdown that says “...from all processes” to “...from browsers only.”
  * Click OK.
* In the toolbar, change “Keep: All sessions” to “Keep: 100 sessions.” (This is to keep the request log from growing too much if you leave the program open for a long time)
* Switch to the “AutoResponder” tab in the right hand panel.
  * Put checkmarks in “Enable rules” and “Unmatched requests passthrough.”
  * Click “Add Rule.”
  * In the “Rule Editor” at the bottom, put the following text in the top textbox: `regex:^https://www.youtube.com/api/timedtext`
  * Click “Save.”

Once this initial setup is done, you only need to do the following whenever you want to test subtitles:
* Launch Fiddler
* Select the rule on the “AutoResponder” tab
* Put the path to your local .ytt file in the bottom textbox in the “Rule Editor”
* Click “Save.”

As long as Fiddler is running (and “Capture Traffic” is enabled in the “File” menu), any YouTube video you view will have the specified .ytt file as its subtitles. If you make a change to the file, you don't even need to refresh the page in your browser to see it; simply disable and re-enable subtitles in the video, which will cause the YouTube player to “redownload” them.

## Uploading
Styled subtitles work on your own videos, but also on those made by others: if a content creator enabled community subtitles on a video, you can upload styled subtitles for it.

You can upload subtitles through the “Actions” dropdown in YouTube's built-in subtitle editor.

![Upload menu](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/images/upload.png)

Once the upload is complete, click “Submit contribution” while making sure not to change *anything* in the built-in editor. If you do, all styling information will be lost. (YouTube warns you about this when uploading to your own channel, but not when uploading to others').

## Limitations
YouTube has some bugs and limitations when it comes to styled subtitles. Please be aware of the following:
* In general, you can only use one style per line of text. For example, while you can make an entire line bold or italic, you can't do this for a single word within a line. In other words, this works: `What's happening?\N{\b1}Nononono!` (only the second line is bold), but this doesn't: `I {\b1}told{\b0} you not to go there!` (nothing will be bold).
  * As an exception to the above, multiple colors within a line *are* possible, but only on PC. For example, the “MAAAN” in `Devil{\c&H0000FF&}MAAAN{\r}!` will be red on PC; on mobile, however, it'll have the same color as the rest of the line.
* Subtitles positioned off-center will move out towards the sides in theater mode, possibly even hanging out of the video frame.
* The mobile apps don't support background customization; they show a black rectangle no matter what color or transparency you specify. This means you need to be careful with dark text, because while it'll be perfectly readable on a custom bright background on PC, it'll be barely readable on the default background on mobile.
  * YTSubConverter detects dark text and adds an invisible, brighter subtitle on top of it. Because the Android app ignores transparency, (only) Android users will see this bright version and be able to read the subtitle. iOS users, however, are not so lucky - the app doesn't show the invisible subtitle, leaving only unreadable black-on-black text.

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
