# YTSubConverter
A tool for creating colored YouTube subtitles.

![Sample image](https://raw.githubusercontent.com/arcusmaximus/YTSubConverter/master/sample.png)

## About this tool
YouTube's built-in subtitle editor does not support formatting of any kind. However, it's possible to have formatting such as bold, italic and coloring by uploading a subtitle file instead. The site accepts a number of file formats; in the case of .rt (RealText), it even allows multiple differently formatted sections within one subtitle line.

Of course, you could create such an .rt file using one of the many existing conversion tools. However, conversion alone is not enough to make the subtitles display correctly on YouTube:
* Uploaded subtitles are shown with a delay of approximately 60ms.
* If you have two adjacent `<font>...</font>` segments in an .rt file, the formatting of the second one will be ignored. For example, `<font color="red">One</font><font color="blue">Two</font>` will result in a red "One" and a *white* "Two", but if a space is inserted between the first `</font>` and the second `<font>`, both words will be colored.

YTSubConverter works around these problems as follows:
* Shifts all timings by -60ms to compensate for the delay.
* Inserts zero-width spaces between differently formatted sections to make coloring work everywhere without affecting text layout.

## Usage
At its core, YTSubConverter is an .ass -> .rt converter. You can create .ass subtitles using e.g. [Aegisub](http://www.aegisub.org/), which allows you to set up and preview the formatting before uploading.

The tool has two functions:
* If you drag&drop an .ass file onto the .exe, it'll create an .rt file that's ready for uploading to YouTube.
* If you drag&drop an .sbv file, it'll create an .srt file that's ready for opening in a subtitle editing program (and saving to .ass afterwards). This feature is handy for adding formatting to subtitles that were created in YouTube's built-in editor; you can click the "Download" button to obtain the .sbv file.

## Formatting support
YouTube only supports bold, italic, underline and coloring when using .rt files. Correspondingly, YTSubConverter only reads `{\b}`, `{\i}`, `{\u}` and `{\c}` tags from .ass files and ignores any others.

Subtitles with formatting work on both your own videos and videos made by others (i.e. if a content creator enabled community subtitles on a video, you can upload formatted subtitles for it). In both cases, it's important to immediately submit the subtitles after uploading; if you make any change in the built-in editor, formatting will be lost.

Note that the video preview in the built-in editor *does not* show the formatting; it'll only become visible on the actual video once the subtitles are published/approved.

## Example workflow for creating subtitles that are color-coded by speaker
* Download the video using e.g. [youtube-dl](http://yt-dl.org). (Tip: because YT-DL picks the highest resolution by default, you can save time by using `-F` to discover the available video resolutions and then downloading with `-f <number>` to download a smaller file.)
* Open the locally saved video in a player that supports global hotkeys (e.g. VLC). If you haven't yet, set up hotkeys for pausing, resuming and rewinding the video.
* Open Notepad and type out the subtitles, using the global hotkeys to control the video without having to switch between windows.
* While typing, prefix each line/section with a "special" character (such as `*`, `+`...) to identify the speaker. Use `\N` for manual line breaks. (Example: `*Huh, that sign has an evil rabbit on it\N+That does look evil`)
* When done, do a search/replace of each special character by the corresponding .ass color code (e.g. `*` -> `{\c&HA92EED&}`); note that the color is in the format BBGGRR, that is, in the opposite order as it would be in HTML.
* Save the subtitles as a .txt file.
* Open the .txt file in Aegisub and set up the timings and (if needed) additional formatting.
* Save the subtitles as an .ass file.
* Drag&drop the .ass file onto YTSubConverter.exe.
* Upload the resulting .rt file to YouTube.
