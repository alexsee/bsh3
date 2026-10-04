using System.Diagnostics;
using Alphaleonis.Win32.Vss;

namespace BSH.Service.VSS
{
    /// <summary>
    /// This class encapsulates some simple VSS logic.  Its goal is to allow
    /// a user to backup a single file from a shadow copy (presumably because
    /// that file is otherwise unavailable on its home volume).
    /// </summary>
    public class VssBackup : IDisposable
    {
        /// <summary>A reference to the VSS context.</summary>
        IVssBackupComponents _backup = null!;

        /// <summary>Some persistent context for the current snapshot.</summary>
        Snapshot? _snap;

        /// <summary>
        /// Constructs a VssBackup object and initializes some of the necessary
        /// VSS structures.
        /// </summary>
        public VssBackup()
        {
            InitializeBackup();
        }

        /// <summary>
        /// Sets up a shadow copy against the specified volume.
        /// </summary>
        /// <remarks>
        /// This methods is separated out from the constructor because if it
        /// throws, we still want the Dispose() method to be called.
        /// </remarks>
        /// <param name="volumeName">Name of the volume to copy.</param>
        public void Setup(string volumeName)
        {
            Discovery(volumeName);
            PreBackup();
        }

        /// <summary>
        /// The disposal of this object involves sending completion notices
        /// to the writers, removing the shadow copies from the system and
        /// finally releasing the BackupComponents object.  This method must
        /// be called when this class is no longer used.
        /// </summary>
        public void Dispose()
        {
            try { Complete(); } catch { }

            if (_snap != null)
            {
                _snap.Dispose();
                _snap = null;
            }

            _backup.Dispose();
        }

        /// <summary>
        /// This stage initializes both the requester (this program) and 
        /// any writers on the system in preparation for a backup and sets
        /// up a communcation channel between the two.
        /// </summary>
        void InitializeBackup()
        {
            // Here we are retrieving an OS-dependent object that encapsulates
            // all of the VSS functionality.  The OS indepdence that this single
            // factory method provides is one of AlphaVSS's major strengths!
            var vss = VssFactoryProvider.Default.GetVssFactory();

            // Now we create a BackupComponents object to manage the backup.
            // This object will have a one-to-one relationship with its backup
            // and must be cleaned up when the backup finishes (ie. it cannot
            // be reused).
            //
            // Note that this object is a member of our class, as it needs to
            // stick around for the full backup.
            _backup = vss.CreateVssBackupComponents();

            // Now we must initialize the components.  We can either start a
            // fresh backup by passing null here, or we could resume a previous
            // backup operation through an earlier use of the SaveXML method.
            _backup.InitializeForBackup(null);

            // At this point, we're supposed to establish communication with
            // the writers on the system.  It is possible before this step to
            // enable or disable specific writers via the BackupComponents'
            // Enable* and Disable* methods.
            _backup.GatherWriterMetadata();
        }

        /// <summary>
        /// This stage involes the requester (us, again) processing the
        /// information it received from writers on the system to find out
        /// which volumes - if any - must be shadow copied to perform a full
        /// backup.
        /// </summary>
        void Discovery(string fullPath)
        {
            // Once we are finished with the writer metadata, we can dispose
            // of it.
            _backup.FreeWriterMetadata();

            // Now we use our helper class to add the appropriate volume to the
            // shadow copy set.
            _snap = new Snapshot(_backup);
            var volumeRoot = Path.GetPathRoot(fullPath)
                ?? throw new ArgumentException($"Could not determine volume root for '{fullPath}'.", nameof(fullPath));
            _snap.AddVolume(volumeRoot);
        }

        /// <summary>
        /// This phase of the backup is focused around creating the shadow copy.
        /// We will notify writers of the impending snapshot, after which they
        /// have a short period of time to get their on-disk data in order and
        /// then quiesce writing.
        /// </summary>
        void PreBackup()
        {
            Debug.Assert(_snap != null);

            // This next bit is a way to tell writers just what sort of backup
            // they should be preparing for.  The important parts for us now
            // are the first and third arguments: we want to do a full backup.
            _backup.SetBackupState(false,
                  true, VssBackupType.Full, false);

            // From here we just need to send messages to each writer that our
            // snapshot is imminent,
            // We simply block while the writers to complete their background preparations.
            _backup.PrepareForBackup();

            // It's now time to create the snapshot.  Each writer will have to
            // freeze its I/O to the selected volumes for up to 10 seconds
            // while this process takes place.
            if (_snap == null)
            {
                throw new InvalidOperationException("VSS snapshot has not been created.");
            }

            _snap.Copy();
        }

        /// <summary>
        /// This simple method uses a bit of string manipulation to turn a
        /// full, local path into its corresponding snapshot path.  This
        /// method may help users perform full file copies from the snapsnot.
        /// </summary>
        /// <param name="localPath">The full path of the original file.</param>
        /// <returns>A full path to the same file on the snapshot.</returns>
        public string GetSnapshotPath(string localPath)
        {
            if (_snap == null)
            {
                throw new InvalidOperationException("VSS snapshot has not been set up. Call Setup() first.");
            }

            Trace.TraceInformation("New volume: " + _snap.Root);

            // This bit replaces the file's normal root information with root
            // info from our new shadow copy.
            if (Path.IsPathRooted(localPath))
            {
                string? root = Path.GetPathRoot(localPath);
                if (root != null)
                {
                    localPath = localPath.Replace(root, string.Empty);
                }
            }
            string slash = Path.DirectorySeparatorChar.ToString();
            if (!_snap.Root.EndsWith(slash) && !localPath.StartsWith(slash))
            {
                localPath = localPath.Insert(0, slash);
            }

            localPath = localPath.Insert(0, _snap.Root);

            Trace.TraceInformation("Converted path: " + localPath);

            return localPath;
        }

        /// <summary>
        /// The final phase of the backup involves some cleanup steps.
        /// We send the BackupComplete event to all of the writers.
        /// </summary>
        void Complete()
        {
            try
            {
                // The BackupComplete event must be sent to all of the writers.
                _backup.BackupComplete();
            }
            // Not sure why, but this throws a VSS_BAD_STATE on XP and W2K3.
            // Per some forum posts about this, I'm just ignoring it.
            catch (VssBadStateException) { }
        }
    }
}
