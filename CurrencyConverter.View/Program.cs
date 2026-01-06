// author: Danila "akshin_" Axyonov

using System.Globalization;

namespace CurrencyConverter.View;

/// <summary>
/// Основной класс консольной утилиты.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Заголовочный текст, появляющийся в самом начале при запуске программы.
    /// </summary>
    private const string _HEADER = 
"""
`CurrencyConverter` (2026).
Author: Danila \"akshin_\" Axyonov.
Type `h` or `help` in any case to display help.
""";

    /// <summary>
    /// Набор команд, завершающих выполнение программы.
    /// </summary>
    private static readonly HashSet<string> _EXIT_COMMANDS = new HashSet<string>() {"exit", "q", "quit"};

    /// <summary>
    /// Набор команд, отображающих справочный текст.
    /// </summary>
    private static readonly HashSet<string> _HELP_COMMANDS = new HashSet<string> {"h", "help"};

    /// <summary>
    /// Заголовок справочного текста.
    /// </summary>
    private const string _HELP_TEXT_HEADER = "Available commands (in any case):";

    /// <summary>
    /// Тело справочного текста.
    /// </summary>
    /// <remarks>Перечисляет доступные команды.</remarks>
    private const string _HELP_TEXT_BODY =
"""
1   exit, q, quit - terminates the program.

2   h, help - displays this text (a list of available commands).

3   l, list - displays a list of available currencies.

4   update [_actual_date_] - updates an XML-file with currency rates
            actual for the latest day, not exceeding _actual_date_;
        Remark 1: the required format for _actual_date_ is dd.MM.yyyy;
        Remark 2: _actual_date_ is an optional parameter; its default
                value is "today" (means the day when the command was
                called);
        Example: UpDaTe 01.02.2003

5   load - loads all currencies from an XML-file to a converter and,
           if succeed, deletes the old ones.

6   reload [_actual_date_] - equals to "update [_actual_date_]" and
            "load" commands sequential calling;
        Remarks 1 & 2: see remarks 1 & 2 for "update [_actual_date_]"
                command.

7   _amount_ _char_code_from_ {to / -> / =>} _char_code_to_ -
            converts _amount_ units of currency with char code
            equals to _char_code_from_ to currency with char code
            equals to _char_code_to_;
        Remark 1: _amount_ must be a valid real number; the decimal
                separator can be either a dot or a comma;
        Remark 2: _char_code_from_ and _char_code_to_ must consist of
                3 Latin letters;
        Example 1: 19999.99 rub -> kzt
        Example 2: 0,25 USd To JPy

8   _char_code_from_ {to / -> / =>} _char_code_to_ - calculates the
            F / T rate where F is the currency with char code equals
            to _char_code_from_ and T is the currency with char code
            equals to _char_code_to_;
        Remark 1: this command is equal to calling "1 _char_code_from_
                {to / -> / =>} _char_code_to_";
        Remark 2: see remark 2 for the previous command;
        Example 1: BYB TO RUB
        Example 2: rSd => tRy
""";

    /// <summary>
    /// Набор команд, отображающих список доступных валют.
    /// </summary>
    private static readonly HashSet<string> _LIST_COMMANDS = new HashSet<string> {"l", "list"};

    /// <summary>
    /// Набор `слов`, эквивалентных слову "to" при команде подсчёта курса или конвертации.
    /// </summary>
    private static readonly HashSet<string> _TO_COMMANDS = new HashSet<string> {"to", "->", "=>"};

    /// <summary>
    /// Точка входа в программу.
    /// </summary>
    /// <param name="args">Аргументы, передаваемые программе при запуске.</param>
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                "\n",
                "Error",
                ConsoleColor.Red,
                ": unknown CLI-arguments: \""
            );
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(args[0]);
            for (int i = 1; i < args.Length; ++i)
                Console.Write(' ' + args[i]);
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\".");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No CLI-arguments was expected.\n");
            Console.ForegroundColor = ConsoleColor.White;

            return;
        }

        CBRService.CBRClient.AddDesktopNETFrameworkEncodings();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(_HEADER);
        Console.ForegroundColor = ConsoleColor.White;

        var converter = new Model.CurrencyConverter();
        try
        {
            Builder.CurrencyConverterBuilder.UpdateRatesFile();
            converter = Builder.CurrencyConverterBuilder.LoadConverterFromRatesFile();
        }
        catch (Exception e)
        {
            ConsoleLogger.ConsoleLogger.ExceptionWriteLine(e, ".");
        }

        while (true)
        {
            Console.Write("\n>>> ");
            var input = Console.ReadLine()!;
            var inputs = input.Split().Where(S => S.Length > 0).ToArray();
            string cmd,
                   cmdLower;

            bool success = false;
            switch (inputs.Length)
            {
                case 1:
                    cmd = inputs[0];
                    cmdLower = cmd.ToLower();

                    if (_EXIT_COMMANDS.Contains(cmdLower))
                        return;
                    else if (_HELP_COMMANDS.Contains(cmdLower))
                    {
                        success = true;

                        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                            "\n",
                            _HELP_TEXT_HEADER,
                            ConsoleColor.Yellow
                        );
                        Console.WriteLine('\n'+ _HELP_TEXT_BODY);
                    }
                    else if (_LIST_COMMANDS.Contains(cmdLower))
                    {
                        success = true;

                        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                            "\n",
                            "Available currencies:",
                            ConsoleColor.Magenta
                        );

                        foreach (var currency in converter)
                            Console.WriteLine(currency.GetRateString());
                        
                        ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                            "Total: ",
                            converter.Length.ToString(),
                            ConsoleColor.Blue,
                            ". | "
                        );
                        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                            "Actual date: ",
                            converter.ActualDate.ToString(Model.CurrencyConverter.ActualDateFormat),
                            ConsoleColor.Blue,
                            "."
                        );
                    }
                    else
                    {
                        try
                        {
                            if (cmdLower == "update" || cmdLower == "reload")
                            {
                                success = true;

                                Builder.CurrencyConverterBuilder.UpdateRatesFile();
                            }
                            if (cmdLower == "load" || cmdLower == "reload")
                            {
                                success = true;

                                converter = Builder.CurrencyConverterBuilder.LoadConverterFromRatesFile();
                            }
                        }
                        catch (Exception e)
                        {
                            ConsoleLogger.ConsoleLogger.ExceptionWriteLine(e, ".");
                        }
                    }

                    break;

                case 2:
                    cmd = inputs[0];
                    cmdLower = cmd.ToLower();

                    if (cmdLower != "update" && cmdLower != "reload")
                        break;
                    
                    success = true;

                    var actualDateString = inputs[1];

                    DateTime actualDate;
                    bool actualDateSuccess = DateTime.TryParseExact(actualDateString, Model.CurrencyConverter.ActualDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out actualDate);
                    if (!actualDateSuccess)
                    {
                        ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                            "\n",
                            "Error",
                            ConsoleColor.Red,
                            ": "
                        );
                        ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                            "\"",
                            cmd,
                            ConsoleColor.Yellow,
                            "\" command was detected but the second argument "
                        );
                        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                            "\"",
                            actualDateString,
                            ConsoleColor.Yellow,
                            "\" can't be parsed as a date."
                        );
                        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                            "Please, provide the date in accordance with the following format: ",
                            Model.CurrencyConverter.ActualDateFormat,
                            ConsoleColor.Blue,
                            "."
                        );

                        break;
                    }

                    try
                    {
                        if (cmdLower == "update" || cmdLower == "reload")
                            Builder.CurrencyConverterBuilder.UpdateRatesFile(actualDate);
                        if (cmdLower == "reload")
                            converter = Builder.CurrencyConverterBuilder.LoadConverterFromRatesFile();
                    }
                    catch (Exception e)
                    {
                        ConsoleLogger.ConsoleLogger.ExceptionWriteLine(e, ".");
                    }

                    break;
                
                case 3:
                case 4:
                    cmd = inputs.Length == 3 ? inputs[1] : inputs[2];
                    cmdLower = cmd.ToLower();

                    if (!_TO_COMMANDS.Contains(cmdLower))
                        break;

                    success = true;

                    string cmdName,
                           amountFromString,
                           charCodeFrom,
                           charCodeFromOrder,
                           charCodeTo,
                           charCodeToOrder;
                    if (inputs.Length == 3)
                    {
                        cmdName = "rate calculating";
                        amountFromString = "1";
                        charCodeFrom = inputs[0];
                        charCodeFromOrder = "first";
                        charCodeTo = inputs[2];
                        charCodeToOrder = "third";
                    }
                    else
                    {
                        cmdName = "converting";
                        amountFromString = inputs[0];
                        charCodeFrom = inputs[1];
                        charCodeFromOrder = "second";
                        charCodeTo = inputs[3];
                        charCodeToOrder = "fourth";
                    }
                    var amountFromStringFixed = amountFromString.Replace(',', '.');
                    var charCodeFromFixed = charCodeFrom.ToUpper();
                    var charCodeToFixed = charCodeTo.ToUpper();

                    decimal amountFrom;
                    bool amountFromSuccess = decimal.TryParse(amountFromStringFixed, out amountFrom);
                    if (!amountFromSuccess)
                    {
                        ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                            "\n",
                            "Error",
                            ConsoleColor.Red,
                            ": "
                        );
                        ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                            "converting command \"",
                            cmd,
                            ConsoleColor.Yellow,
                            "\" was detected but the first argument "
                        );
                        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                            "\"",
                            amountFromString,
                            ConsoleColor.Yellow,
                            "\" can't be parsed as a number.\nPlease, provide the number in the correct format."
                        );

                        break;
                    }

                    var checkingCharCodes = new Tuple<string, string, string>[]
                    {   
                        new Tuple<string, string, string>(charCodeFrom, charCodeFromFixed, charCodeFromOrder),
                        new Tuple<string, string, string>(charCodeTo, charCodeToFixed, charCodeToOrder)
                    };
                    foreach (var (charCode, charCodeFixed, charCodeOrder) in checkingCharCodes)
                        if (!Model.Currency.IsAvailableCharCode(charCodeFixed))
                        {
                            ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                                "\n",
                                "Error",
                                ConsoleColor.Red,
                                ": "
                            );
                            ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                                cmdName + " command \"",
                                cmd,
                                ConsoleColor.Yellow,
                                "\" was detected but the " + charCodeOrder + " argument "
                            );
                            ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                                "\"",
                                charCode,
                                ConsoleColor.Yellow,
                                "\" can't be parsed as a currency char code."
                            );
                            ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                                "Please, provide the currency char code in the form of ",
                                "3 Latin letters",
                                ConsoleColor.Blue,
                                " in any case."
                            );

                            break;
                        }

                    try
                    {
                        var amountTo = converter.Convert(amountFrom, charCodeFromFixed, charCodeToFixed);
                        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                            $"\n{amountFrom} {charCodeFromFixed} = ",
                            $"{amountTo:F4}",
                            ConsoleColor.Magenta,
                            $" {charCodeToFixed}."
                        );
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        ConsoleLogger.ConsoleLogger.ExceptionWriteLine(e, ".");
                    }

                    break;
            }

            if (!success)
            {
                ConsoleLogger.ConsoleLogger.MiddleColoredWrite(
                    "\n",
                    "Error",
                    ConsoleColor.Red,
                    ": "
                );
                if (inputs.Length == 0)
                    Console.Write("no command was recieved");
                else
                {
                    Console.Write("unknown command: \"");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(inputs[0]);
                    for (int i = 1; i < inputs.Length; ++i)
                        Console.Write(' ' + inputs[i]);
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.Write('\"');
                }
                Console.WriteLine('.');

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Type `h` or `help` in any case to display help.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
