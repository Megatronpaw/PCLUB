namespace PClub.Domain.Enums;

/// <summary>Вид операции по кошельку.</summary>
public enum WalletTransactionType
{
    /// <summary>Пополнение пользователем.</summary>
    TopUp = 1,

    /// <summary>Списание за бронь.</summary>
    BookingCharge = 2,

    /// <summary>Возврат за отменённую бронь.</summary>
    BookingRefund = 3,

    /// <summary>Ручная корректировка администратором.</summary>
    Adjustment = 4
}
