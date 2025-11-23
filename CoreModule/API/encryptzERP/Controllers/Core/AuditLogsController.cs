using System;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace encryptzERP.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ExceptionHandler _exceptionHandler;

        public AuditLogsController(IAuditLogService auditLogService, ExceptionHandler exceptionHandler)
        {
            _auditLogService = auditLogService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] AuditLogFilterDto filter)
        {
            try
            {
                var logs = await _auditLogService.GetAuditLogsAsync(filter ?? new AuditLogFilterDto());
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "Failed to load audit logs." });
            }
        }
    }
}

