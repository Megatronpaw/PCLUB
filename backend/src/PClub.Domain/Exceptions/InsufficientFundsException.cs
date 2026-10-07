using System;
using System.Collections.Generic;
using System.Text;

namespace PClub.Domain.Exceptions
{
    /// <summary>
    /// Не хватило денег. Станет HTTP 402.
    /// </summary>
    public sealed class InsufficientFundsException : DomainException
    {
        ///<summary>Создаёт исключение нехватки средств.</summary>
        ///<param name="requiredCents">Сколько нужно, в копейках.</param>
        ///<param name="availableCents">Сколько есть, в копейках.</param>
        public InsufficientFundsException(long requiredCents, long availableCents) 
            : base($"Требуется {requiredCents} коп., доступно {availableCents} коп.")
        {
            RequiredCents = requiredCents;
            AvailableCents = availableCents;
            ShortfallCents = requiredCents - availableCents;
        }
        /// <summary>
        /// Сколько нужно, в копейках.
        /// </summary>
        public long RequiredCents { get; }
        /// <summary>
        /// Сколько есть, в копейках.
        /// </summary>
        public long AvailableCents { get; }
        /// <summary>
        /// Сколько не хватает, в копейках.
        /// </summary>
        public long ShortfallCents { get; }
    }
}
