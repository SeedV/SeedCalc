# SeedCalc - A Wonder Calculator

Numbers take you to wonders! Let kids explore how tall dinosaurs are, how small
DNA is, and how big the sun is!

![Screenshot1](./Screenshots/01_en.jpg)

## About SeedCalc

SeedCalc helps kids understand numbers, orders of magnitude, and the four
operations with fun things at different scales.

![Screenshot2](./Screenshots/02_en.jpg)

### Key features

- Dozens of fun things and animations to show different scales of the universe.
- Demonstrate the precedence of the four operations.
- A full-featured calculator, supporting parentheses, and the four arithmetic
  operations.

![Screenshot3](./Screenshots/03_en.jpg)

### Background

SeedCalc is a visualizable calculator application. It uses
[SeedLang](https://github.com/SeedV/SeedLang) as the underlying runtime
engine, demonstrating SeedLang's visualization abilities with educational
animations.

## Official releases and downloads

- iOS App (iPad only): [SeedCalc - A Wonder
  Calculator](https://apps.apple.com/app/seedcalc-a-wonder-calculator/id1606514630)

## How to

SeedCalc is a Unity project, designed for both Tablets and PCs. The project can
be built into iOS app, macOS app, Windows exe, etc.

### Prerequisites

Please install [Git Large File Storage](https://git-lfs.github.com/) before git
cloning this repository. Also check [.gitattributes](.gitattributes) for the
file types that are managed with git-lfs.

Please check [ProjectVersion.txt](./ProjectSettings/ProjectVersion.txt) for the
required version of Unity.

### Contributing

For the developers of SeedCalc, please check out the [contributing
guide](CONTRIBUTING.md) for guidelines about how to proceed.

### Localization

Fow now, SeedCalc supports English (en) and Simplified Chinese (zh-CN). Here is
a screenshot when SeedCalc is running in Simplified Chinese.

![Screenshot1zh](./Screenshots/01_zh.jpg)

We use [Unity
Localization](https://docs.unity3d.com/Packages/com.unity.localization@1.1/manual/index.html)
to localize the project.

Remember to build the addressable assets group before building the application.
Otherwise the localized texts won't be shown correctly. See [Unity Localization:
Preview and configure your
build](https://docs.unity3d.com/Packages/com.unity.localization@1.1/manual/QuickStartGuideWithVariants.html#preview-and-configure-your-build)
for more details.

### Open source and Non-open source assets

Although the SeedCalc source code is 100% open-sourced, its official releases
use a number of non-open-source assets which are not included in this GitHub
repo. We replace them with placeholder objects. Please take a look at the About
dialog of the application if you are interested in the copyrights or the source
links of those non-open source assets.

