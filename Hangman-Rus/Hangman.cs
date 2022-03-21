using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hangman_Rus
{
    class Hangman
    {
        string _Word { get; set; }
        int _WordLength { get; set; }
        int _Counter { get; set; } = 0;
        int _FalseAnswerCounter { get; set; } = 0;
        List<char> ErrorList = new List<char>();

        const int maxTries = 6;

        StringBuilder _Board { get; set; }

        enum States { Exists, NotExists, AlreadyTaken }


        public void Start()
        {
            bool firstStart = true;
            bool restart = false;

            do
            {
                Console.Clear();

                if (firstStart)
                {
                    Console.WriteLine("Добро пожаловать в Виселицу!\n");
                    firstStart = false;
                }

                _Word = GetRndWord();
                _WordLength = _Word.Length;

                //Reseting counters in case we restart the game
                _Counter = 0;
                _FalseAnswerCounter = 0;
                ErrorList = new List<char>();

                Console.WriteLine($"Длина слова: {_WordLength}");
                GenerateBoard();

                bool gameRunning = _Counter != _WordLength;
                bool triesNotExceeded = _FalseAnswerCounter < maxTries;

                do
                {
                    Console.WriteLine($"\nОсталось попыток: {maxTries - _FalseAnswerCounter}");
                    DisplayErrors();
                    DrawHangman();
                    Console.WriteLine();
                    DrawBoard();
                    Console.WriteLine("\nВыберите букву!");
                    Console.Write("> ");

                    bool isValid;
                    string cheatCode = "chlenix_on";
                    char input;

                    do
                    {
                        string ans = Console.ReadLine();
                        //isValid = char.TryParse(ans.ToLower(), out input) && !string.IsNullOrWhiteSpace(ans) && !ans.Any(char.IsSeparator) && !ans.Any(char.IsPunctuation) && !ans.Any(char.IsDigit) && !ans.Any(char.IsSymbol);
                        isValid = char.TryParse(ans.ToLower(), out input) && !string.IsNullOrWhiteSpace(ans) && ans.Any(char.IsLetter);

                        if (!isValid && ans != cheatCode)
                            Console.WriteLine("Невалидный ввод! Попробуйте еще раз");
                        else if (ans == cheatCode)
                        {
                            Console.WriteLine("\nChlenix 2.0 activated");
                            Console.WriteLine($"The word is: {_Word}");
                        }

                    } while (!isValid);

                    States checkLetter = CheckLetter(input);

                    if (checkLetter == States.Exists)
                    {
                        Console.Clear();
                        if (_Counter != _WordLength)
                            Console.WriteLine($"Отлично! Откройте букву '{input}'.");

                        else
                        {
                            Console.WriteLine($"Поздравляю! Вы полностью отгадали слово '{_Word}'!");
                            gameRunning = false;
                            DrawHangman();
                            Console.WriteLine();
                            DrawBoard();
                        }


                    }
                    else if (checkLetter == States.NotExists)
                    {
                        Console.Clear();
                        Console.WriteLine("К сожалению, такой буквы нет!");
                        //Console.WriteLine("Пробуйте еще!");
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Вы уже открыли эту букву! Выберите другую");
                    }


                    if (_FalseAnswerCounter >= maxTries)
                    {
                        DrawHangman();
                        DrawBoard();
                        Console.WriteLine("\nУвы, Вы проиграли!");
                        Console.WriteLine($"Загадонное слово было - '{_Word}'");
                        gameRunning = false;
                    }


                    if (!gameRunning)
                    {
                        Console.WriteLine("\nЖелаете попробовать еще раз?");
                        Console.WriteLine("('д' - Да, 'н' - нет)");
                        isValid = false;

                        do
                        {
                            string lastReply = Console.ReadLine();

                            if (lastReply.ToLower() == "д")
                            {
                                Console.WriteLine("Превосходно! Давайте сыграем еще раз!");
                                isValid = true;
                                restart = true;
                            }
                            else if (lastReply.ToLower() == "н")
                            {
                                Console.WriteLine("Принято! Хорошего вам дня!");
                                isValid = true;
                                restart = false;
                            }
                            else
                            {
                                Console.WriteLine("Невалидный ввод! Попробуйте еще раз");
                                isValid = false;
                            }

                        } while (!isValid);

                    }

                } while (gameRunning && triesNotExceeded);

            } while (restart);

            //Console.ReadKey();
        }

        private string GetRndWord()
        {
            try
            {
                int lineCount = File.ReadLines("WordsStockRus.txt").Count();

                Random rnd = new Random();
                int randomLine = rnd.Next(0, lineCount + 1);

                string guessWord = File.ReadLines("WordsStockRus.txt").ElementAtOrDefault(randomLine);

                if (string.IsNullOrEmpty(guessWord))
                {
                    throw new NullReferenceException("*Couldn't get a random word from file*");

                }

                if (guessWord.Any(char.IsDigit) || guessWord.Any(char.IsSeparator) || guessWord.Any(char.IsPunctuation))
                {
                    throw new FileLoadException("*File contains invalid words*");
                }

                guessWord.ToLower();
                Console.WriteLine($"Загруженно слов из словаря: {lineCount}");
                //Console.WriteLine($"Word is: {guessWord}");

                return guessWord;

            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(FileNotFoundException))
                {
                    Console.WriteLine("*Coulnd't find a dictionary file.*");
                    Console.WriteLine(ex);

                    Console.ReadKey();
                    Environment.Exit(-1);
                }
                else
                {
                    Console.WriteLine("**Dictionary file should contain only one valid word at a line");
                    Console.WriteLine("without any numbers, separator/punctuation or any other invalid characters.**");
                    Console.WriteLine();
                    Console.WriteLine(ex);

                    Console.ReadKey();
                    Environment.Exit(-1);
                }
                return null;
            }

        }

        private void DisplayErrors()
        {

            string errors = "";

            Console.Write($"Ошибки ({ErrorList.Count}): ");
            foreach (char c in ErrorList)
            {
                if (ErrorList[0] != c)
                    errors += "," + c;
                else
                    errors += c;
            }

            Console.WriteLine(errors);
        }

        private States CheckLetter(char ch)
        {
            char[] letters = _Word.ToCharArray();
            bool foundAny = false;

            for (int i = 0; i < letters.Length; i++)
            {
                if (letters[i] == ch && _Board[i * 2] != ch)
                {
                    _Board[i * 2] = ch;
                    _Counter++;
                    foundAny = true;
                }
                else if (_Board[i * 2] == ch)
                {
                    Console.WriteLine("Вы уже открыли эту букву! Выберите другую букву");
                    return States.AlreadyTaken;
                }
            }

            if (foundAny)
                return States.Exists;
            else
            {
                if (!ErrorList.Contains(ch))
                {
                    ErrorList.Add(ch);
                    _FalseAnswerCounter++;
                }

                return States.NotExists;
            }

        }

        private void GenerateBoard()
        {

            StringBuilder board = new StringBuilder();

            for (int i = 0; i < _WordLength; i++)
            {
                board.Append("_ ");
            }
            board.Remove(board.Length - 1, 1); // Removing last space in string

            _Board = board;

            //DrawBoard();
        }

        private void DrawBoard()
        {
            Console.WriteLine(_Board.ToString());
        }

        private void DrawHangman()
        {

            int c = ErrorList.Count();

            string s0 = @"
      ____________________
      |   |          \   |
      |  _|_          \  |
                       \ |
                        \|
                         |
                         |
                         |
                         |
                         |
                         |
                         |
            _____________|";

            string s1 = @"
      ____________________
      |   |          \   |
      |  _|_          \  |
        /   \          \ |
        \___/           \|
                         |
                         |
                         |
                         |
                         |
                         |
                         |
            _____________|";

            string s2 = @"
      ____________________
      |   |          \   |
      |  _|_          \  |
        /   \          \ |
        \___/           \|
          |              |
          |              |
                         |
                         |
                         |
                         |
                         |
            _____________|";

            string s3 = @"
      ____________________
      |   |          \   |
      |  _|_          \  |
        /   \          \ |
        \___/           \|
       \  |              |
        \_|              |
                         |
                         |
                         |
                         |
                         |
            _____________|";

            string s4 = @"
      ____________________
      |   |          \   |
      |  _|_          \  |
        /   \          \ |
        \___/           \|
       \  |  /           |
        \_|_/            |
                         |
                         |
                         |
                         |
                         |
            _____________|";

            string s5 = @"
      ____________________
      |   |          \   |
      |  _|_          \  |
        /   \          \ |
        \___/           \|
       \  |  /           |
        \_|_/            |
          |              |
         /               |
        /                |
                         |
                         |
            _____________|";

            string s6 = @"
      ____________________
      |   |          \   |
      |  _|_          \  |
        /x x\          \ |
        \ - /           \|
       \  |  /           |
        \_|_/            |
          |              |
         / \             |
        /   \            |
                         |
                         |
            _____________|";


            switch (c)
            {
                case 0:
                    Console.WriteLine(s0);
                    break;
                case 1:
                    Console.WriteLine(s1);
                    break;
                case 2:
                    Console.WriteLine(s2);
                    break;
                case 3:
                    Console.WriteLine(s3);
                    break;
                case 4:
                    Console.WriteLine(s4);
                    break;
                case 5:
                    Console.WriteLine(s5);
                    break;
                case 6:
                    Console.WriteLine(s6);
                    break;
                default:
                    Console.WriteLine("O_O");
                    break;
            }

        }



    }

}
