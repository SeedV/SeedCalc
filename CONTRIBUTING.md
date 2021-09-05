# Contributing to SeedCalc

Thanks for taking the time to contribute!

## Submitting changes

For the main branch, all commits must be submitted via a pull request with at
least one approving reviews from the core team.

For the developers who have write-access to the repository, please consider the
following branch naming conventions before submitting a pull request:

* Release branches: `release_<version>`
* Personal working branches:
  * `wip_<your-github-id>`, or
  * `wip_<your-github-id>_<task-desc>`, or
  * `wip_<your-github-id>_<sequence_no>`, or
* Experimental branches:
  * `exp_<your-github-id>`, or
  * `exp_<your-github-id>_<sequence_no>`
* Temporary bugfix branch: `bugfix_<issue-id>`
* Temporary hotfix branch: `hotfix_<issue-id>`
* Temporary feature branch: `feature_<issue-id>`

Please follow [How to Write a Git Commit
Message](https://chris.beams.io/posts/git-commit/) when writing your commit
messages whenever possible.

## Coding conventions

### Directory and file structure

```shell
<ProjectRoot>
  └── Assets
        ├─── Audio             # Music and sound effects.
        ├─── Fonts             # Font files. TTF format is preferred.
        ├─── Materials         # Material definitions.
        ├─── Models            # Imported models. FBX format is preferred.
        ├─── Plugins           # Plugin files such as DLL assemblies.
        │     └─── <platform>  # Platform-specific plugin files.
        ├─── Prefabs           # Unity prefabs.
        ├─── Scenes            # Scene definitions.
        ├─── Src               # Source code files.
        │     ├─── Scripts     # C# scripts.
        │     └─── Shaders     # ShaderLab source code files.
        └─── Textures          # PNG and JPG files.
```

See [Unity Plug-ins](https://docs.unity3d.com/Manual/Plugins.html) for the
preferred organization of compatible and platform-specific plugin files.

### Directory names and filenames

* Directory names use `PascalCase`. E.g.:
  * `/Assets/Audio`
  * `Src/Scripts`
* Filenames of source code and configurations use `PascalCase`. E.g.:
  * `GameManager.cs`
  * `MainScene.unity`
* Non-code assets use `lower_case_with_underscores`. E.g.:
  * `button_click.mp3`
  * `button.png`
* Third-party assets may keep their original filenames. E.g.:
  * `RobotoMono-Medium.ttf`

### C# scripts

* Please follow the [Google C# Style
  Guide](https://google.github.io/styleguide/csharp-style.html).

### Shaders (Unity ShaderLab and HLSL code)

* We follow the [Google C# Style
  Guide](https://google.github.io/styleguide/csharp-style.html) for whitespace
  rules, indention and line-wrapping rules, including:
  * Column limit: 100 chars.
  * Indentation of 2 spaces, no tabs.
  * Line continuations are indented 4 spaces.
  * No line break before opening brace.
  * No line break between closing brace and else.
* As for naming rules, please refer to the docs of
  [HLSL](https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference)
  and [Unity ShaderLab](https://docs.unity3d.com/Manual/SL-Shader.html). Here
  are some examples:
  * `_ExamplePropertyName(..) = ...`
  * `_ExamplePropertyVariable`
  * `sampler2D _MainTex`
  * `float4 _MainTex_ST`
  * `const ... constVariable = ...`
  * `float4 localVariable`
  * `#define MACRO_NAME ...`
  * `struct StructName { ... }`
  * `void FunctionName(...) ...`

 Please use your best judgement as many inconsistent styles can be found in the
 docs of
 [HLSL](https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference)
 and [Unity ShaderLab](https://docs.unity3d.com/Manual/SL-Shader.html).

### Documentations

* Please follow the [Google documentation
  guide](https://google.github.io/styleguide/docguide/) whenever possible.
