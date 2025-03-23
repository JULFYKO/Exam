using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DbController;
using DbController.Entities;
using System.Text;

namespace Bookstore.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            var optionsBuilder = new DbContextOptionsBuilder<BookstoreContext>();
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BookStore;Integrated Security=True");

            using var context = new BookstoreContext(optionsBuilder.Options);
            context.Database.Migrate();

            if (!Login(context))
            {
                Console.WriteLine("Невірний логін/пароль. Завершення роботи.");
                return;
            }

            ShowBanner();
            bool exit = false;
            while (!exit)
            {
                ShowMenu();
                Console.Write("Введіть номер команди: ");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        AddBook(context);
                        break;
                    case "2":
                        EditBook(context);
                        break;
                    case "3":
                        DeleteBook(context);
                        break;
                    case "4":
                        SearchBooks(context);
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Невірна команда.");
                        break;
                }
            }
            Console.WriteLine("Вихід із програми. До побачення!");
        }

        static bool Login(BookstoreContext context)
        {
            Console.Write("Логін: ");
            string username = Console.ReadLine();
            Console.Write("Пароль: ");
            string password = Console.ReadLine();

            var user = context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            return user != null;
        }

        static void ShowBanner()
        {
            Console.Clear();
            Console.WriteLine(@"  ____  _                   _                          ");
            Console.WriteLine(@" |  _ \| | __ _ _ __   __ _| | ___  _ __   __ _ _ __  ");
            Console.WriteLine(@" | |_) | |/ _` | '_ \ / _` | |/ _ \| '_ \ / _` | '_ \ ");
            Console.WriteLine(@" |  __/| | (_| | | | | (_| | | (_) | | | | (_| | |_) |");
            Console.WriteLine(@" |_|   |_|\__,_|_| |_|\__,_|_|\___/|_| |_|\__,_| .__/ ");
            Console.WriteLine(@"                                             |_|    ");
        }

        static void ShowMenu()
        {
            Console.WriteLine("Меню:");
            Console.WriteLine("1. Додати книгу");
            Console.WriteLine("2. Редагувати книгу");
            Console.WriteLine("3. Видалити книгу");
            Console.WriteLine("4. Пошук книг");
            Console.WriteLine("5. Вихід\n");
        }

        static void AddBook(BookstoreContext context)
        {
            Console.WriteLine("\n--- Додавання книги ---");
            var book = new Book();

            Console.Write("Назва: ");
            book.Title = Console.ReadLine();

            Console.Write("ID автора: ");
            book.AuthorId = ReadInt();

            Console.Write("ID видавництва: ");
            book.PublisherId = ReadInt();

            Console.Write("ID жанру: ");
            book.GenreId = ReadInt();

            Console.Write("Кількість сторінок: ");
            book.Pages = ReadInt();

            Console.Write("Рік видання: ");
            book.YearOfPublication = ReadInt();

            Console.Write("Собівартість: ");
            book.Cost = ReadDecimal();

            Console.Write("Ціна продажу: ");
            book.Price = ReadDecimal();
            if (book.Price < book.Cost)
            {
                Console.WriteLine("Помилка: ціна продажу не може бути меншою за собівартість.");
                return;
            }

            Console.Write("Чи є книга продовженням? (1 – так, 0 – ні): ");
            book.IsSequel = Console.ReadLine() == "1";
            if (book.IsSequel)
            {
                Console.Write("ID книги, від якої є продовження: ");
                book.SequelToBookId = ReadInt();
            }

            context.Books.Add(book);
            if (context.SaveChanges() > 0)
                Console.WriteLine("Книгу додано успішно.");
            else
                Console.WriteLine("Не вдалося додати книгу.");
        }

        static void EditBook(BookstoreContext context)
        {
            Console.WriteLine("\n--- Редагування книги ---");
            Console.Write("Введіть ID книги: ");
            int bookId = ReadInt();

            var book = context.Books.Find(bookId);
            if (book == null)
            {
                Console.WriteLine("Книга не знайдена.");
                return;
            }

            Console.Write("Нова назва (залиште порожнім для пропуску): ");
            var input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
                book.Title = input;

            Console.Write("Новий ID автора (0 для пропуску): ");
            int newAuthorId = ReadInt();
            if (newAuthorId != 0)
                book.AuthorId = newAuthorId;

            Console.Write("Новий ID видавництва (0 для пропуску): ");
            int newPublisherId = ReadInt();
            if (newPublisherId != 0)
                book.PublisherId = newPublisherId;

            Console.Write("Новий ID жанру (0 для пропуску): ");
            int newGenreId = ReadInt();
            if (newGenreId != 0)
                book.GenreId = newGenreId;

            Console.Write("Нова кількість сторінок (0 для пропуску): ");
            int newPages = ReadInt();
            if (newPages != 0)
                book.Pages = newPages;

            Console.Write("Новий рік видання (0 для пропуску): ");
            int newYear = ReadInt();
            if (newYear != 0)
                book.YearOfPublication = newYear;

            Console.Write("Нова собівартість (0 для пропуску): ");
            decimal newCost = ReadDecimal();
            if (newCost != 0)
                book.Cost = newCost;

            Console.Write("Нова ціна продажу (0 для пропуску): ");
            decimal newPrice = ReadDecimal();
            if (newPrice != 0)
            {
                if (newPrice < book.Cost)
                {
                    Console.WriteLine("Помилка: ціна продажу не може бути меншою за собівартість.");
                    return;
                }
                book.Price = newPrice;
            }

            Console.Write("Чи є книга продовженням? (1 – так, 0 – ні, порожньо для пропуску): ");
            var sequelInput = Console.ReadLine();
            if (sequelInput == "1")
            {
                book.IsSequel = true;
                Console.Write("ID книги, від якої є продовження: ");
                book.SequelToBookId = ReadInt();
            }
            else if (sequelInput == "0")
            {
                book.IsSequel = false;
                book.SequelToBookId = null;
            }

            if (context.SaveChanges() > 0)
                Console.WriteLine("Книгу оновлено успішно.");
            else
                Console.WriteLine("Не вдалося оновити книгу.");
        }

        static void DeleteBook(BookstoreContext context)
        {
            Console.WriteLine("\n--- Видалення книги ---");
            Console.Write("Введіть ID книги: ");
            int bookId = ReadInt();

            var book = context.Books.Find(bookId);
            if (book == null)
            {
                Console.WriteLine("Книга не знайдена.");
                return;
            }

            context.Books.Remove(book);
            if (context.SaveChanges() > 0)
                Console.WriteLine("Книгу видалено.");
            else
                Console.WriteLine("Не вдалося видалити книгу.");
        }

        static void SearchBooks(BookstoreContext context)
        {
            Console.WriteLine("\n--- Пошук книг ---");
            Console.Write("Назва (або порожньо для пропуску): ");
            string title = Console.ReadLine();

            Console.Write("ПІБ автора (або порожньо для пропуску): ");
            string author = Console.ReadLine();

            Console.Write("ID жанру (0 для пропуску): ");
            int genreId = ReadInt();
            int? genreFilter = genreId == 0 ? (int?)null : genreId;

            var query = context.Books.Include(b => b.Author)
                                     .Include(b => b.Genre)
                                     .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(b => b.Title.Contains(title));
            if (!string.IsNullOrWhiteSpace(author))
                query = query.Where(b => b.Author.FullName.Contains(author));
            if (genreFilter.HasValue)
                query = query.Where(b => b.GenreId == genreFilter.Value);

            var results = query.ToList();
            if (results.Count == 0)
            {
                Console.WriteLine("Книг не знайдено.");
            }
            else
            {
                Console.WriteLine("\nЗнайдені книги:");
                foreach (var b in results)
                {
                    Console.WriteLine($"ID: {b.BookId}, Назва: {b.Title}, Автор: {b.Author.FullName}, Рік: {b.YearOfPublication}, Ціна: {b.Price}");
                }
            }
        }

        static int ReadInt()
        {
            int value;
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.Write("Некоректне число. Спробуйте ще раз: ");
            }
            return value;
        }

        static decimal ReadDecimal()
        {
            decimal value;
            while (!decimal.TryParse(Console.ReadLine(), out value))
            {
                Console.Write("Некоректне число. Спробуйте ще раз: ");
            }
            return value;
        }
    }
}
