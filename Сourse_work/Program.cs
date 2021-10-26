using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace Сourse_work
{
    class Program
    {
        static int PositionCursore = 1;
        //список строк подкаталогов и файлов
        static List<string> listCatalog = new List<string>();
        //количество файлов
        static int FileCnt = 0;
        static DirectoryInfo directory;
        static string dirName;
        static Configuration configFile;
        static KeyValueConfigurationCollection settings;
        static int Size = 0;

        public static void Main(string[] args)
        {
            Thread primarythread = Thread.CurrentThread;
            //многопоточность
            ThreadStart readLineStart = new ThreadStart(readLine);
            Thread myThread = new Thread(new ThreadStart(readLineStart));
            ThreadStart myThreadKeyStart = new ThreadStart(keyread);
            Thread myThreadKey = new Thread(new ThreadStart(myThreadKeyStart));

            myThread.Priority = ThreadPriority.Highest;
            myThreadKey.Priority = ThreadPriority.Lowest;
            //сохранение конфигурации
            configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            settings = configFile.AppSettings.Settings;

            if (settings["Direction"] == null)
            {
                settings.Add("Direction", "C:\\");
                //Вывод подкаталогов
                dirName = "C:\\";
            }
            else {
                dirName = settings["Direction"].Value;
            }
            //название проекта
            Console.Title = "Курсовая работа \"Файловый менеджер\"";
            //размер консоли по всему экрану
            Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            //консоль синего цвета
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;
            Size = Console.LargestWindowHeight;

            OutConcole(dirName, ref FileCnt, ref listCatalog, ref PositionCursore);

            FileCnt = OutDirectory(dirName, ref listCatalog);
            directory = new DirectoryInfo(dirName);
            //Вывод информации
            if (directory.Exists) // Если указанная директория существует, то выводим о ней информацию.
            {
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 14);
                Console.WriteLine($"Имя {directory.FullName}");
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 13);
                Console.WriteLine($"Время создания {directory.CreationTime}");
            }
            // обработка нажатий кнопок
            myThreadKey.Start();
            myThread.Start();

        }

        public static void readLine()
        {
            string name;
            bool end = true;
            Console.SetCursorPosition(3, Size - 1);
            do
            {
                Console.SetCursorPosition(3, Size - 1);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(dirName);
                name = Console.ReadLine();

                //выбор папки
                if (name.IndexOf("cd")!=-1)
                {
                    dirName = name.Substring("cd".Length, name.Length - 2);
                    settings.Remove("Direction");
                    //сохранение новых конфигураций
                    settings.Add("Direction", dirName);
                    OutConcole(dirName, ref FileCnt, ref listCatalog, ref PositionCursore);
                    configFile.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                    
                }
                //удаление файла и каталога
                if (name.IndexOf("rm") != -1)
                {
                    FileInfo fileInf = new FileInfo(name.Substring("rm".Length, name.Length - 2));
                    if (fileInf.Exists)
                    {
                        fileInf.Delete();
                        OutConcole(dirName, ref FileCnt, ref listCatalog, ref PositionCursore);
                    }
                    else
                    {
                        try
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(name.Substring("rm".Length, name.Length - 2));
                            dirInfo.Delete(true);
                            OutConcole(dirName, ref FileCnt, ref listCatalog, ref PositionCursore);
                        }
                        catch (Exception ex)
                        {
                            Console.SetCursorPosition(3, Size - 1);
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                //копирование каталога или файла
                if (name.IndexOf("cp") != -1)
                {
                    FileInfo fileInf = new FileInfo(name.Substring("cp".Length, name.IndexOf(' ')-1));
                    if (fileInf.Exists)
                    {
                        string s = name.Substring(name.IndexOf(' ')-1, name.Length);
                        fileInf.CopyTo(name.Substring(name.IndexOf(' '), name.Length-2), true);
                    }
                }              

                switch (name)
                {
                    case "Help":
                        Console.WriteLine("cd -Выводит список файлов в каталоге\ncd-Переход в каталог\nrm-Удаление файла по имени или каталога\ncp-Копирование файла или каталога");
                        break;
                    case "Exit":
                        end = false;
                        break;
                    default:
                        //keyread();
                        break;
                }
            } while (end != false);

        }

        public static void keyread()
        {
            Thread threadSecond = Thread.CurrentThread;
            ConsoleKey key = Console.ReadKey().Key;

            while (key != ConsoleKey.Escape)
            {
                Console.CursorVisible = true;

                switch (key)
                {
                    case ConsoleKey.UpArrow://клавиша вверх
                        if (PositionCursore > 0)
                        {
                            Console.SetCursorPosition(3, PositionCursore);
                            Console.BackgroundColor = ConsoleColor.Blue;
                            if (PositionCursore <= listCatalog.Count - FileCnt) Console.ForegroundColor = ConsoleColor.White;
                            else Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine(listCatalog[PositionCursore - 1]);
                            PositionCursore--;
                            Console.SetCursorPosition(3, PositionCursore);
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.White;
                            if (PositionCursore == 0) Console.WriteLine("...");
                            else Console.WriteLine(listCatalog[PositionCursore - 1]);

                            if (PositionCursore != 0 && PositionCursore <= listCatalog.Count - FileCnt)
                            {
                                directory = new DirectoryInfo(dirName + listCatalog[PositionCursore - 1]);
                                //Вывод информации
                                OutInfoDirectory(directory);
                            }
                            if (PositionCursore > listCatalog.Count - FileCnt) OutInfoFile(dirName + listCatalog[PositionCursore - 1]);
                        }
                        break;
                    case ConsoleKey.DownArrow://клавиша вниз
                        if (PositionCursore < Console.LargestWindowHeight - 15 && PositionCursore < listCatalog.Count)
                        {
                            Console.SetCursorPosition(3, PositionCursore);
                            Console.BackgroundColor = ConsoleColor.Blue;
                            if (PositionCursore <= listCatalog.Count - FileCnt) Console.ForegroundColor = ConsoleColor.White;
                            else Console.ForegroundColor = ConsoleColor.Black;
                            if (PositionCursore == 0) Console.WriteLine("...");
                            else Console.WriteLine(listCatalog[PositionCursore - 1]);
                            PositionCursore++;
                            Console.SetCursorPosition(3, PositionCursore);
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(listCatalog[PositionCursore - 1]);
                            if (PositionCursore != 0 && PositionCursore <= listCatalog.Count - FileCnt)
                            {
                                directory = new DirectoryInfo(dirName + listCatalog[PositionCursore - 1]);
                                //Вывод информации
                                OutInfoDirectory(directory);
                            }
                            if (PositionCursore > listCatalog.Count - FileCnt) OutInfoFile(dirName + listCatalog[PositionCursore - 1]);
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (PositionCursore != 0)
                        {
                            dirName = dirName + listCatalog[PositionCursore - 1];
                            //очищение конфигурации
                            settings.Remove("Direction");
                            //сохранение новых конфигураций
                            settings.Add("Direction", dirName);
                            OutConcole(dirName, ref FileCnt, ref listCatalog, ref PositionCursore);
                            configFile.Save(ConfigurationSaveMode.Modified);
                            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                        }
                        else
                        {
                            for (int i = settings["Direction"].Value.Length - 2; i > 0; i--)
                            {
                                if (settings["Direction"].Value[i] == '\\')
                                {
                                    dirName = settings["Direction"].Value.Substring(0, i + 1);
                                    //очищение конфигурации
                                    settings.Remove("Direction");
                                    //сохранение новых конфигураций
                                    settings.Add("Direction", dirName);
                                    OutConcole(dirName, ref FileCnt, ref listCatalog, ref PositionCursore);
                                    configFile.Save(ConfigurationSaveMode.Modified);
                                    ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        Console.SetCursorPosition(Console.CursorLeft,Size-1);
                        break;
                }                
                key = Console.ReadKey().Key;
            }
        }

        //метод вывода подкаталогов и файлов
        public static int OutDirectory(string dirName, ref List<string> list)
        {
            Thread thread = Thread.CurrentThread;

            Console.SetCursorPosition(3, 0);
            Console.WriteLine("...");
            list.Clear();
            int FileCnt = 0;

            if (Directory.Exists(dirName))
            {
                try
                {
                    string[] dirs = Directory.GetDirectories(dirName);
                    foreach (string s in dirs)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(3, Console.CursorTop);
                        Console.WriteLine(s.Replace(dirName, ""));
                        list.Add(s.Replace(dirName, ""));
                    }
                    string[] files = Directory.GetFiles(dirName);
                    FileCnt = files.Length;
                    foreach (string s in files)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(3, Console.CursorTop);
                        Console.WriteLine(s.Replace(dirName, ""));
                        list.Add(s.Replace(dirName, ""));
                    }
                }
                catch (Exception ex)
                {
                    Console.SetCursorPosition(3, Size - 1);
                    Console.WriteLine(ex.Message);
                }
            }
            Console.SetCursorPosition(3, Size-1);
            return FileCnt;
        }
        //метод вывода информации о каталоге
        public static void OutInfoDirectory(DirectoryInfo dirName)
        {
            if (dirName.Exists) // Если указанная директория существует, то выводим о ней информацию.
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 14);
                Console.WriteLine($"Имя {dirName.FullName}                                                   ");
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 13);
                Console.WriteLine($"Время создания {dirName.CreationTime}                                    ");
            }
        }
        public static void OutInfoFile(string path)
        {
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 14);
                Console.WriteLine($"Имя файла: {fileInf.Name}                                                 ");
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 13);
                Console.WriteLine($"Время создания: {fileInf.CreationTime}                                    ");
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 12);
                Console.WriteLine($"Размер: {fileInf.Length}                                                  ");
            }
        }

        public static void OutConcole(string DirName, ref int FileCnt, ref List<string> listCatalog, ref int PositionCursore)
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Clear();
            //прорисовка экрана
            Console.WriteLine($"{new string('=', Console.LargestWindowWidth - 2)}");
            Console.SetCursorPosition(0, 1);
            for (int k = 0; k < Console.LargestWindowHeight - 14; k++) Console.WriteLine($"‖{new string(' ', Console.LargestWindowWidth - 3)}‖");
            Console.WriteLine($"{new string('=', Console.LargestWindowWidth - 2)}");
            for (int k = 0; k < 12; k++) Console.WriteLine($"‖{new string(' ', Console.LargestWindowWidth - 3)}‖");
            Console.WriteLine($"{new string('=', Console.LargestWindowWidth - 2)}");

            //Вывод подкаталогов
            FileCnt = OutDirectory(DirName, ref listCatalog);

            var directory = new DirectoryInfo(DirName);
            //Вывод информации
            if (directory.Exists) // Если указанная директория существует, то выводим о ней информацию.
            {
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 14);
                Console.WriteLine($"Имя {directory.FullName}");
                Console.SetCursorPosition(3, Console.LargestWindowHeight - 13);
                Console.WriteLine($"Время создания {directory.CreationTime}");
            }
        }
    }
}
