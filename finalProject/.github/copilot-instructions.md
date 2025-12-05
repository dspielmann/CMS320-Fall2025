Project: Unity finalProject — AI assistant instructions

Quick orientation
- **Unity Editor version:** `ProjectSettings/ProjectVersion.txt` shows `m_EditorVersion: 6000.2.7f2` — open with that version (or the closest compatible Unity 6.x build).
- **Open in IDE:** use `finalProject.sln` / `Assembly-CSharp.csproj` for code navigation in Visual Studio or Rider.

Where to look first
- `Assets/Scripts/` — all gameplay logic lives here (examples: `PlayerMovement.cs`, `PlayerUIManager.cs`, `AudioManager.cs`).
- `Assets/Scenes/` — scene entry points, serialized references, and UI wiring live in scenes; many behaviors depend on Inspector hookup.
- `Packages/manifest.json` — project packages (contains `com.unity.inputsystem`, `render-pipelines.universal`, etc.).
- `ProjectSettings/` — project-wide settings including Unity editor version and input/rendering configs.

High-level architecture & patterns
- MonoBehaviour-first design: scripts are attached to GameObjects and use public fields for wiring in the Inspector.
  - Example: `PlayerMovement` exposes `nextLevelButton`, `underwaterTimerText`, and `tutorialMode` which are set from the scene.
- Scene-driven flow: code often calls `SceneManager.LoadScene("Level1")` or checks `Finish` tag — scene names and tags are meaningful and authoritative.
  - Example: `InstructionsMenu2.StartGame()` calls `SceneManager.LoadScene("Level1")`.
- Lightweight service discovery: code uses `GameObject.FindGameObjectWithTag("music")` and `Object.FindFirstObjectByType<T>()` rather than a DI container — search for tags and singletons.
  - Example: `AudioManager` is expected on a GameObject with tag `music`.
- Mixed input APIs: `Packages/manifest.json` includes `com.unity.inputsystem`, but many scripts use `Input.GetKeyDown(...)`. Check both `InputSystem_Actions.inputactions` and existing code before changing input handling.

Conventions & gotchas (do not assume defaults)
- Serialized wiring: public fields are frequently left uninitialized in code. Changing constructors or renaming fields will often break scene bindings.
- Tags & names: GameObjects referenced by name or tag (e.g., `CountdownText`, `music`, `Finish`) must exist in the scene hierarchy.
- Scene names: strings like `"Level1"` are used directly; renaming scenes requires updating code or adding a mapping.
- UI hookup: Buttons are wired via public `Button` fields and `onClick` listeners added in `Start()` — check the scene inspector before changing.

Build/test/debug workflows
- Typical workflow: open project in Unity Editor and run the Scene. Most debugging is done in Editor Play mode.
- IDE code compile: open `finalProject.sln` to get IntelliSense and code navigation; Unity controls actual build/compilation.
- Automated builds (if needed): use Unity CLI in CI (example pattern):
  - `"C:\\Program Files\\Unity\\Editor\\Unity.exe" -quit -batchmode -projectPath "<repo>" -buildWindowsPlayer "<out>.exe" -logFile build.log`
  - Verify the exact Unity path / version before running.

What an AI code-change should validate
- Keep serialized field names and signatures stable — prefer adding new fields rather than renaming or removing existing public fields.
- When changing scene wiring or tags, update all scenes under `Assets/Scenes/` and mention required inspector changes in the PR.
- Prefer non-destructive edits: add null checks and clear warnings when a required Inspector field is missing (the codebase already does this in many places).

Useful search terms & entry files
- `PlayerMovement.cs` — rhythm, dive, and collision logic; shows many gameplay conventions.
- `PlayerUIManager.cs` — how UI reads from gameplay classes and expects public getters.
- `AudioManager.cs` — look for audio hooks and tag usage (`music`).
- `InputSystem_Actions.inputactions` — if planning to migrate input, inspect this file and `Packages/manifest.json`.

If you change scenes or serialized fields
- Add a short checklist in PR description listing scenes/objects that must be updated in the Inspector (object names, tags, and field assignments).

Example quick tasks for contributors
- Add a new public UI field: update the script, then open the relevant scene and wire the field in the Inspector before testing.
- Fix a missing null-check: add a defensive null-check + `Debug.LogWarning(...)` and return early — consistent with existing style.

Questions or missing info
- If you need the exact Unity editor installer or CI configuration, tell me which target platform (Windows/Linux/macOS) and I can propose exact CLI commands and a minimal CI step.

— End of instructions —
