using System.Security.Claims;
using ERP.Application.DTOs;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Transactions.View)]
    public IActionResult GetAll()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        int? customerId = null;

        if (role == "Customer")
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int cId))
            {
                return Forbid();
            }
            customerId = cId;
        }

        var list = _transactionService.GetAll(customerId);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Transactions.View)]
    public IActionResult GetById(int id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var txn = _transactionService.GetById(id);
        if (txn == null) return NotFound(new { message = $"Transaction with ID {id} not found." });

        if (role == "Customer")
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (int.TryParse(custIdStr, out int cId))
            {
                var allowed = _transactionService.GetAll(cId).Any(t => t.TransactionId == id);
                if (!allowed) return Forbid();
            }
            else
            {
                return Forbid();
            }
        }

        return Ok(txn);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.Transactions.Add)]
    public IActionResult Create([FromBody] CreateTransactionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var created = _transactionService.Create(dto, user);
            return CreatedAtAction(nameof(GetById), new { id = created.TransactionId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
