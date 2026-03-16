# Contributing to SoulsHero

Thank you for contributing to SoulsHero. This document describes the development standards, coding conventions, and workflow expectations for this repository. All contributors (including maintainers) must follow these guidelines to keep the codebase consistent and high quality.

## Table of Contents

- Guidelines
- Development workflow
- Branching & pull request process
- Commit message style
- Coding standards (C# / Unity)
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

Adds a new HUD element that displays current stamina and max stamina.
```

## Coding standards (C# / Unity)

We follow common C# conventions and Unity best practices. Key rules:

- Use PascalCase for class, property, and method names.
- Use camelCase for private fields and parameters. Prefix private serialized fields with `_` if necessary: `_movementSpeed`.
- Avoid public fields unless they are intentionally exposed to the Unity Inspector. Prefer `[SerializeField] private` for inspector-exposed fields.
- Keep MonoBehaviour scripts focused on a single responsibility.
- Use `nameof(...)` for property/method names in logs and exceptions.
- Prefer composition over inheritance for gameplay systems.
- Avoid heavy logic in `Update()`; use events or coroutines when appropriate.

### Event and delegate usage

- Use UnityEvent for inspector-wired events. Keep event payloads small and explicit.
- Unsubscribe listeners in `OnDisable()` to avoid leaks and null-reference issues.

### Null checks

- Guard public API points with argument checks where appropriate.
- In Unity callbacks, check for null references before accessing components.

## Formatting and EditorConfig

This repository uses an `.editorconfig` to enforce formatting. If one is present in the root, configure your editor to respect it. Key preferences expected:

- indent_style = space
- indent_size = 4
- charset = utf-8
- end_of_line = lf
- insert_final_newline = true
- trim_trailing_whitespace = true

If an `.editorconfig` is not present, adhere to the above formatting rules.

## Unit tests & automated CI

- Add unit tests for core gameplay logic where practical (non-MonoBehaviour logic is easiest to test).
- Unit tests should be placed under `Assets/Tests` and use the Unity Test Runner.
- CI will run tests; ensure all tests pass locally before opening a PR.

## Code reviews

- Reviews focus on correctness, readability, and adherence to conventions.
- Suggest improvements with clear rationale.
- Approvals should be given only if the reviewer is confident in the change.

## Adding assets and third-party packages

- Prefer package manager (UPM) for dependencies when possible.
- Check licenses before adding third-party assets. Document the license and source in the PR description.
- Keep large binary changes (audio, textures) limited to separate PRs when possible.

## Reporting issues

Open an issue with a clear title and reproduction steps. Attach logs, stack traces, and screenshots where useful.

---

Thank you for contributing — your work helps make this project better for everyone.