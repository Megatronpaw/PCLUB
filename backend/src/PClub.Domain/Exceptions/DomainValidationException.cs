using System;
using System.Collections.Generic;
using System.Text;

namespace PClub.Domain.Exceptions
{
    ///<summary>Данные не проходят преданные области. Станет HTTP 400.</summary>
    public sealed class DomainValidationException : DomainException
    {
        ///<summary>Создаёт исключение нарушения правила.</summary>
        public DomainValidationException(string message) : base(message)
        {
        }
    }
}
