using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MicroRabbit.Transfer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService TransferService;

        public TransferController(ITransferService transferService)
        {
            TransferService = transferService;
        }

        // GET: api/Transfer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransferLog>>> Get()
        {
            return Ok(TransferService.GetTransferLogs());
        }
    }
}
