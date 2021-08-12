using FASTER.core;
using KvFASTER.Models;
using KvFASTER.Models.Request;
using KvFASTER.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace KvFASTER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ILogger<CarsController> logger;
        public SharedFASTERKVService SharedKv { get; }

        public CarsController(ILogger<CarsController> logger, SharedFASTERKVService sharedKv)
        {
            this.logger = logger;
            SharedKv = sharedKv;
        }

        [HttpGet]
        public IActionResult Get()
        {
            using (var s = SharedKv.CarsCollectionFasterKV.NewSession(new SimpleFunctions<Guid, CarRegistration>()))
            {
                var iterator = s.Iterate();
                List<CarRegistration> allReg = new List<CarRegistration>();
                while (iterator.GetNext(out RecordInfo recordInfo, out Guid key, out CarRegistration value))
                {
                    allReg.Add(value);
                }
                return new ObjectResult(allReg);
            }
        }


        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            using (var s = SharedKv.CarsCollectionFasterKV.NewSession(new SimpleFunctions<Guid, CarRegistration>()))
            {
                //Try And Retrive it Now
                var _key = id;
                CarRegistration _output = null;
                var status = s.Read(ref _key, ref _output);
                return new ObjectResult(_output);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] CarRegistrationRequest request)
        {
            //Mapping here
            CarRegistration model = new CarRegistration { Model = request.Model, Price = request.Price, Color = request.Color };
            //Pass to Content Delivery
            //RWM If already presebt it wunt be added || see TryAddFunctions
            // using (var tryAddSession = sharedKv.CarsCollectionFasterKV.NewSession(new TryAddFunctions<Guid, CarRegistration>()))
            //Using Simple Functions
            using (var s = SharedKv.CarsCollectionFasterKV.NewSession(new SimpleFunctions<Guid, CarRegistration>()))
            {
                //Save to Storage
                var _key = model.Id;
                var _value = model;
                s.Upsert(ref _key, ref _value);

                //Try And Retrive it Now
                CarRegistration _output = null;
                var status = s.Read(ref _key, ref _output);

                //Lets Print both Status and Output
                Console.WriteLine($"Retrived from Storage, STATUS: {status}, OBJECT: {_output}");
                //Respond Back with what was Logged
                return new OkObjectResult(_output);
            }
        }


    }
}
