using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Core.Interface;
using Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanManageBusinesses")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IUserBusinessService _userBusinessService;
        private readonly IUserBusinessRoleService _userBusinessRoleService;
        private readonly IRoleRepository _roleRepository;
        private readonly ExceptionHandler _exceptionHandler;

        public BusinessController(
            IBusinessService businessService,
            IUserBusinessService userBusinessService,
            IUserBusinessRoleService userBusinessRoleService,
            IRoleRepository roleRepository,
            ExceptionHandler exceptionHandler)
        {
            _businessService = businessService;
            _userBusinessService = userBusinessService;
            _userBusinessRoleService = userBusinessRoleService;
            _roleRepository = roleRepository;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessDto>>> GetAll()
        {
            try
            {
                var result = await _businessService.GetAllBusinessesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessDto>> GetById(Guid id)
        {
            try
            {
                var result = await _businessService.GetBusinessByIdAsync(id);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(BusinessDto businessDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized("User ID claim is missing or invalid.");
                }

                var newBusiness = await _businessService.AddBusinessAsync(businessDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = newBusiness.BusinessID }, newBusiness);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, BusinessDto businessDto)
        {
            try
            {
                if (id != businessDto.BusinessID)
                {
                    return BadRequest("Business ID in the URL does not match the ID in the request body.");
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized("User ID claim is missing or invalid.");
                }

                var success = await _businessService.UpdateBusinessAsync(id, businessDto, userId);
                if (!success)
                    return NotFound(new { message = "Business not found or update failed." });

                return Ok(new { message = "Business updated successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _businessService.DeleteBusinessAsync(id);
                if (!success)
                    return NotFound(new { message = "Business not found or could not be deleted." });

                return Ok(new { message = "Business deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Allows authenticated users to create their own business.
        /// This endpoint does not require CanManageBusinesses permission.
        /// It automatically links the user to the business and assigns the Business Owner role.
        /// </summary>
        [HttpPost("my-business")]
        [Authorize] // Only requires authentication, not CanManageBusinesses permission
        public async Task<IActionResult> CreateMyBusiness(BusinessDto businessDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized("User ID claim is missing or invalid.");
                }

                // Create the business
                var newBusiness = await _businessService.AddBusinessAsync(businessDto, userId);

                // Check if this is the user's first business (to set as default)
                var existingBusinesses = await _userBusinessService.GetByUserIdAsync(userId);
                var isFirstBusiness = !existingBusinesses.Any();

                // Link the user to the business
                var userBusinessLink = await _userBusinessService.CreateAsync(new CreateUserBusinessRequest
                {
                    UserID = userId,
                    BusinessID = newBusiness.BusinessID,
                    IsDefault = isFirstBusiness
                }, userId);

                // Assign Business Owner role to the user
                var ownerRoleId = await _roleRepository.GetRoleIdByNameAsync("Business Owner")
                    ?? await _roleRepository.GetRoleIdByNameAsync("Admin");

                if (ownerRoleId.HasValue)
                {
                    await _userBusinessRoleService.AddAsync(new UserBusinessRoleDto
                    {
                        UserID = userId,
                        BusinessID = newBusiness.BusinessID,
                        RoleID = ownerRoleId.Value
                    });
                }

                return CreatedAtAction(nameof(GetById), new { id = newBusiness.BusinessID }, newBusiness);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}