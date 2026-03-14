# myconference

This [app](https://github.com/jfversluis/myconference) was originally created as part of a live stream demostrating using [Copilot to develop MAUI applications](https://www.youtube.com/watch?v=9uO1rY2aswU).

There have been several changes in this port:

- Fixed the themes to properly support dark and light mode.
- The original app didn't set the fonts for its text, I've updated it so it does.
- Changed the "heart" emoji used for selecting favorites to be a path icon.
- Fixed trimming support, including System.Text.Json JsonSerializerContext's. This is needed so the app can work on Browser.
- Added a CORS proxy so the Json from Sessionize can be fetched. This is needed for Browser support as well.