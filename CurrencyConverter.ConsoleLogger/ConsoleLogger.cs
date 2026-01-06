// author: Danila "akshin_" Axyonov

namespace CurrencyConverter.ConsoleLogger;

/// <summary>
/// Статический класс, предоставляющий удобный функционал для вывода информации в консоль в некоторых случаях.
/// </summary>
public static class ConsoleLogger
{
    /// <summary>
    /// Выводит в консоль конкатенацию 3-ёх строк, окрашивая срединную (т.е. 2-ую) в заданный цвет.
    /// </summary>
    /// <param name="beginning">Начальная (1-ая) строка.</param>
    /// <param name="middle">Срединная (2-ая) строка.</param>
    /// <param name="middleColor">Цвет, в который окрашивается срединная (т.е. 2-ая) строка.</param>
    /// <param name="ending">Конечная (3-ья) строка.<br/>По умолчанию пуста.</param>
    public static void MiddleColoredWrite(string beginning, string middle, ConsoleColor middleColor, string ending = "")
    {
        Console.Write(beginning);
        Console.ForegroundColor = middleColor;
        Console.Write(middle);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(ending);
    }

    /// <summary>
    /// Выводит в консоль конкатенацию 3-ёх строк, окрашивая срединную (т.е. 2-ую) в заданный цвет, и выполняет перевод строки.
    /// </summary>
    /// <param name="beginning">Начальная (1-ая) строка.</param>
    /// <param name="middle">Срединная (2-ая) строка.</param>
    /// <param name="middleColor">Цвет, в который окрашивается срединная (т.е. 2-ая) строка.</param>
    /// <param name="ending">Конечная (3-ья) строка.<br/>По умолчанию пуста.</param>
    public static void MiddleColoredWriteLine(string beginning, string middle, ConsoleColor middleColor, string ending = "")
    {
        MiddleColoredWrite(beginning, middle, middleColor, ending);
        Console.WriteLine();
    }

    /// <summary>
    /// Выводит в консоль информацию об исключении в следующем формате:
    /// <br/><b>Тип исключения: [Текст исключения]</b>
    /// <br/>, - а также заданную строку-окончание.
    /// </summary>
    /// <param name="e">Объект исключения (Exception).</param>
    /// <param name="ending">Строка-окончание, выводящаяся после информации об исключении.<br/>По умолчанию пуста.</param>
    public static void ExceptionWrite(Exception e, string ending = "")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(e.GetType().ToString());
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($": [{e.Message}]" + ending);
    }

    /// <summary>
    /// Выводит в консоль информацию об исключении в следующем формате:
    /// <br/><b>Тип исключения: [Текст исключения]</b>
    /// <br/>, - а также заданную строку-окончание, и выполняет перенос строки.
    /// </summary>
    /// <param name="e">Объект исключения (Exception).</param>
    /// <param name="ending">Строка-окончание, выводящаяся после информации об исключении.<br/>По умолчанию пуста.</param>
    public static void ExceptionWriteLine(Exception e, string ending = "")
    {
        ExceptionWrite(e, ending);
        Console.WriteLine();
    }
}
