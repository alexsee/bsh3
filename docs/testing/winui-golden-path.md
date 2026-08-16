# WinUI golden-path QA

Click-through checklist for `BSH.MainApp`. Installer and upgrade/uninstall are out of scope here.

English UI labels. Use a debug/F5 build or an already-installed WinUI build.

Companion fixture: `docs/testing/New-WinUiGoldenPathFixture.ps1`.

## Prepare

1. Run the fixture:

   ```powershell
   powershell -NoProfile -File docs/testing/New-WinUiGoldenPathFixture.ps1
   ```

   Default source: `%TEMP%\bsh-winui-golden-path\source`  
   Restore-to folder: `%TEMP%\bsh-winui-golden-path\restore-to`

2. If the app already has a configuration: **Extras and Support → Reset Configuration**. Confirm. The setup wizard should appear.

3. Local target is a **drive**, stored as `X:\Backups\{Computer}\{User}`. Pick a drive that does **not** already have that folder (USB is fine). The wizard rejects an existing backup folder.

## Checklist

Pass/fail each step. Stop on the first failure.

### 1. Setup wizard

- [ ] Welcome: **Start a new backup setup → Continue** (do not Import).
- [ ] **Choose what to back up:** **Add a folder to back up** → select the fixture `source` folder. **Next**.
- [ ] **Where should backups be stored?** **This PC or drive** → select the empty test drive. **Next**.
- [ ] **Choose backup mode:** **Manual backups**. **Next** (opens Settings so compression can be set before the first backup).

Expected: shell navigation is enabled; Settings opens; no first backup has run yet.

### 2. Compression (optional pass — do both over two runs if time allows)

Default is no compression. For the compressed pass:

- [ ] Settings gear → **Backup Options** → **Compress backups**.
- [ ] Leave encryption off.

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

All boxes pass on one uncompressed run. Compressed run (step 2) is strongly recommended before public beta, but not required to start dogfood.

Failures: note the step, screenshot, and attach `%AppData%\Alexosoft\Backup Service Home 3\log{yyyyMMdd}.txt` (**Extras and Support → Event Logs**).
