using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using SwiftPay.Services.Interfaces;
using SwiftPay.Models;
using SwiftPay.DTOs.UserRoleDTO;

namespace SwiftPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _service;
        private readonly IUserRoleService _userRoleService;
        private readonly IUserService _userService;

        public RolesController(IRoleService service, IUserRoleService userRoleService, IUserService userService) => (_service, _userRoleService, _userService) = (service, userRoleService, userService);

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="dto">Role creation data</param>
        /// <returns>Created role object</returns>
        /// <response code="200">Role created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(Role), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequestDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return Ok(new { message = "Role created successfully.", data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the role.", error = ex.Message });
            }
        }
        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>Role object</returns>
        /// <response code="200">Role found</response>
        /// <response code="404">Role not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("{roleId}")]
        [ProducesResponseType(typeof(Role), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int roleId)
        {
            try
            {
                var role = await _service.GetByIdAsync(roleId);
                if (role == null)
                    return NotFound(new { message = $"Role with ID {roleId} not found." });

                return Ok(new { message = "Role retrieved successfully.", data = role });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the role.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        /// <returns>List of all active roles with count</returns>
        /// <response code="200">Roles retrieved successfully</response>
        /// <response code="500">Server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(RoleListResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _service.GetAllAsync();
                var roleList = roles.ToList();
                var response = new RoleListResponseDto
                {
                    Roles = roleList,
                    TotalCount = roleList.Count
                };
                return Ok(new { message = "Roles retrieved successfully.", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving roles.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete (soft delete) a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>Success or failure message</returns>
        /// <response code="200">Role deleted successfully</response>
        /// <response code="404">Role not found</response>
        /// <response code="500">Server error</response>
        [HttpDelete("{roleId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int roleId)
        {
            try
            {
                var result = await _service.DeleteAsync(roleId);
                if (!result)
                    return NotFound(new { message = $"Role with ID {roleId} not found." });

                return Ok(new { message = "Role deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the role.", error = ex.Message });
            }
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">Role assignment data</param>
        /// <returns>Assigned user role DTO</returns>
        /// <response code="200">Role assigned successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="409">Business conflict (e.g., user already has role)</response>
        /// <response code="500">Server error</response>
        [HttpPost("users/{userId}/roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserRoleResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignRoleToUser(int userId, [FromBody] CreateUserRoleRequestDto dto)
        {
            try
            {
                var assigned = await _userRoleService.AssignRoleToUserAsync(userId, dto);
                return Ok(new { message = "Role assigned to user successfully.", data = assigned });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning the role.", error = ex.Message });
            }
        }

        /// <summary>
        /// Admin-only: create a staff user and assign a role in one request
        /// </summary>
        [HttpPost("staff")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
        {
            try
            {
                // Create user (UserService will hash password and perform validations)
                var createUserDto = new SwiftPay.DTOs.UserCustomerDTO.CreateUserDto
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Password = dto.Password
                };

                var createdUser = await _userService.CreateAsync(createUserDto);

                // Assign requested role to newly created user
                var assignDto = new CreateUserRoleRequestDto { RoleId = dto.RoleId };
                var assigned = await _userRoleService.AssignRoleToUserAsync(createdUser.UserId, assignDto);

                return Ok(new { message = "Staff user created and role assigned.", data = new { user = createdUser, userRole = assigned } });
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating staff user.", error = ex.Message });
            }
        }
    }
}
