# GitHub Pages branch-mode fallback

This `docs/` folder mirrors the contents of `web-prototype/` so that GitHub Pages can serve the prototype when Pages is configured as:

- **Deploy from a branch**
- Branch: `master` (or `main`)
- Folder: `/docs`

If you use the Actions-based deployment (`.github/workflows/deploy-web-prototype.yml`), set Pages source to **GitHub Actions** instead.
