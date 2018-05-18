## Who should consider contributing to this project?
- Every user may contribute to this project by creating an issue for a bug report or feature request.
- Experienced users with C# and ASP.NET Core may contribute pull requests to this project following the Coding Style below

## What we expect from users who want to contribute a pull request
- Read the file [_Development/README.md](https://github.com/Legedric/ptmagic/blob/master/_Development/README.md) and follow the instructions carefully
- In depth knowledge of the project PT Magic and what it is supposed to do
- In depth knowledge of the architecture, the different parts and layers of PT Magic

## What main goal should I follow when I want to implement something and submit my pull request?
PT Magic is a project/addon that supports the trading experience of several users of Profit Trailer.

## What pull requests will get rejected?
PRs that do not apply to the general goal will be rejected. For example, PTM is not meant to replace major parts of Profit Trailer, so anything that goes too much into actual trading (like implementing indicators) will probably get rejected.
Furthermore we always try to not over-engineer things. If you think that the feature you want to implement sounds somehow complicated, its most likely is. Always ask yourself if adding a new level of complexity is really necessary or you may end up with a rejected pull request.

## Pull Request Rules
1. Be as detailed as possible with your commit messages and descriptions.
2. If we do not understand what you did in your PR, it will most likely get rejected.
3. The code has to follow the Coding Style at all times.
4. Test your stuff! We are not your beta testers! What doesn't work, won't get accepted.
5. Respected the .gitignore file at all times. Do not commit your personal settings or Profit Trailer data etc.

## Recommended Tools
- Visual Studio Code
- C# (OmniSharp) extension

## Coding Style
1. We use OmniSharp (and omnisharp.json) to setup some basic code style guidelines.
2. We use two spaces of indentation (no tabs).
3. We use CamelCase for all namings.
4. Abbreviations are only allowed in Lamdba/Linq expressions or for some variables when it is clearly visible what this variable is supposed to contain.
5. We always put comments into their own lines. No inline comments are allowed and no comment blocks (with /* comment */) are allowed.
6. We use only English language for all namings.
7. We use "Simplicity over Elegance" at all times. Meaning that code has to be simple and understandable, not compact and elegant.
