## [Unreleased]

## [0.6.0] - 18/09/2026

### Added

- `FoldoutGUI`: the shared foldout header (band, arrow, click-to-toggle, alt-click group toggle), `Title(primary, secondary)` label pair, `BeginContent`/`EndContent` indented body, `State` and `SessionBool` expanded-state storage, and the ghost icon buttons (kebab, add, remove). Moved here from the Localization, Styling and Sequencer packages so inspectors share one look

### Changed

- `CHANGELOG.md` now ships inside the package folder
- `ColorExtra` OkLab and OkLCh interpolation now blends in true linear light: a `Color`'s nominal sRGB values are converted to linear before the perceptual maths and back afterwards, regardless of the project's colour space. Previously, in a linear project, the sRGB values were fed to the maths untouched, which darkened and muddied the midtones. This changes the output of every OkLab/OkLCh transition (including Property Bindings and Styling)

## [0.5.1] - 21/06/2026

- Fixed drawers applying value changes outside of the GUI loop, making it impossible to (for example) detect value changes in editor code:
   - [ColorInterpolationModeDrawer](com.timbojimbo.core/Editor/ColorInterpolationModeDrawer.cs)
   - [DiscreteValueSelectionModeDrawer](com.timbojimbo.core/Editor/DiscreteValueSelectionModeDrawer.cs)
   - [EaseTypeDrawer](com.timbojimbo.core/Editor/EaseTypeDrawer.cs)

## [0.5.0] - 04/06/2026

- Moved shared easing/interpolation related enums/editors/etc to Core package

## [0.1.0] - 04/06/2026

- Initial commit with core utilities and extensions that are used across multiple packages.