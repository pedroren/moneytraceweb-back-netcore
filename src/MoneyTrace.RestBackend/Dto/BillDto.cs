using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Bills;

namespace MoneyTrace.RestBackend.Dto;

public record BillDto(
    int Id,
    string Name,
    int TemplateId,
    Frequency PaymentFrequency,
    DateTime NextDueDate,
    decimal NextDueAmount,
    int PaymentDay,
    int? PaymentMonth);
public static class BillDtoExtensions
{
    public static UpdateBillCommand ToUpdateCommand(this BillDto dto, int id, int userId)
    {
        return new UpdateBillCommand(
            userId,
            id,
            dto.Name.Trim(),
            dto.TemplateId,
            dto.PaymentFrequency,
            dto.NextDueDate,
            dto.NextDueAmount,
            dto.PaymentDay,
            dto.PaymentMonth);
    }

    public static CreateBillCommand ToCreateCommand(this BillDto dto, int userId)
    {
        return new CreateBillCommand(
            userId,
            dto.Name.Trim(),
            dto.TemplateId,
            dto.PaymentFrequency,
            dto.NextDueDate,
            dto.NextDueAmount,
            dto.PaymentDay,
            dto.PaymentMonth);
    }

    public static BillDto ToDto(this BillEntity bill)
    {
        return new BillDto(
            bill.Id,
            bill.Name,
            bill.TemplateId,
            bill.PaymentFrequency,
            bill.NextDueDate,
            bill.NextDueAmount,
            bill.PaymentDay,
            bill.PaymentMonth);
    }
    public static BillEntity ToEntity(this BillDto billDto)
    {
        return new BillEntity
        {
            Id = billDto.Id,
            Name = billDto.Name,
            TemplateId = billDto.TemplateId,
            PaymentFrequency = billDto.PaymentFrequency,
            NextDueDate = billDto.NextDueDate,
            NextDueAmount = billDto.NextDueAmount,
            PaymentDay = billDto.PaymentDay,
            PaymentMonth = billDto.PaymentMonth
        };
    }
}