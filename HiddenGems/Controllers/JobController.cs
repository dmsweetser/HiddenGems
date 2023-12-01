using HiddenGems.Business;
using HiddenGems.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static HiddenGems.Common.JobRequestManager;

namespace HiddenGems.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : Controller
    {
        private ILogger<JobController> _logger;
        public JobController(ILogger<JobController> logger)
        {
            _logger = logger;
        }

        [HttpPost("UploadDataFile")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadDataFile([FromForm] IFormFile dataToUpload)
        {
            return await Task.Run(() =>
            {
                var CurrentJobRequest = GetOrAddJobRequest(HttpContext.Session.Id);
                CurrentJobRequest.Status.StatusMessage = "";

                try
                {
                    var dataStream = dataToUpload.OpenReadStream();
                    var incomingFileName = dataToUpload.FileName;
                    JobControllerHelpers.UploadData(dataStream, incomingFileName, CurrentJobRequest);
                    return new JsonResult(CurrentJobRequest.Data.EligibleColumns);
                }
                catch
                {
                    return new JsonResult("");
                }
            });
        }

        [HttpPost("UploadDataUrl")]
        public async Task<IActionResult> UploadDataUrl([FromForm] string dataToUpload)
        {
            return await Task.Run(() =>
            {
                var CurrentJobRequest = GetOrAddJobRequest(HttpContext.Session.Id);
                CurrentJobRequest.Status.StatusMessage = "";

                try
                {
                    JobControllerHelpers.UploadDataUrl(dataToUpload, CurrentJobRequest);
                    return new JsonResult(CurrentJobRequest.Data.EligibleColumns);
                }
                catch
                {
                    return new JsonResult("");
                }
            });
        }

        [HttpPost("SubmitRequest")]
        public bool SubmitRequest([FromForm] List<string> selectedColumns)
        {
            var CurrentJobRequest = GetExistingJobRequest(HttpContext.Session.Id);
            CurrentJobRequest.Status.StatusMessage = "";
            return JobControllerHelpers.SubmitRequest(CurrentJobRequest, selectedColumns);
        }

        [HttpGet("CheckStatus")]
        public async Task<JobStatus> CheckStatus()
        {
            return await Task.Run(() =>
            {
                return JobControllerHelpers.CheckStatus(HttpContext.Session.Id, _logger);
            });
        }

        [HttpGet("GetResult")]
        public async Task<JobResultDTO> GetResult()
        {
            return await Task.Run(() =>
            {
                return JobControllerHelpers.GetResult(HttpContext.Session.Id, _logger);
            });
        }

        [HttpPost("Evaluate")]
        public async Task<IActionResult> Evaluate([FromForm] string recordToAnalyze)
        {
            return await Task.Run(() =>
            {
                var parsedRecordToAnalyze = JsonConvert.DeserializeObject<Dictionary<string, string>>(recordToAnalyze);
                return new JsonResult(JobControllerHelpers.Evaluate(HttpContext.Session.Id, parsedRecordToAnalyze, _logger));
            });
        }

        [HttpDelete("Cancel")]
        public async Task<bool> CancelJob()
        {
            return await Task.Run(() =>
            {
                return JobControllerHelpers.CancelJob(HttpContext.Session.Id, _logger);
            });
        }

        [HttpPost("Activate")]
        public bool Activate([FromForm] string activationKey)
        {
            return JobControllerHelpers.Activate(activationKey);
        }

        [HttpPost("ResendEmail")]
        public bool ResendEmail([FromForm] string emailAddress)
        {
            return JobControllerHelpers.ResendEmail(emailAddress);
        }
    }
}
