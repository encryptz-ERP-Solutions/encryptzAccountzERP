using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Admin.DTOs;

namespace BusinessLogic.Admin.Interface
{
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all users.
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves a user by their unique ID.
        /// </summary>
        Task<UserDto?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a user by their user handle.
        /// </summary>
        Task<UserDto?> GetUserByUserHandleAsync(string userHandle);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        Task<UserDto?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="userCreateDto">The user creation data.</param>
        /// <returns>The newly created user DTO.</returns>
        Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="userUpdateDto">The user update data.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        Task<bool> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto);

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        Task<bool> DeleteUserAsync(Guid id);
    }
}