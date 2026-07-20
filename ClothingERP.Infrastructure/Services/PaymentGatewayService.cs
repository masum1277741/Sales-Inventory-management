using ClothingERP.Application.Common;
using ClothingERP.Application.DTOs;
using ClothingERP.Application.Interfaces;
using ClothingERP.Application.Interfaces.Services;
using ClothingERP.Domain.Entities;
using ClothingERP.Infrastructure.PaymentGateways;
using Microsoft.EntityFrameworkCore;

namespace ClothingERP.Infrastructure.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly IUnitOfWork _uow;
    private readonly BkashApiClient _bkash;
    private readonly NagadApiClient _nagad;
    private readonly IExchangeRateService _rateSvc;   // Feature #14 — USD↔BDT conversion এর জন্য

    public PaymentGatewayService(IUnitOfWork uow, BkashApiClient bkash, NagadApiClient nagad, IExchangeRateService rateSvc)
        => (_uow, _bkash, _nagad, _rateSvc) = (uow, bkash, nagad, rateSvc);

    // ── Initiate ──────────────────────────────────────────────────────────
    public async Task<InitiatePaymentResultDto> InitiatePaymentAsync(InitiatePaymentDto dto, int userId)
    {
        var rates = await _rateSvc.GetCurrentRatesAsync();
        var amountBDT = Math.Round(dto.AmountUSD * rates.UsdToBdt, 2);
        var orderId = $"CLZ-{DateTime.UtcNow:yyyyMMddHHmmss}-{userId}";

        if (dto.Provider.Equals("bKash", StringComparison.OrdinalIgnoreCase))
        {
            var (success, paymentId, bkashUrl, rawJson, error) = await _bkash.CreatePaymentAsync(amountBDT, orderId);

            await SaveTransactionAsync(dto.Provider, paymentId, dto.AmountUSD, amountBDT,
                success ? "Pending" : "Failed", dto.CustomerMsisdn, error, rawJson, userId);

            return new InitiatePaymentResultDto
            {
                Success = success,
                Message = success ? "bKash checkout তৈরি হয়েছে — customer কে redirect করুন।" : error,
                GatewayPaymentId = paymentId,
                RedirectUrl = bkashUrl,
                AmountBDT = amountBDT
            };
        }
        else if (dto.Provider.Equals("Nagad", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(dto.CustomerMsisdn))
                return new InitiatePaymentResultDto { Success = false, Message = "Nagad এর জন্য customer এর মোবাইল নম্বর প্রয়োজন।" };

            var (success, refId, callbackUrl, rawJson) = await _nagad.InitializePaymentAsync(amountBDT, orderId, dto.CustomerMsisdn);

            await SaveTransactionAsync(dto.Provider, refId ?? orderId, dto.AmountUSD, amountBDT,
                success ? "Pending" : "Failed", dto.CustomerMsisdn, success ? null : "Initialize failed", rawJson, userId);

            return new InitiatePaymentResultDto
            {
                Success = success,
                Message = success ? "Nagad payment শুরু হয়েছে — customer কে confirm করতে বলুন।" : "Nagad payment শুরু করা যায়নি।",
                GatewayPaymentId = refId ?? "",
                RedirectUrl = callbackUrl,
                AmountBDT = amountBDT
            };
        }

        return new InitiatePaymentResultDto { Success = false, Message = "অজানা payment provider।" };
    }

    private async Task SaveTransactionAsync(string provider, string gatewayPaymentId, decimal amountUSD,
        decimal amountBDT, string status, string? msisdn, string? error, string rawJson, int userId)
    {
        await _uow.DigitalPaymentTransactions.AddAsync(new DigitalPaymentTransaction
        {
            Provider = provider,
            GatewayPaymentId = gatewayPaymentId,
            Amount = amountUSD,
            AmountBDT = amountBDT,
            Status = status,
            CustomerMsisdn = msisdn,
            FailureReason = error,
            RawResponseJson = rawJson,
            CreatedBy = userId
        });
        await _uow.SaveChangesAsync();
    }

    // ── Execute (bKash এর জন্য, customer approve করার পরে confirm) ──────────
    public async Task<ServiceResult<PaymentStatusDto>> ExecutePaymentAsync(ExecutePaymentDto dto)
    {
        var txn = await _uow.DigitalPaymentTransactions.GetQueryable()
            .FirstOrDefaultAsync(t => t.GatewayPaymentId == dto.GatewayPaymentId);
        if (txn == null) return ServiceResult<PaymentStatusDto>.Fail("Transaction পাওয়া যায়নি।");

        if (dto.Provider.Equals("bKash", StringComparison.OrdinalIgnoreCase))
        {
            var (success, status, trxId, rawJson, error) = await _bkash.ExecutePaymentAsync(dto.GatewayPaymentId);

            txn.Status = success ? "Completed" : "Failed";
            txn.GatewayTrxId = trxId;
            txn.FailureReason = error;
            txn.RawResponseJson = rawJson;
            txn.CompletedAt = success ? DateTime.UtcNow : null;
            _uow.DigitalPaymentTransactions.Update(txn);
            await _uow.SaveChangesAsync();

            return ServiceResult<PaymentStatusDto>.Ok(new PaymentStatusDto
            {
                Status = txn.Status,
                GatewayTrxId = trxId,
                AmountUSD = txn.Amount,
                FailureReason = error,
                IsFinal = true
            }, success ? "Payment সফল হয়েছে!" : "Payment ব্যর্থ হয়েছে।");
        }

        return ServiceResult<PaymentStatusDto>.Fail("এই provider এর জন্য execute step প্রয়োজন নেই।");
    }

    // ── Check Status (POS polling) ────────────────────────────────────────
    public async Task<PaymentStatusDto> CheckStatusAsync(string gatewayPaymentId)
    {
        var txn = await _uow.DigitalPaymentTransactions.GetQueryable()
            .FirstOrDefaultAsync(t => t.GatewayPaymentId == gatewayPaymentId);

        if (txn == null) return new PaymentStatusDto { Status = "NotFound", IsFinal = true };

        // ইতিমধ্যে final status হয়ে থাকলে আবার gateway call করার দরকার নেই
        if (txn.Status is "Completed" or "Failed" or "Cancelled")
        {
            return new PaymentStatusDto
            {
                Status = txn.Status,
                GatewayTrxId = txn.GatewayTrxId,
                AmountUSD = txn.Amount,
                FailureReason = txn.FailureReason,
                IsFinal = true
            };
        }

        // ── Pending হলে gateway থেকে fresh status query করো ──────────────────
        if (txn.Provider == "bKash")
        {
            var (status, trxId, rawJson) = await _bkash.QueryPaymentAsync(gatewayPaymentId);
            txn.Status = status == "Completed" ? "Completed" : status == "Initiated" ? "Pending" : "Failed";
            txn.GatewayTrxId = trxId;
            txn.RawResponseJson = rawJson;
            if (txn.Status != "Pending") txn.CompletedAt = DateTime.UtcNow;
        }
        else if (txn.Provider == "Nagad")
        {
            var (status, trxId, rawJson) = await _nagad.VerifyPaymentAsync(gatewayPaymentId);
            txn.Status = status.Equals("Success", StringComparison.OrdinalIgnoreCase) ? "Completed" : "Pending";
            txn.GatewayTrxId = trxId;
            txn.RawResponseJson = rawJson;
        }

        _uow.DigitalPaymentTransactions.Update(txn);
        await _uow.SaveChangesAsync();

        return new PaymentStatusDto
        {
            Status = txn.Status,
            GatewayTrxId = txn.GatewayTrxId,
            AmountUSD = txn.Amount,
            IsFinal = txn.Status is "Completed" or "Failed" or "Cancelled"
        };
    }

    // ── Callback Handler (gateway থেকে browser redirect এ আসবে) ─────────────
    public async Task<ServiceResult<PaymentStatusDto>> HandleCallbackAsync(string provider, string gatewayPaymentId, string? status)
    {
        if (status?.Equals("cancel", StringComparison.OrdinalIgnoreCase) == true ||
            status?.Equals("failure", StringComparison.OrdinalIgnoreCase) == true)
        {
            var txn = await _uow.DigitalPaymentTransactions.GetQueryable()
                .FirstOrDefaultAsync(t => t.GatewayPaymentId == gatewayPaymentId);
            if (txn != null)
            {
                txn.Status = status.Equals("cancel", StringComparison.OrdinalIgnoreCase) ? "Cancelled" : "Failed";
                _uow.DigitalPaymentTransactions.Update(txn);
                await _uow.SaveChangesAsync();
            }
            return ServiceResult<PaymentStatusDto>.Fail("Customer payment বাতিল/ব্যর্থ করেছে।");
        }

        // bKash এর জন্য success callback এলে execute করতে হয়
        if (provider.Equals("bKash", StringComparison.OrdinalIgnoreCase))
            return await ExecutePaymentAsync(new ExecutePaymentDto { GatewayPaymentId = gatewayPaymentId, Provider = provider });

        // Nagad নিজেই callback এ confirm করে দেয় — শুধু status query করো
        var statusResult = await CheckStatusAsync(gatewayPaymentId);
        return ServiceResult<PaymentStatusDto>.Ok(statusResult, "Status updated.");
    }

    public async Task<IEnumerable<DigitalPaymentTransaction>> GetRecentTransactionsAsync(int take = 50)
    {
        return await _uow.DigitalPaymentTransactions.GetQueryable()
            .OrderByDescending(t => t.InitiatedAt)
            .Take(take)
            .ToListAsync();
    }
}