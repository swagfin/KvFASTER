using FASTER.core;
using KvFASTER.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;

namespace KvFASTER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        public YetAnotherTestingFASTERKVService YetAnotherTestingFASTERKVService { get; }
        public ILogger<TestingController> Logger { get; }

        public TestingController(YetAnotherTestingFASTERKVService yetAnotherTestingFASTERKVService, ILogger<TestingController> logger)
        {
            YetAnotherTestingFASTERKVService = yetAnotherTestingFASTERKVService;
            Logger = logger;
        }


        [HttpGet("GetStoredCollection")]
        public IActionResult GetStoredCollection(bool includeData = false)
        {
            using (var session = YetAnotherTestingFASTERKVService.CarsFasterKV.NewSession(new SimpleFunctions<string, long>()))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var iterator = session.Iterate();
                Dictionary<string, long> allReg = new Dictionary<string, long>();
                while (iterator.GetNext(out RecordInfo recordInfo, out string _key, out long _value))
                {
                    allReg.Add(_key, _value);
                }
                stopwatch.Stop();
                var responseData = new DataRetrievedResponse
                {
                    RecordsCount = allReg.Count,
                    ElaspedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Data = includeData ? allReg : null
                };
                return new ObjectResult(responseData);
            }

        }

        [HttpGet("GetStoredCollection/{key}")]
        public IActionResult GetStoredCollectionByKey(string key)
        {
            using (var session = YetAnotherTestingFASTERKVService.CarsFasterKV.NewSession(new SimpleFunctions<string, long>()))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                long _value = 0;
                var status = session.Read(key, out _value);
                var responseData = new DataRetrievedResponse
                {
                    RecordsCount = _value > 0 ? 1 : 0,
                    ElaspedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Data = _value > 0 ? $"KEY: {key} | VALUE: {_value}" : null
                };
                return new ObjectResult(responseData);
            }

        }

        [HttpGet("CreateDemoCollection")]
        public IActionResult GetCreateDemoCollection(int count = 1000)
        {
            //Check
            count = (count < 1) ? 0 : count;
            using (var session = YetAnotherTestingFASTERKVService.CarsFasterKV.NewSession(new SimpleFunctions<string, long>()))
            {
                this.Logger.LogInformation("Creating Demo Collection.....");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int indexAt = 1;
                while (count > 0)
                {
                    session.Upsert($"key_id_{indexAt}", indexAt);
                    count--;
                    indexAt++;
                }
                stopwatch.Stop();
                this.Logger.LogInformation($"Creating Demo Collection Completed in: {stopwatch.ElapsedMilliseconds:N0}");
                //Response
                return new OkObjectResult($"completed in milliseconds: {stopwatch.ElapsedMilliseconds}");
            }
        }

        [HttpGet("RestoreCheckpoint")]
        public IActionResult GetRestoreCheckpoint()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            YetAnotherTestingFASTERKVService.AttemptRestoreFromLastCheckpoint();
            stopwatch.Stop();
            return new OkObjectResult($"completed in milliseconds: {stopwatch.ElapsedMilliseconds}");
        }

        [HttpGet("CreateCheckpoint")]
        public IActionResult GetCreateCheckpoint()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            YetAnotherTestingFASTERKVService.AttemptCreateLatestCheckpoint();
            stopwatch.Stop();
            return new OkObjectResult($"completed in milliseconds: {stopwatch.ElapsedMilliseconds}");
        }

        [HttpGet("ResetEverything")]
        public IActionResult GetResetEverything()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            YetAnotherTestingFASTERKVService.ResetAndDisposeEverything();
            stopwatch.Stop();
            return new OkObjectResult($"completed in milliseconds: {stopwatch.ElapsedMilliseconds}");
        }
    }

    class DataRetrievedResponse
    {
        public int RecordsCount { get; set; }
        public long ElaspedMilliseconds { get; set; }
        public object Data { get; set; }
    }
}
