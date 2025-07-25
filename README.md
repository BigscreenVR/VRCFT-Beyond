# BeyondExtTrackingInterface

VRCFaceTracking tracking module for the Bigscreen Beyond HMD.

## Installing
Grab the latest build from the Releases page and drop it into 
```
%APPDATA%\VRCFaceTracking\CustomLibs
```
VRCFaceTracking will load the Beyond Module on next launch.

## Building

```bash
# 1. Ensure .NET is installed (>=v7.0)
dotnet --version

# 2. Restore deps & build
dotnet restore
dotnet build -c Release
```

Grab the resulting *`bin/Release/net7.0/BeyondExtTrackingInterface.dll`* and drop it into

```
%APPDATA%\VRCFaceTracking\CustomLibs
```

VRCFaceTracking will load the Beyond Module on next launch.
