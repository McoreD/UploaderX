# UploaderX
macOS implementation of UploaderX

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
