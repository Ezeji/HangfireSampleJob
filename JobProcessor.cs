using HangfireSample.Services.Message.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellaHealthCoreData.Models.Insurance;
using WellaHealthCoreRepository.Interfaces;

namespace HangfireSample
{
    public class JobProcessor
    {
        private readonly IMessageService _service;

        private readonly ILogger<JobProcessor> _logger;

        public JobProcessor(IMessageService service, ILogger<JobProcessor> logger)
        {
            _service = service;
            _logger = logger;
        }

        //main method
        public async Task ProcessJobsAsync()
        {
            try
            {
                await _service.WelcomeMessage();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:{ex.Message}");
            }
        }

    }
}
