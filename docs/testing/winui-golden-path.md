# WinUI golden-path QA

Click-through checklist for an installed `BSH.MainApp` beta candidate. Installation and
upgrade evidence are release gates; uninstall behavior is covered by the rollback rehearsal.

English UI labels are shown below. Run the same checklist once in German across the
candidate machine matrix. Record results in `docs/testing/beta-release-signoff.md`.

Companion fixture: `docs/testing/New-WinUiGoldenPathFixture.ps1`.

## Prepare

1. Install the immutable `backupservicehome-*-winui-win64.exe` candidate. Record its
   filename and SHA-256. On a fresh-install machine, confirm it starts `BSH.MainApp` and
   `Get-Service 'Backup Service Home-Dienst'` reports `Running`.

2. Open **Extras and Support → About** and confirm the displayed version includes the
   same `-betaN` identifier as the installer and release.

3. Run the fixture:

   ```powershell
   powershell -NoProfile -File docs/testing/New-WinUiGoldenPathFixture.ps1
   ```

   Default source: `%TEMP%\bsh-winui-golden-path\source`  
   Restore-to folder: `%TEMP%\bsh-winui-golden-path\restore-to`

4. On the fresh-install pass, continue directly to setup. On a reset-only repeat pass,
   use **Extras and Support → Reset Configuration** and confirm that the setup wizard appears.
   A reset-only result does not satisfy the fresh-install or upgrade gates.

5. Local target is a **drive**, stored as `X:\Backups\{Computer}\{User}`. Pick a drive that does **not** already have that folder (USB is fine). The wizard rejects an existing backup folder.

## Checklist

Pass/fail each step. Stop on the first failure.

### 1. Setup wizard

- [ ] Welcome: **Start a new backup setup → Continue** (do not Import).
- [ ] **Choose what to back up:** **Add a folder to back up** → select the fixture `source` folder. **Next**.
- [ ] **Where should backups be stored?** **This PC or drive** → select the empty test drive. **Next**.
- [ ] **Choose backup mode:** **Manual backups**. **Next** (opens Settings so compression can be set before the first backup).

Expected: shell navigation is enabled; Settings opens; no first backup has run yet.

### 2. Compression

Default is no compression. For the compressed pass:

- [ ] Settings gear → **Backup Options** → **Compress backups**.
- [ ] Leave encryption off.

An uncompressed pass is enough for the first internal dogfood run. A compressed pass is
required before public beta. Encryption is covered by `beta-risk-scenarios.md`.

### 3. First backup

- [ ] **Overview → Start Backup**.
- [ ] In **Create Backup**, leave **Create full backup** unchecked. **Create Backup**.
- [ ] Status finishes without error. Overview shows **1** backup and the fixture file count.

### 4. Incremental

- [ ] Edit `%TEMP%\bsh-winui-golden-path\source\notes.txt` to `notes-v2` and save.
- [ ] **Overview → Start Backup** again (not a full backup).

Expected: **2** backups. First version still listed in the browser.

### 5. Browse, search, favorite, preview

- [ ] **Backup browser**. Two versions in **Backups**; select the newest.
- [ ] Open the source folder; `notes.txt`, `drop.txt`, `nested`, `empty`, `unicode` are visible. `empty` is present.
- [ ] Search box: `notes` → `notes.txt` is listed.
- [ ] Clear search. Select `nested` → **… → Add to folder favorites**. Favorite appears in the pane.
- [ ] Select `notes.txt` → **Quick preview**. Content is `notes-v2`.
- [ ] Open `unicode` → `äöü` → `файл.txt`. Preview still works.
- [ ] Use previous/next version navigation while viewing `notes.txt`; content and selected version remain synchronized.

### 6. Restore file and folder

- [ ] Select `notes.txt` on the newest version → **Restore** (original path). Overwrite if asked. File on disk is `notes-v2`.
- [ ] **Restore to…** → pick `restore-to`. `notes.txt` appears there with `notes-v2`.
- [ ] Navigate into `nested` → **… → Restore all to…** → same `restore-to` folder (or a subfolder). `readme.txt` content is `nested-readme`.

### 7. Delete a version and a file

- [ ] Select the **older** version → **… → Delete backup**. Confirm.
- [ ] One version remains. Opening it still shows `notes.txt` as `notes-v2`.
- [ ] Select `drop.txt` → **… → Delete selected from all backups**. Confirm.
- [ ] `drop.txt` is gone from the remaining version. `notes.txt` and `nested` remain.
- [ ] Restore the remaining version **Restore all to…** `restore-to`. `drop.txt` is not restored; other files are.

## Done when

All boxes pass on one installed, uncompressed run. Before public beta, also complete a
compressed run, the German UI pass, the stable-to-beta upgrade in
`beta-risk-scenarios.md`, and the release matrix in `beta-release-signoff.md`.

Failures: note the step, screenshot, and attach `%AppData%\Alexosoft\Backup Service Home 3\log{yyyyMMdd}.txt` (**Extras and Support → Event Logs**).
