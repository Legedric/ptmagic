# Instructions for developing PT Magic
These are some instructions on how to start developing for PT Magic.
The tools listed below (VS Code and its extensions) are only recommendations. Of course you may use any other suitable tool to develop .NET Core projects with.
However, you should be carefull to not mess up your files when you commit using a different tool as most tools add some user specific files to the workspace and we do not want to have those in our repository.

## Setup your environment
1. Download and install Visual Studio Code for your platform from https://code.visualstudio.com/
2. Install the VS Code extension C# (OmniSharp)
3. Install .NET Core from https://www.microsoft.com/net/download/

## Setup PT Magic for development
1. Copy settings.analyzer.json and settings.general.json from /_Development/DevSettings to /PTMagic
2. Setup a Profit Trailer instance for development (for example in _Development/LocalProfitTrailer)
3. Adjust the necessary values in settings.general.json to your needs