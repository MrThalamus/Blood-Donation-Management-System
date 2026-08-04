---
description: Commit and push the current working tree to origin/main. No other tasks.
---

Push the current working tree to GitHub. Do exactly this, nothing more:

1. Run `git status` and `git diff` (staged + unstaged) to see what changed.
2. Stage the changes with `git add -A` — unless something looks like it shouldn't be committed (e.g. a secrets/credentials file), in which case stop and ask instead of staging it.
3. Write a concise, why-focused 1-2 sentence commit message matching the style of recent commits (`git log --oneline -5`), and commit.
4. Run `git push`.
5. Run `git status` to confirm the push succeeded, and report the result in one line.

Do not run tests, review the code, refactor anything, or ask clarifying questions about scope. This is a fixed, repeatable action — just commit and push. The only reasons to stop and surface something instead of pushing: the push is rejected/history has diverged, or something looks unsafe to commit (secrets, credentials).
