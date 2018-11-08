using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using KnotDiary.Common.Web.Models;
using Serilog;
using System;
using System.Collections.Generic;

namespace KnotDiary.AuthApi.Controllers
{
    [Route("status")]
    public class StatusController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger _logger;

        public StatusController(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [Route("configuration")]
        [HttpGet]
        public IActionResult Configuration()
        {
            var configStatus = new StatusConfiguration { StatusItems = new List<StatusItem>() };

            foreach (var keyValue in _configuration.AsEnumerable())
            {
                configStatus.StatusItems.Add(new StatusItem { Key = keyValue.Key, Value = keyValue.Value });
            }

            return Ok(configStatus);
        }

        [Route("log-test")]
        [HttpPost]
        public IActionResult LogTest(LogTest model)
        {
            try
            {
                _logger.Information(model.Message);
                return Ok(new { isSuccess = true });
            }
            catch (Exception ex)
            {
                return Ok(new { isSuccess = true, exception = ex });
            }
        }
    }
}
