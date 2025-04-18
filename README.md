# Deeper Shades of House Audio Downloader

A .NET 9.0 console application that downloads audio files from the Deeper Shades of House website and saves them to your local file system.

If you use this app, please support Lars Behrenroth at https://www.deepershades.net, or on SoundCloud: https://soundcloud.com/larslb

## Features

- Downloads audio files from Deeper Shades of House on SoundCloud
- Supports downloading multiple shows in parallel
- Shows download progress with a nice console UI powered by Spectre.Console
- Configurable output directory and parallel download limit
- Supports downloading single shows or ranges of shows

## Usage

### Running from the command line

```bash
# Download a single show
dotnet run -- 896

# Download multiple shows
dotnet run -- 896 897 898

# Download a range of shows
dotnet run -- 890-895

# Download a mix of single shows and ranges
dotnet run -- 890-895 900 905
```

### Interactive mode

If you run the application without any arguments, it will prompt you for:
- Output directory (where downloaded files will be saved)
- Maximum number of parallel downloads
- Show numbers or ranges to download

Then follow the on-screen instructions to enter the required information.

## Dependencies

- [SoundCloudExplode](https://github.com/MuddyC3/SoundCloudExplode) (SoundCloud API access)
- [Spectre.Console](https://spectreconsole.net/) (Console UI and progress)

## Publishing & Deployment

This project is configured for modern .NET deployment features:

- **AOT Compilation**: Publish fully native executables for maximum performance and fast startup.
- **Single-file publishing**: All dependencies are bundled into a single output file.
- **Trimming & Compression**: Unused code is trimmed and the output is compressed for minimal file size.
- **Invariant globalization**: Reduces binary size by removing culture-specific data.
- **Self-contained**: No .NET runtime installation required on the target machine.

### How to publish a native, single-file executable

```bash
dotnet publish -c Release -r <RID> --self-contained true /p:PublishAot=true
```
Replace `<RID>` with your target runtime (e.g., `win-x64`, `osx-arm64`, `linux-x64`). The output will be a single, native executable in the `bin/Release/net9.0/<RID>/publish` folder.

All these features are enabled by default in the project file.
