using FASTER.core;
using KvFASTER.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KvFASTER.Services
{
    public class SharedFASTERKVService : IDisposable
    {
        private readonly ILogger<SharedFASTERKVService> _logger;

        public FasterKV<string, long> CarsFasterKV { get; set; }
        public FasterKV<Guid, CarRegistration> CarsCollectionFasterKV { get; set; }

        public SharedFASTERKVService(ILogger<SharedFASTERKVService> logger)
        {
            this._logger = logger;
        }

        internal void InitializeService()
        {

            this._logger.LogInformation("Starting Service....");
            var storagePath = Path.GetTempPath() + "\\KvFASTER\\";
            var log = Devices.CreateLogDevice(storagePath + "hlog.log");
            var objLog = Devices.CreateLogDevice(storagePath + "hlog.obj.log");
            //For Checkpoint or Call it Restore Points
            var logCheckPointManager = new DeviceLogCommitCheckpointManager(new LocalStorageNamedDeviceFactory(), new DefaultCheckpointNamingScheme(storagePath + "\\Checkpoints\\"));

            this._logger.LogInformation("Creating Cars Collection KV....");
            this.CarsCollectionFasterKV = new FasterKV<Guid, CarRegistration>(
             size: 1L << 20, // 1M cache lines of 64 bytes each = 64MB hash table
             logSettings: new LogSettings { LogDevice = log, ObjectLogDevice = objLog }, // specify log settings (e.g., size of log in memory)
             checkpointSettings: new CheckpointSettings { CheckpointManager = logCheckPointManager }
             );


            var log2 = Devices.CreateLogDevice(storagePath + "hlog2.log");
            var objLog2 = Devices.CreateLogDevice(storagePath + "hlog2.obj.log");

            this.CarsFasterKV = new FasterKV<string, long>(
             size: 1L << 20, // 1M cache lines of 64 bytes each = 64MB hash table
             logSettings: new LogSettings { LogDevice = log2, ObjectLogDevice = objLog2 }, // specify log settings (e.g., size of log in memory)
             checkpointSettings: new CheckpointSettings { CheckpointManager = logCheckPointManager }
             );

            this._logger.LogInformation("Service started.....");
            //Attempt to Resume Session
            AttemptRestoreFromLastCheckpoint();
            //Issue Periodic Checkpoints Automatically
            IssuePeriodicCheckpoints();
        }

        private void AttemptRestoreFromLastCheckpoint()
        {
            try
            {
                //Check points restore
                this._logger.LogInformation("Restoring last checkpoints imagery....");
                // Recover store from latest checkpoint
                CarsCollectionFasterKV.Recover();
                CarsFasterKV.Recover();
                this._logger.LogInformation("Restoring last checkpoints imagery....DONE");
            }
            catch (Exception)
            {
                this._logger.LogInformation("Restoring last checkpoints imagery....(NO RECOVERY IMAGES)");
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
                    this._logger.LogInformation("Creating Restore Checkpoint....");
                    (_, _) = CarsCollectionFasterKV.TakeHybridLogCheckpointAsync(CheckpointType.FoldOver).GetAwaiter().GetResult();
                    (_, _) = CarsFasterKV.TakeHybridLogCheckpointAsync(CheckpointType.FoldOver).GetAwaiter().GetResult();
                    this._logger.LogInformation("Creating Restore Checkpoint....DONE");
                }
            });
            t.Start();
        }

        public Task TerminateService(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Service stopping.....");
            // Purge cloud log files
            this._logger.LogInformation("Service stopped.....");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Dispose store
            CarsCollectionFasterKV.Dispose();
            // Close devices
            CarsFasterKV.Dispose();
        }
    }
}
