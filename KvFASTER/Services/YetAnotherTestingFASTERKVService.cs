using FASTER.core;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;

namespace KvFASTER.Services
{
    public class YetAnotherTestingFASTERKVService : IDisposable
    {
        private readonly ILogger<YetAnotherTestingFASTERKVService> _logger;
        private string _storagePath;

        public FasterKV<Guid, Student> DataStoreKV { get; set; }

        public YetAnotherTestingFASTERKVService(ILogger<YetAnotherTestingFASTERKVService> logger)
        {
            this._logger = logger;
        }

        internal void InitializeService()
        {

            this._logger.LogInformation("Starting Service....");
            _storagePath = Path.GetTempPath() + "\\KvFASTER-TESTING\\";
            var log = Devices.CreateLogDevice(_storagePath + "hlog.log");
            var objLog = Devices.CreateLogDevice(_storagePath + "hlog.obj.log");
            //For Checkpoint or Call it Restore Points
            var logCheckPointManager = new DeviceLogCommitCheckpointManager(new LocalStorageNamedDeviceFactory(), new DefaultCheckpointNamingScheme(_storagePath + "\\Checkpoints\\"));

            this.DataStoreKV = new FasterKV<Guid, Student>(
             size: 1L << 20, // 1M cache lines of 64 bytes each = 64MB hash table
             logSettings: new LogSettings { LogDevice = log, ObjectLogDevice = objLog }, // specify log settings (e.g., size of log in memory)
             checkpointSettings: new CheckpointSettings { CheckpointManager = logCheckPointManager }
             );

            this._logger.LogInformation("Service started.....");
        }

        public void AttemptRestoreFromLastCheckpoint()
        {
            try
            {
                //Check points restore
                this._logger.LogInformation("Restoring last checkpoints imagery....");
                // Recover store from latest checkpoint
                DataStoreKV.Recover();
                DataStoreKV.Recover();
                this._logger.LogInformation("Restoring last checkpoints imagery....DONE");
            }
            catch (Exception)
            {
                this._logger.LogInformation("Restoring last checkpoints imagery....(NO RECOVERY IMAGES)");
            }
        }
        public void AttemptCreateLatestCheckpoint()
        {
            try
            {
                //Check points restore
                this._logger.LogInformation("Creating Restore Checkpoint....");
                (_, _) = DataStoreKV.TakeHybridLogCheckpointAsync(CheckpointType.FoldOver).GetAwaiter().GetResult();
                this._logger.LogInformation("Creating Restore Checkpoint....DONE");
            }
            catch (Exception ex)
            {
                this._logger.LogInformation("Creating Restore Checkpoint Failed: {ERROR}", ex.Message);
            }
        }

        private void IssuePeriodicCheckpoints()
        {
            var t = new Thread(() =>
            {
                while (true)
                {
                    //On Every 10 Seconds
                    Thread.Sleep(10000);
                    AttemptCreateLatestCheckpoint();
                }
            });
            t.Start();
        }


        public void Dispose()
        {
            // Dispose store
            // Close devices
            try { DataStoreKV.Dispose(); } catch { }
        }

        internal void ResetAndDisposeEverything()
        {
            _logger.LogWarning("Reseting Everything...");
            Dispose();
            if (!string.IsNullOrWhiteSpace(_storagePath) && Directory.Exists(_storagePath))
                Directory.Delete(_storagePath, true);
            //Re-Initializing
            _logger.LogWarning("Reseting Everything...Completed");
            InitializeService();

        }
    }

    public class Student
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string StudentName { get; set; } = string.Empty;
        public double Marks { get; set; } = 0;
        public DateTime RegistrationDate { get; set; }
        public string StudentRegNo { get; set; } = "REG-" + new Random().Next(1000, 9999).ToString();
    }
}
