# UploaderX
Cross-platform implementation of ShareX.UploadersLib

# Getting Started
- Grab a copy of your `UploadersConfig.json` from ShareX
- Grab a copy of your `ApplicationConfig.json` from ShareX
- Copy the `json` files into the same folder where UploaderX resides

## macOS
- To customize the Destination Folder, update the following field in the `ApplicationConfig.json` file:
e.g. 
```json
"CustomScreenshotsPath2": "/Users/mike/Library/CloudStorage/OneDrive-Personal/Pictures/Screenshots",
```

## Windows
- To customize the Destination Folder, update the following field in the `ApplicationConfig.json` file:
e.g. 
```json
"CustomScreenshotsPath2": "C:\\Users\\mike\\Pictures\\Screenshots",
```

# ApplicationConfig.json
- If you are keen to create the json file from scratch, the following (untested) should work:
```json
{
 "BufferSizePower": 5,
 "CustomScreenshotsPath2": "C:\\Users\\mike\\Pictures\\Screenshots"
}
```

# Features
- Supports Amazon S3 as long as settings are configured in `UploadersConfig.json`
- Supports Imgur if no json file is configured
- Supports converting `mov` to `mp4` if `ffmpeg` is available in `Tools` folder

# FAQ
## Where are UploaderX app data saved?
- In MAUI app, it is saved in `~/Documents/UploaderX`
- In Avalonia app, it is saved in `~/UploaderX`

## Where is the Watch Folder is no json file is configured?
- Watch Folder is created in the app data folder e.g. `~/Documents/UploaderX/Watch Folder`
