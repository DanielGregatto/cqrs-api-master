using Domain.DTO.Infrastructure.API;
using Domain.DTO.Responses;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Features.LogError.Commands.CreateLogError;
using Services.Features.LogError.Commands.DeleteLogError;
using Services.Features.LogError.Commands.SaveError;
using Services.Features.LogError.Commands.UpdateLogError;
using Services.Features.LogError.Queries.GetAllLogErrors;
using Services.Features.LogError.Queries.GetLogErrorById;
using Services.Features.LogError.Queries.SearchLogErrorsPaginated;
using UI.API.Controllers.Base;

namespace UI.API.Controllers
{
    public class LogErrorController : CoreController
    {
        private readonly IMediatorHandler _mediator;
        private readonly IUser _user;

        public LogErrorController(IMediatorHandler mediator, IUser user)
        {
            _mediator = mediator;
            _user = user;
        }

        /// <summary>
        /// Retrieves all log error entries.
        /// </summary>
        /// <remarks>Requires authentication. Returns a collection of log error data transfer objects if
        /// successful. The response may include error information if the request is unauthorized, forbidden, or if no
        /// log errors are found.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with an <see
        /// cref="IEnumerable{T}"/> of <see cref="LogErrorDto"/> objects if successful; otherwise, an <see
        /// cref="ErrorResponseDto"/> describing the error.</returns>
        [HttpGet("v1/log-error")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<LogErrorDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllLogErrorsQuery();
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Retrieves a log error entry by its unique identifier.
        /// </summary>
        /// <remarks>Requires authentication. Returns status code 200 if the log error is found, 401 if
        /// the user is unauthorized, 403 if access is forbidden, or 404 if the log error entry does not
        /// exist.</remarks>
        /// <param name="id">The unique identifier of the log error entry to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the <see
        /// cref="LogErrorDto"/> if found; otherwise, an <see cref="ErrorResponseDto"/> indicating the error.</returns>
        [HttpGet("v1/log-error/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<LogErrorDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetLogErrorByIdQuery { Id = id };
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Retrieves a paginated list of log errors with optional sorting.
        /// </summary>
        /// <remarks>This endpoint requires authentication. Use this method to browse log errors with
        /// support for pagination and customizable sorting.</remarks>
        /// <param name="pageIndex">The one-based index of the page to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items to include in each page. Must be greater than 0.</param>
        /// <param name="orderByProperty">The name of the property to sort the results by. If <see langword="null"/>, the default sort order is
        /// applied.</param>
        /// <param name="orderByDescending"><see langword="true"/> to sort the results in descending order; otherwise, <see langword="false"/> for
        /// ascending order.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with a <see
        /// cref="PaginatedResponseDto{T}"/> of <see cref="LogErrorDto"/> items if successful. Returns an <see
        /// cref="ErrorResponseDto"/> with the appropriate status code if the request is unauthorized, forbidden, or not
        /// found.</returns>
        [HttpGet("v1/log-error/search")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<PaginatedResponseDto<LogErrorDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> SearchPaginated(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string orderByProperty = null,
            [FromQuery] bool orderByDescending = false)
        {
            var query = new SearchLogErrorsPaginatedQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                OrderByProperty = orderByProperty,
                OrderByDescending = orderByDescending
            };
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Saves a new error log entry based on the provided error details.
        /// </summary>
        /// <remarks>Requires authentication. Returns HTTP 200 with the saved error log on success, 401 if
        /// the user is unauthorized, or 403 if access is forbidden.</remarks>
        /// <param name="command">The command containing the error information to be logged. Must not be <c>null</c>.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the saved <see
        /// cref="LogErrorDto"/> if the operation succeeds; otherwise, an <see cref="ErrorResponseDto"/> describing the
        /// error.</returns>
        [HttpPost("v1/log-error/save-error")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<LogErrorDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        public async Task<IActionResult> SaveError([FromBody] SaveErrorCommand command)
        {
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Creates a new log error entry using the specified command.
        /// </summary>
        /// <remarks>Requires authentication. Returns HTTP 200 with the created log error on success, 401
        /// if the user is unauthorized, or 403 if access is forbidden.</remarks>
        /// <param name="command">The command containing the details of the log error to create. Cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the created <see
        /// cref="LogErrorDto"/> if successful; otherwise, an <see cref="ErrorResponseDto"/> indicating the error.</returns>
        [HttpPost("v1/log-error")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<LogErrorDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        public async Task<IActionResult> Create([FromBody] CreateLogErrorCommand command)
        {
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Updates an existing log error entry with the specified values.
        /// </summary>
        /// <remarks>Requires authentication. The caller must have appropriate permissions to update the
        /// specified log error entry.</remarks>
        /// <param name="id">The unique identifier of the log error to update. Must correspond to an existing log error entry.</param>
        /// <param name="command">An object containing the updated values for the log error. The request body must include all required fields
        /// for the update operation.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the updated <see
        /// cref="LogErrorDto"/> if the update is successful; otherwise, an <see cref="ErrorResponseDto"/> describing
        /// the error.</returns>
        [HttpPut("v1/log-error/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<LogErrorDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLogErrorCommand command)
        {
            command.Id = id;
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Deletes the log error entry with the specified identifier.
        /// </summary>
        /// <remarks>Requires authentication. Only authorized users can delete log error
        /// entries.</remarks>
        /// <param name="id">The unique identifier of the log error entry to delete.</param>
        /// <returns>— An <see cref="IActionResult"/> indicating the result of the delete operation. Returns status code 200 if
        /// the deletion is successful; 401 if the user is unauthorized; 403 if the user does not have permission; or
        /// 404 if the entry is not found.</returns>
        [HttpDelete("v1/log-error/{id}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteLogErrorCommand { Id = id };
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }
    }
}
