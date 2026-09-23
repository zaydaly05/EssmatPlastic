# Automatic desktop updates

The desktop application checks its local API at startup. The local API reads the
latest release metadata from Neon, and the application downloads the Windows ZIP
from the public GitHub Releases page when the user accepts an update.

## One-time repository setup

Add a repository Actions secret named `NEON_DATABASE_URL`. Set its value to the
Neon PostgreSQL connection string in Npgsql format (`Host=...;Port=...;...`).
The publisher workflow uses it to write the latest version and release URL to
the `AppUpdates` table. The release ZIP deliberately excludes API appsettings so
it does not distribute the per-PC database configuration.

## Publishing a release

Push code to `main`, or run **Publish desktop updates** from GitHub Actions. The
workflow builds the desktop app and API, creates a versioned public GitHub
Release, then records that release in Neon. Each configured PC checks Neon the
next time the app starts and offers the update.

## Existing installations

Install the first updater-enabled release manually on each existing PC. Older
1.1.0 binaries do not contain an update checker and cannot install it themselves.
The updater overlays program files while preserving the PC's local `appsettings`
and SQLite database files.
