using Microsoft.AspNetCore.Mvc;
using Lucrarea1PSSC.clase.Workflow;
using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.api.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace Lucrarea1PSSC.api.Controllers
{
    /// <summary>
    /// DTOs for Order Pickup operations
    /// </summary>
    public record PreluareComandaRequest
    {
        /// <summary>
        /// The ID of the order to pick up
        /// </summary>
        public Guid ComandaId { get; init; }

        /// <summary>
        /// Name of the operator picking up the order
        /// </summary>
        public string OperatorName { get; init; } = string.Empty;
    }

    public record FinalizeazaPregatireRequest
    {
        /// <summary>
        /// The ID of the order to finalize
        /// </summary>
        public Guid ComandaId { get; init; }

        /// <summary>
        /// Name of the operator finalizing the order
        /// </summary>
        public string OperatorName { get; init; } = string.Empty;

        /// <summary>
        /// Whether the order is ready for shipment
        /// </summary>
        public bool IsReadyForShipment { get; init; } = true;
    }

    public record PreluareComandaResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public Guid ComandaId { get; init; }
        public string? NumeClient { get; init; }
        public string? OperatorName { get; init; }
        public double Total { get; init; }
        public int NumarProduse { get; init; }
        public DateTime PickupTime { get; init; }
        public string? StareCurenta { get; init; }
        public double? EstimatedPreparationMinutes { get; init; }
    }

    public record OrderStatusResponse
    {
        public Guid ComandaId { get; init; }
        public string NumeClient { get; init; } = string.Empty;
        public string Stare { get; init; } = string.Empty;
        public double Total { get; init; }
        public int NumarProduse { get; init; }
        public DateTime DataPlasare { get; init; }
        public string AdresaLivrare { get; init; } = string.Empty;
    }

    /// <summary>
    /// Order Pickup (Preluare Comanda) API Controller
    /// Handles the workflow for picking up orders for preparation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Order pickup workflow - manage order preparation and fulfillment")]
    public class PreluareComandaController : ControllerBase
    {
        private readonly PreluareComandaWorkflow _workflow;

        public PreluareComandaController(PreluareComandaWorkflow workflow)
        {
            _workflow = workflow;
        }

        /// <summary>
        /// Pick up an order for preparation
        /// </summary>
        /// <param name="request">Order pickup request</param>
        /// <returns>Pickup confirmation with order details</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/preluarecomanda/pickup
        ///     {
        ///        "comandaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///        "operatorName": "Ion Operator"
        ///     }
        ///
        /// This endpoint will:
        /// - Validate the order exists and is in 'Plasata' state
        /// - Verify all order details are valid
        /// - Transition order to 'InPregatire' state
        /// - Calculate estimated preparation time
        /// - Publish order pickup events
        /// - Return pickup receipt
        /// 
        /// **Business Rules:**
        /// - Only orders in 'Plasata' state can be picked up
        /// - Operator name must be provided
        /// - Order must have valid delivery address
        /// - Order must contain at least one product
        /// </remarks>
        /// <response code="200">Order picked up successfully</response>
        /// <response code="400">Invalid request or order state</response>
        /// <response code="404">Order not found</response>
        [HttpPost("pickup")]
        [SwaggerOperation(
            Summary = "?? Pick Up Order for Preparation",
            Description = "Starts the order preparation process by picking up a placed order",
            OperationId = "PickupOrder",
            Tags = new[] { "Order Pickup", "Workflow" }
        )]
        [SwaggerResponse(200, "Order picked up successfully", typeof(PreluareComandaResponse))]
        [SwaggerResponse(400, "Invalid request or order state", typeof(ErrorResponse))]
        [SwaggerResponse(404, "Order not found", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(PreluareComandaResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> PickupOrder([FromBody] PreluareComandaRequest request)
        {
            if (request.ComandaId == Guid.Empty)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "ID-ul comenzii nu poate fi gol",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(request.OperatorName))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Numele operatorului nu poate fi gol",
                    Timestamp = DateTime.UtcNow
                });
            }

            var result = await _workflow.PreluareComandaAsync(request.ComandaId, request.OperatorName);

            return result switch
            {
                ComandaEvent.ComandaPreluataSuccessEvent success => Ok(new PreluareComandaResponse
                {
                    Success = true,
                    Message = success.Message,
                    ComandaId = success.ComandaId,
                    NumeClient = success.NumeClient,
                    OperatorName = success.OperatorName,
                    Total = success.Total,
                    NumarProduse = success.NumarProduse,
                    PickupTime = success.PickupTime,
                    StareCurenta = "InPregatire"
                }),

                ComandaEvent.ComandaPreluataFailedEvent failed when failed.Reason.Contains("gasita") =>
                    NotFound(new ErrorResponse
                    {
                        Error = failed.Message,
                        Timestamp = failed.Timestamp
                    }),

                ComandaEvent.ComandaPreluataFailedEvent failed => BadRequest(new ErrorResponse
                {
                    Error = failed.Message,
                    Timestamp = failed.Timestamp
                }),

                _ => BadRequest(new ErrorResponse
                {
                    Error = "Eroare necunoscuta la preluarea comenzii",
                    Timestamp = DateTime.UtcNow
                })
            };
        }

        /// <summary>
        /// Finalize order preparation
        /// </summary>
        /// <param name="request">Finalization request</param>
        /// <returns>Finalization confirmation</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/preluarecomanda/finalize
        ///     {
        ///        "comandaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///        "operatorName": "Ion Operator",
        ///        "isReadyForShipment": true
        ///     }
        ///
        /// This endpoint will:
        /// - Verify order is in 'InPregatire' state
        /// - Mark order as ready (or not ready) for shipment
        /// - Publish preparation completed event
        /// </remarks>
        /// <response code="200">Order preparation finalized successfully</response>
        /// <response code="400">Invalid request or order state</response>
        /// <response code="404">Order not found</response>
        [HttpPost("finalize")]
        [SwaggerOperation(
            Summary = "? Finalize Order Preparation",
            Description = "Marks the order preparation as complete and ready for shipment",
            OperationId = "FinalizePreparation",
            Tags = new[] { "Order Pickup", "Workflow" }
        )]
        [SwaggerResponse(200, "Preparation finalized successfully", typeof(PreluareComandaResponse))]
        [SwaggerResponse(400, "Invalid request or order state", typeof(ErrorResponse))]
        [SwaggerResponse(404, "Order not found", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(PreluareComandaResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public IActionResult FinalizePreparation([FromBody] FinalizeazaPregatireRequest request)
        {
            if (request.ComandaId == Guid.Empty)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "ID-ul comenzii nu poate fi gol",
                    Timestamp = DateTime.UtcNow
                });
            }

            var result = _workflow.FinalizeazaPregatire(
                request.ComandaId,
                request.OperatorName,
                request.IsReadyForShipment);

            return result switch
            {
                ComandaEvent.ComandaPreluataSuccessEvent success => Ok(new PreluareComandaResponse
                {
                    Success = true,
                    Message = $"Pregatirea comenzii a fost finalizata. {(request.IsReadyForShipment ? "Comanda este gata pentru expediere." : "Comanda necesita verificari suplimentare.")}",
                    ComandaId = success.ComandaId,
                    NumeClient = success.NumeClient,
                    OperatorName = success.OperatorName,
                    Total = success.Total,
                    NumarProduse = success.NumarProduse,
                    PickupTime = success.PickupTime
                }),

                ComandaEvent.ComandaPreluataFailedEvent failed when failed.Reason.Contains("gasita") =>
                    NotFound(new ErrorResponse
                    {
                        Error = failed.Message,
                        Timestamp = failed.Timestamp
                    }),

                ComandaEvent.ComandaPreluataFailedEvent failed => BadRequest(new ErrorResponse
                {
                    Error = failed.Message,
                    Timestamp = failed.Timestamp
                }),

                _ => BadRequest(new ErrorResponse
                {
                    Error = "Eroare necunoscuta",
                    Timestamp = DateTime.UtcNow
                })
            };
        }

        /// <summary>
        /// Get order status by ID
        /// </summary>
        /// <param name="comandaId">The order ID</param>
        /// <returns>Order status and details</returns>
        [HttpGet("status/{comandaId:guid}")]
        [SwaggerOperation(
            Summary = "?? Get Order Status",
            Description = "Retrieves the current status and details of an order",
            OperationId = "GetOrderStatus",
            Tags = new[] { "Order Pickup", "Query" }
        )]
        [SwaggerResponse(200, "Order status retrieved", typeof(OrderStatusResponse))]
        [SwaggerResponse(404, "Order not found", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(OrderStatusResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public IActionResult GetOrderStatus([FromRoute] Guid comandaId)
        {
            var order = _workflow.GetOrder(comandaId);

            if (order == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = $"Comanda {comandaId} nu a fost gasita",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new OrderStatusResponse
            {
                ComandaId = order.ComandaId,
                NumeClient = order.NumeClient,
                Stare = order.Stare.ToString(),
                Total = order.Total.ToDouble(),
                NumarProduse = order.Produse.Count,
                DataPlasare = order.DataPlasare,
                AdresaLivrare = order.AdresaLivrare.adress
            });
        }

        /// <summary>
        /// Get all orders in a specific state
        /// </summary>
        /// <param name="state">The order state to filter by</param>
        /// <returns>List of orders in the specified state</returns>
        [HttpGet("by-state/{state}")]
        [SwaggerOperation(
            Summary = "?? Get Orders by State",
            Description = "Retrieves all orders in a specific state (Plasata, InPregatire, Expediata, Livrata, Anulata)",
            OperationId = "GetOrdersByState",
            Tags = new[] { "Order Pickup", "Query" }
        )]
        [SwaggerResponse(200, "Orders retrieved", typeof(OrderStatusResponse[]))]
        [SwaggerResponse(400, "Invalid state", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(OrderStatusResponse[]), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public IActionResult GetOrdersByState([FromRoute] string state)
        {
            if (!Enum.TryParse<StaraComanda>(state, true, out var orderState))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Stare invalida: {state}. Stari valide: Plasata, InPregatire, Expediata, Livrata, Anulata",
                    Timestamp = DateTime.UtcNow
                });
            }

            var orders = _workflow.GetOrdersByState(orderState);
            var response = new List<OrderStatusResponse>();

            foreach (var order in orders)
            {
                response.Add(new OrderStatusResponse
                {
                    ComandaId = order.ComandaId,
                    NumeClient = order.NumeClient,
                    Stare = order.Stare.ToString(),
                    Total = order.Total.ToDouble(),
                    NumarProduse = order.Produse.Count,
                    DataPlasare = order.DataPlasare,
                    AdresaLivrare = order.AdresaLivrare.adress
                });
            }

            return Ok(response);
        }
    }
}
