# Git handoff

The project package preserves the supplied `.git` history, its `main` branch and existing origin:

`https://github.com/EMGPSD/emgpsd-prog7312-group-2-2026-poe-part1-st10438564.git`

New commits describe actual changes made during this implementation. They use the coding agent identity, not invented student identities or backdated timestamps. The source has **not** been pushed. The origin name refers to Part 1, so confirm the lecturer's expected Part 2 repository before publishing.

## Inspect first (CMD from the extracted SmartX directory)

```bat
git status
git remote -v
git log --oneline -30
git rev-list --count HEAD
```

Do not initialise a new repository or delete `.git`; that would discard the preserved commit history. Keep this archive separate from an already-connected working copy until deciding which copy you will use.

## Publish a Part 2 branch to the existing origin, if that is correct

```bat
git switch -c part2-command-stream
git push -u origin part2-command-stream
```

GitHub may prompt you to authenticate. If the branch already exists, choose a new descriptive branch name or inspect it first. Do not force-push over someone else's work.

## If the lecturer requires a new repository

Create the empty repository in GitHub, copy its real HTTPS URL, then add it as another remote. Replace `YOUR_REAL_REPOSITORY_URL` before running:

```bat
git remote add part2 YOUR_REAL_REPOSITORY_URL
git push -u part2 main
```

This retains history without changing the original remote. A submission needs the repository link, not just local commits. Multiple-contributor evidence must come from genuine contributions; do not fabricate it. Review all source and documentation against your lecturer's rules on assistance and attribution.
