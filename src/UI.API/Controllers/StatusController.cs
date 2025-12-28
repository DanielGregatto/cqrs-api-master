using Domain.DTO.Infrastructure.API;
using Domain.DTO.Responses;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Features.Status.Commands.CreateStatus;
using Services.Features.Status.Commands.DeleteStatus;
using Services.Features.Status.Commands.UpdateStatus;
using Services.Features.Status.Queries.GetAllStatus;
using Services.Features.Status.Queries.GetIdStatusAtivo;
using Services.Features.Status.Queries.GetIdStatusInativo;
using Services.Features.Status.Queries.GetStatusById;
using Services.Features.Status.Queries.SearchStatusPaginated;
using UI.API.Controllers.Base;

namespace UI.API.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : CoreController
    {
        private readonly IMediatorHandler _mediator;
        private readonly IUser _user;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IMediatorHandler mediator, IUser user, ILogger<StatusController> logger)
        {
            _mediator = mediator;
            _user = user;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all available status records.
        /// </summary>
        /// <remarks>This endpoint returns a collection of status records. The response includes a list of
        /// status data transfer objects if the request is successful. Authorization is required to access this
        /// endpoint.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with an <see
        /// cref="IEnumerable{T}"/> of <see cref="StatusDto"/> objects if successful; otherwise, an <see
        /// cref="ErrorResponseDto"/> describing the error.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<StatusDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllStatusQuery();
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Retrieves the status with the specified unique identifier.
        /// </summary>
        /// <remarks>Returns a 200 response with the status data if the status exists.  Returns a 404
        /// response if no status is found for the specified identifier.  Returns a 401 or 403 response if the caller is
        /// not authorized to access the resource.</remarks>
        /// <param name="id">The unique identifier of the status to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the status data if found; 
        /// otherwise, an <see cref="ErrorResponseDto"/> indicating the reason for failure.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<StatusDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Getting status by ID: {StatusId}", id);

            var query = new GetStatusByIdQuery { Id = id };
            var result = await _mediator.SendCommand(query);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully retrieved status {StatusId}", id);
            else
                _logger.LogWarning("Failed to retrieve status {StatusId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Retrieves the unique identifier for the active status.
        /// </summary>
        /// <remarks>This endpoint returns the GUID representing the "active" status in the system.  The
        /// caller must have appropriate authorization to access this resource.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{Guid}"/> with the active status
        /// identifier if successful;  otherwise, an <see cref="ErrorResponseDto"/> describing the error.</returns>
        [HttpGet("ativo")]
        [ProducesResponseType(typeof(SuccessResponse<Guid>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> GetIdStatusAtivo()
        {
            var query = new GetIdStatusAtivoQuery();
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }


        /// <summary>
        /// Retrieves the unique identifier for the "Inativo" status.
        /// </summary>
        /// <remarks>This endpoint returns the GUID associated with the "Inativo" status. The caller must
        /// have appropriate authorization to access this resource.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{Guid}"/> with the identifier of the
        /// "Inativo" status if successful; otherwise, an <see cref="ErrorResponseDto"/> describing the error.</returns>
        [HttpGet("inativo")]
        [ProducesResponseType(typeof(SuccessResponse<Guid>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> GetIdStatusInativo()
        {
            var query = new GetIdStatusInativoQuery();
            var result = await _mediator.SendCommand(query);
            return Response(result);
        }

        /// <summary>
        /// Retrieves a paginated list of status records, optionally sorted by a specified property and order.
        /// </summary>
        /// <remarks>This endpoint supports pagination and sorting for status records. Only authorized
        /// users can access this resource.</remarks>
        /// <param name="pageIndex">The one-based index of the page to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of items to include in each page. Must be greater than 0.</param>
        /// <param name="orderByProperty">The name of the property to sort the results by. If <see langword="null"/>, the default sort order is
        /// applied.</param>
        /// <param name="orderByDescending"><see langword="true"/> to sort the results in descending order; otherwise, <see langword="false"/> for
        /// ascending order.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with a paginated list of <see
        /// cref="StatusDto"/> objects if successful; otherwise, an <see cref="ErrorResponseDto"/> describing the error.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(SuccessResponse<PaginatedResponseDto<StatusDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> SearchPaginated(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string orderByProperty = null,
            [FromQuery] bool orderByDescending = false)
        {
            var query = new SearchStatusPaginatedQuery
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
        /// Creates a new status using the specified command.
        /// </summary>
        /// <remarks>This action requires authentication. The caller must have appropriate permissions to
        /// create a new status.</remarks>
        /// <param name="command">The <see cref="CreateStatusCommand"/> containing the details of the status to create. This parameter must
        /// not be <c>null</c>.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="SuccessResponse{T}"/> with the created status data if
        /// successful, or an <see cref="ErrorResponseDto"/> with error details if the operation fails.</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<StatusDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]        
        public async Task<IActionResult> Create([FromBody] CreateStatusCommand command)
        {
            _logger.LogInformation("Creating new status with name: {StatusName}", command.Nome);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully created status with name: {StatusName}", command.Nome);
            else
                _logger.LogWarning("Failed to create status. Errors: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }

        /// <summary>
        /// Updates the status entity identified by the specified ID with the provided data.
        /// </summary>
        /// <remarks>Requires authentication. Returns a 200 response with the updated status on success,
        /// 401 if the user is not authenticated, 403 if the user is not authorized, or 404 if the status with the
        /// specified ID does not exist.</remarks>
        /// <param name="id">The unique identifier of the status to update. Must correspond to an existing status entity.</param>
        /// <param name="command">An <see cref="UpdateStatusCommand"/> object containing the updated status information. The request body must
        /// not be null.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains an <see
        /// cref="IActionResult"/> that returns a <see cref="SuccessResponse{StatusDto}"/> with the updated status if
        /// successful; otherwise, an <see cref="ErrorResponseDto"/> describing the error.</returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<StatusDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStatusCommand command)
        {
            command.Id = id;
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Deletes the status with the specified identifier.
        /// </summary>
        /// <remarks>This action requires authentication. Only authorized users can delete
        /// statuses.</remarks>
        /// <param name="id">The unique identifier of the status to delete.</param>
        /// <returns>An <see cref="IActionResult"/> that indicates the result of the delete operation. Returns status code 200 if
        /// the deletion is successful; otherwise, returns an error response with the appropriate status code.</returns>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteStatusCommand { Id = id };
            var result = await _mediator.SendCommand(command);
            return Response(result);
        }
    }
}
