File: CONTRIBUTING.md
# Contributing to SoulsHero

Thank you for contributing to SoulsHero. This document describes the development standards, coding conventions, and workflow expectations for this repository. All contributors (including maintainers) must follow these guidelines to keep the codebase consistent and high quality.

## Table of Contents

- Guidelines
- Development workflow
- Branching & pull request process
- Commit message style
- Coding standards (C# / Unity)
- UI standards
- Formatting and EditorConfig
- Unit tests & automated CI
- Code reviews
- Adding assets and third-party packages
- Contact / reporting issues

## Guidelines

- Open clear, focused pull requests. Each PR should address a single logical change.
- Keep changes small and easy to review.
- Run unit tests and playtest the affected scenes before opening a PR.
- Include screenshots or short recordings for visual changes (UI, art, animation).

## Development workflow

- Use feature branches off `master` for new features: `feature/<short-description>`.
- Use `hotfix/<short-description>` for urgent fixes to `master`.
- Rebase or squash commits when appropriate to keep history clear.

## Branching & pull request process

1. Create a branch from `master`.
2. Implement changes and add/modify tests.
3. Ensure the editor and project settings are not accidentally changed unless required.
4. Push the branch and open a PR targeting `master`.
5. Add a short description, list of changes, and testing notes.
6. Request reviews from at least one maintainer.
7. Address review comments and re-request review.
8. Merge when approvals are present and CI is green.

## Commit message style

- Use a short, imperative summary (max 50 chars).
- Optionally include a longer description after a blank line.
- Prefix commits with types when useful, e.g., `feat:`, `fix:`, `chore:`, `docs:`.

Example:

```
feat: add stamina UI and binding to PlayerStamina

Adds a new HUD element that displays current stamina.
```

## Coding standards (C# / Unity)

- Follow standard C# naming conventions (PascalCase for public members, camelCase for private fields).
- Use `[Header]` and `[Tooltip]` attributes on serialized fields for Inspector clarity.
- Prefer events/delegates for cross-system communication over direct references.

## UI standards

- **Always use TextMeshPro (`TextMeshProUGUI`)** for all in-game UI text elements. Do **not** use the legacy `UnityEngine.UI.Text` component.
- Import the TMPro namespace with `using TMPro;`.
- Serialized text fields should be typed as `TextMeshProUGUI` (for canvas/screen-space UI) or `TextMeshPro` (for world-space 3D text).
- When referencing text in code, use the `.text` property on the TMP component as usual.

## Formatting and EditorConfig

- Follow the `.editorconfig` rules defined in the repository root (when present).
- Use 4-space indentation for C# files.

## Unit tests & automated CI

- Add or update unit tests for new logic.
- Ensure all tests pass locally before pushing.

## Code reviews

- Be constructive and respectful.
- Focus on correctness, readability, and adherence to these standards.

## Adding assets and third-party packages

- Document any new packages in the README or a dedicated dependencies section.
- Prefer Unity Package Manager packages over manual imports.

## Contact / reporting issues

- Use GitHub Issues for bug reports and feature requests.
- Tag issues with appropriate labels (bug, enhancement, etc.).