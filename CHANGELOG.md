## [0.5.1] - 21/06/2026

- Fixed drawers applying value changes outside of the GUI loop, making it impossible to (for example) detect value changes in editor code:
   - [ColorInterpolationModeDrawer](com.timbojimbo.core/Editor/ColorInterpolationModeDrawer.cs)
   - [DiscreteValueSelectionModeDrawer](com.timbojimbo.core/Editor/DiscreteValueSelectionModeDrawer.cs)
   - [EaseTypeDrawer](com.timbojimbo.core/Editor/EaseTypeDrawer.cs)

## [0.5.0] - 04/06/2026

- Moved shared easing/interpolation related enums/editors/etc to Core package

## [0.1.0] - 04/06/2026

- Initial commit with core utilities and extensions that are used across multiple packages.