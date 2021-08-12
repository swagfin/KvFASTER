using FASTER.core;
using KvFASTER.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
            using (var session = YetAnotherTestingFASTERKVService.DataStoreKV.NewSession(new SimpleFunctions<Guid, Student>()))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var iterator = session.Iterate();
                List<Student> allReg = new List<Student>();
                while (iterator.GetNext(out RecordInfo recordInfo, out Guid _key, out Student _value))
                {
                    allReg.Add(_value);
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
        public IActionResult GetStoredCollectionByKey(Guid key)
        {
            using (var session = YetAnotherTestingFASTERKVService.DataStoreKV.NewSession(new SimpleFunctions<Guid, Student>()))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Student _value = null;
                var status = session.Read(ref key, ref _value);
                var responseData = new DataRetrievedResponse
                {
                    RecordsCount = _value != null ? 1 : 0,
                    ElaspedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Data = new List<Student>()
                };
                responseData.Data.Add(_value);
                return new ObjectResult(responseData);
            }

        }

        [HttpGet("CreateDemoCollection")]
        public IActionResult GetCreateDemoCollection(int count = 1000)
        {
            //Check
            count = (count < 1) ? 0 : count;
            using (var session = YetAnotherTestingFASTERKVService.DataStoreKV.NewSession(new SimpleFunctions<Guid, Student>()))
            {
                this.Logger.LogInformation("Creating Demo Collection.....");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int indexAt = 1;
                while (count > 0)
                {
                    Guid studentKey = Guid.NewGuid();
                    Student newStudent = new Student
                    {
                        Id = studentKey,
                        StudentName = $"STUDENT-NO-{indexAt:N2}",
                        Marks = new Random().Next(100),
                        RegistrationDate = DateTime.Now
                    };
                    session.Upsert(ref studentKey, ref newStudent);
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
        public List<Student> Data { get; set; } = new List<Student>();
    }
}
