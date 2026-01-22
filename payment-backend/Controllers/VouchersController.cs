using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentService2.Models;
using PaymentService2.Services;

namespace PaymentService2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VouchersController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Voucher>>> GetVouchers()
    {
        var vouchers = await _voucherService.GetVouchersAsync();
        return Ok(vouchers);
    }

    [HttpPost("apply")]
    public async Task<ActionResult<VoucherApplyResult>> ApplyVoucher([FromBody] ApplyVoucherRequest request)
    {
        Console.WriteLine($"[VoucherDebug] Received Apply Request. Code: '{request.Code}', OrderTotal: {request.OrderTotal}");
        var result = await _voucherService.ApplyVoucherAsync(request.Code, request.OrderTotal);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}


