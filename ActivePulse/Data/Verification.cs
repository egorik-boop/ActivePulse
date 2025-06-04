using ActivePulse.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ActivePulse.Data
{
    internal class Verification
    {
        AppDbContext db = new AppDbContext();
        public static string GetSHA512Hash(string input)
        {
            // Создаем новый экземпляр объекта MD5CryptoServiceProvider.
            SHA512CryptoServiceProvider SHA512Hasher = new SHA512CryptoServiceProvider();

            // Преобразуем входную строку в байтовый массив и вычисляем хеш.
            byte[] data = SHA512Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Создаем новый Stringbuilder для сбора байтов и создания строки.
            StringBuilder sBuilder = new StringBuilder();

            // Перебираем каждый байт хэшированных данных 
            // и форматируем каждый как шестнадцатеричную строку.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Возвращаем шестнадцатеричную строку.
            return sBuilder.ToString();
        }

        // Проверяет хэш (из базы данных) на соответствие строке (из поля ввода)
        public static bool VerifySHA512Hash(string input, string hash)
        {
            // Хэшируем ввод.
            string hashOfInput = GetSHA512Hash(input);

            // Создаем StringComparer для сравнения хэшей.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            // Сравниваем два хэша.
            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true; // Хэши равны.
            }
            else
            {
                return false; // Не равны.
            }
        }

        public static bool CheckPassword(string password, string passRepeat)
        {
            if (password.Length < 6)
            {
                CustomMessageBox.Show("Ошибка регистрации", "Длина пароля не может быть меньше 6 символов");
                return false;
            }
            else
            {
                bool f, f1, f2;
                f = f1 = f2 = false;
                for (int i = 0; i < password.Length; i++)
                {
                    if (Char.IsDigit(password[i])) f1 = true;
                    if (Char.IsUpper(password[i])) f2 = true;
                    if (f1 && f2) break;
                }
                if (!(f1 && f2))
                {
                    CustomMessageBox.Show("Ошибка регистрации", "Пароль должен содержать хотя бы одну цифру и заглавную букву!");
                    return f1 && f2;
                }
                else
                {
                    string simbol = "!@#$%^";
                    for (int i = 0; i < password.Length; i++)
                    {
                        for (int j = 0; j < simbol.Length; j++)
                        {
                            if (password[i] == simbol[j])
                            {
                                f = true;
                                break;
                            }
                        }
                    }
                    if (!f) CustomMessageBox.Show("Ошибка регистрации", "Пароль должен содержать 1 из следующих символов: !@#$%^");
                    if ((password == passRepeat) && f) return true; else { CustomMessageBox.Show("Ошибка ввода пароля", "Неверно подтвержден пароль"); return false; }
                }
            }
        }

        public static bool CheckUser(string login)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    bool exists = db.Users.Any(u => u.Login == login);
                    return exists;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка", ex.Message);
                return false;
            }
        }

    }
}
