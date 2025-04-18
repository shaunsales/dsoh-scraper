# Deeper Shades of House Audio Downloader

A .NET 9.0 console application that downloads audio files from the Deeper Shades of House website and saves them to your local file system.

If you use this app, please support Lars Behrenroth at https://www.deepershades.net, or on SoundCloud: https://soundcloud.com/larslb

## Features

- Downloads audio files from Deeper Shades of House on SoundCloud
- Supports downloading multiple shows in parallel
- Shows download progress with a nice console UI powered by Spectre.Console
- Configurable output directory and parallel download limit
- Supports downloading single shows or ranges of shows
- Modern C# coding practices including source-generated regex and centralized package management

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

## Project Structure

- `Program.cs`: Main entry point with top-level statements
- `Services/AudioDownloader.cs`: Module for downloading audio files with parallel execution support
- `Services/ConsoleService.cs`: Static service for handling all console input/output operations
- `Directory.Packages.props`: Centralized package management file

## Dependencies

- [SoundCloudExplode](https://github.com/MuddyC3/SoundCloudExplode) (SoundCloud API access)
- [Spectre.Console](https://spectreconsole.net/) (Console UI and progress)
