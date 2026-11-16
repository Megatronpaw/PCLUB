using System;
using System.Collections.Generic;
using System.Text;

namespace PClub.Application.Abstractions
{
    /// <summary>
    /// Хэширование и проверка паролей.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Считает хеш пароля вместе с солью и параметрами
        /// </summary>
        /// <param name="password">Пароль как есть</param>
        /// <returns>Строка, пригодная для хранения в базе.</returns>
        string Hash(string password);

        /// <summary>
        /// Проверяет пароль против протоколов сохраненого хеша.
        /// </summary>
        /// <param name="password">Пароль как есть</param>
        /// <param name="storedHash">Что лежит в базе</param>
        /// <returns>Совпал ли пароль</returns>
        bool Verify(string password, string storedHash);
    }
}
