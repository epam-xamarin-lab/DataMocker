﻿// =========================================================================
// Copyright 2018 EPAM Systems, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =========================================================================
using System;
using System.IO;
using DataMocker.SharedModels;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using DataMocker.SharedModels.Resources;
using System.Collections.Generic;
using DataMocker.MockServer.Dto;

namespace DataMocker.MockServer.Controllers
{
    public class StorageController : Controller
    {
        private const string RootPath = @"DataMocker\samples\DataMockerDemoApp.Mock\Data";

        [Route("api/")]
        [HttpPost]
        public IActionResult Post([FromBody] MockRequest request)
        {
            var clientVersion = Request.Headers["user-agent"].ToString();

            var serverVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (clientVersion != serverVersion)
            {
                return BadRequest(new IncompalibleVersionsException(serverVersion, clientVersion));
            }
            var mockResource = new MockResource(ResourceStream(),request); 
            RequestToConsole(request);

            using (var stream = mockResource.ToStream())
            {
                if (stream == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(string.Format("File is not found: {0}\n\n", mockResource.RequestedResourcePath));
                    return NotFound(mockResource.RequestedResourcePath);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(string.Format("File: {0}\n\n", mockResource.RequestedResourcePath));

                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    Console.WriteLine($"Post return {result}");
                    reader.Close();
                    stream.Close();
                    return Ok(result);
                }
            }
        }

        [Route("api")]
        [HttpGet]
        public string Hello()
        {
            var list = new List<ItemDto>
            {
                new ItemDto{Id=1,Title="Audi A4", Text="Germany"},
                new ItemDto{Id=2,Title="Chevrolet Camaro", Text="USA"}
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(list);
        }

        private void RequestToConsole(MockRequest request)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(string.Format("RequestId: {0}\n", request.Hash));

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(string.Format("Test: {0}\n Method: {1}\n FileName: {2}\n",
                request.TestName,
                request.HttpMethod,
                request.FileName));
        }

        private IResourceStream ResourceStream()
        {
            return new FileResourceStream(RootPath);
        }
    }
}