// author: Danila "akshin_" Axyonov

using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace CurrencyConverter.Builder;

/// <summary>
/// Статический класс, связывающий конвертер валют (Model.CurrencyConverter) с XML-файлом, получаемым из API ЦБ РФ (CBRService.CBRClient) (обновление, построение).
/// </summary>
public static class CurrencyConverterBuilder
{
    /// <summary>
    /// Допустимая погрешность равенства чисел с плавающей точкой.
    /// </summary>
    private const decimal _EPS = 1e-6M;

    /// <summary>
    /// Название XML-файла, хранящего в себе курсы валют.
    /// </summary>
    public static string RatesFileName = "rates.xml";

    /// <summary>
    /// Имя корневого XML-элемента в файле {RatesFileName}.
    /// </summary>
    public static string RootXElementName = "ValCurs";

    /// <summary>
    /// Имя аттрибута корневого XML-элемента, содержащего актуальную дату курсов (котировок) валют.
    /// </summary>
    public static string RootActualDateAttributeName = "Date";

    /// <summary>
    /// Имя XML-элемента внутри корневого XML-элемента, представляющего информацию об одной валюте.
    /// </summary>
    public static string CurrencyXElementName = "Valute";

    /// <summary>
    /// Имя аттрибута XML-элемента отдельной валюты, содержащего код валюты.
    /// </summary>
    public static string CharCodeXElementName = "CharCode";

    /// <summary>
    /// Имя аттрибута XML-элемента отдельной валюты, содержащего полное наименование валюты.
    /// </summary>
    public static string NameXElementName = "Name";

    /// <summary>
    /// Имя аттрибута XML-элемента отдельной валюты, содержащего количество единиц валюты, стоящее {AmountCostXElementName} российских рублей.
    /// </summary>
    public static string AmountXElementName = "Nominal";

    /// <summary>
    /// Имя аттрибута XML-элемента отдельной валюты, содержащего стоимость за {AmountXElementName} единиц валюты в российских рублях.
    /// </summary>
    public static string AmountCostXElementName = "Value";

    /// <summary>
    /// Имя аттрибута XML-элемента отдельной валюты, содержащего стоимость за единицу валюты в российских рублях.
    /// </summary>
    public static string UnitCostXElementName = "VunitRate";

    /// <summary>
    /// Создаёт/обновляет XML-файл с курсами валют, актуальными на заданный момент.
    /// </summary>
    /// <param name="actualDate">Актуальная дата обновляемых курсов валют.<br/>По умолчанию - "сегодня", т.е. день, в который был вызван метод.</param>
    /// <exception cref="HttpRequestException">Не удалось успешно получить данные из CBRService.CBRClient.GetRatesXML().</exception>
    public static void UpdateRatesFile(DateTime? actualDate = null)
    {
        var document = CBRService.CBRClient.GetRatesXML(actualDate); // HttpRequestException?

        document.Save(RatesFileName);
        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
            "\nRates have been saved to ",
            RatesFileName,
            ConsoleColor.Blue,
            "!"
        );
    }

    /// <summary>
    /// Создаёт и возвращает объект конвертера валют на основе содержимого XML-файла, хранящего в себе курсы валют.
    /// </summary>
    /// <returns>Объект конвертера валют (Model.CurrencyConverter), наполненный валютами и актуальной датой.</returns>
    /// <exception cref="FileNotFoundException">XML-файл с названием {RatesFileName}, хранящий в себе курсы валют, не существует/не был найден.</exception>
    /// <exception cref="XmlException">Файл с названием {RatesFileName}, хранящий в себе курсы валют, не соответствует структуре XML-файла.</exception>
    /// <exception cref="XmlSchemaException">Схема XML-файла, хранящего в себе курсы валют, неверна/повреждена.</exception>
    /// <exception cref="ArgumentException">Данные, содержащиеся в XML-файле, не соответствуют ограничениям целостности: UnitCost * Amount != AmountCost.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Содержимое XML-файла (информация о валютах) невалидно/повреждено.</exception>
    public static Model.CurrencyConverter LoadConverterFromRatesFile()
    {
        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
            "\nStarting loading rates from ",
            RatesFileName,
            ConsoleColor.Blue,
            "."
        );

        var document = XDocument.Load(RatesFileName); // FileNotFoundException? XmlException?

        var rootElement = document.Root;
        if (rootElement is null || rootElement.Name != RootXElementName)
            throw new XmlSchemaException($"{RatesFileName} file has wrong schema: \"{RootXElementName}\" element not found");

        var actualDateString = rootElement.Attribute("Date")?.Value;
        if (actualDateString is null)
            throw new XmlSchemaException($"\"{RootXElementName}\" element in {RatesFileName} file has wrong schema: \"{RootActualDateAttributeName}\" attribute not found");
        DateTime actualDate;
        var success = DateTime.TryParseExact(actualDateString, Model.CurrencyConverter.ActualDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out actualDate);
        if (!success)
            throw new XmlSchemaException($"\"{RootXElementName}\" element in {RatesFileName} file has wrong schema: \"{RootActualDateAttributeName}\" attribute has invalid value");

        var converter = new Model.CurrencyConverter(actualDate);
        int skippedCurrenciesAmount = 0;
        foreach (var currencyElement in rootElement.Elements())
        {
            try
            {
                var currency = LoadCurrencyFromXElement(currencyElement);
                converter.Add(currency);
            }
            catch (Exception e)
            {
                ConsoleLogger.ConsoleLogger.ExceptionWriteLine(e, ".");
                ++skippedCurrenciesAmount;
            }
        }
        if (skippedCurrenciesAmount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Currencies skipped");
            ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                ": ",
                skippedCurrenciesAmount.ToString(),
                ConsoleColor.Red,
                "."
            );
        }

        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
            "Rates have been loaded ",
            "successfully",
            ConsoleColor.Green,
            "!"
        );
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
        return converter;
    }

    /// <summary>
    /// Создаёт объект валюты на основе содержимого XML-элемента, хранящего в себе необходимую информацию о валюте.
    /// </summary>
    /// <param name="currencyElement">XML-элемент (объект XElement) с необходимой информацией о валюте.</param>
    /// <returns>Объект валюты (Model.Currency), наполненный информацией</returns>
    /// <exception cref="XmlSchemaException">Схема XML-элемента, хранящего в себе необходимую информацию о валюте, неверна/повреждена.</exception>
    /// <exception cref="ArgumentException">Данные, содержащиеся в XML-элементе, не соответствуют ограничениям целостности: UnitCost * Amount != AmountCost.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Содержимое XML-элемента (информация о валюте) невалидно/повреждено.</exception>
    public static Model.Currency LoadCurrencyFromXElement(XElement currencyElement)
    {
        if (currencyElement.Name != CurrencyXElementName)
            throw new XmlSchemaException($"\"{RootXElementName}\" element in {RatesFileName} file has wrong schema: unknown element \"{currencyElement.Name}\"");



        var charCode = currencyElement.Element(CharCodeXElementName)?.Value;
        if (charCode is null)
            throw new XmlSchemaException($"\"{CurrencyXElementName}\" element in {RatesFileName} file has wrong schema: \"{CharCodeXElementName}\" element not found");

        var name = currencyElement.Element(NameXElementName)?.Value;
        if (name is null)
            throw new XmlSchemaException($"\"{CurrencyXElementName}\" element in {RatesFileName} file has wrong schema: \"{NameXElementName}\" element not found");

        var amountString = currencyElement.Element(AmountXElementName)?.Value;
        if (amountString is null)
            throw new XmlSchemaException($"\"{CurrencyXElementName}\" element in {RatesFileName} file has wrong schema: \"{AmountXElementName}\" element not found");

        var amountCostString = currencyElement.Element(AmountCostXElementName)?.Value?.Replace(',', '.');
        if (amountCostString is null)
            throw new XmlSchemaException($"\"{CurrencyXElementName}\" element in {RatesFileName} file has wrong schema: \"{AmountCostXElementName}\" element not found");

        var unitCostString = currencyElement.Element(UnitCostXElementName)?.Value?.Replace(',', '.');
        if (unitCostString is null)
            throw new XmlSchemaException($"\"{CurrencyXElementName}\" element in {RatesFileName} file has wrong schema: \"{UnitCostXElementName}\" element not found");



        int amount;
        bool amountSuccess = int.TryParse(amountString, out amount);
        if (!amountSuccess)
            throw new XmlSchemaException($"\"{CurrencyXElementName}\" element in {RatesFileName} file contains wrong data: \"{AmountXElementName}\" element has invalid value");

        decimal amountCost;
        bool amountCostSuccess = decimal.TryParse(amountCostString, out amountCost);
        if (!amountCostSuccess)
            throw new XmlSchemaException($"\"{CurrencyXElementName}\" element in {RatesFileName} file contains wrong data: \"{AmountCostXElementName}\" element has invalid value");

        decimal unitCost;
        bool unitCostSuccess = decimal.TryParse(unitCostString, out unitCost);
        if (!unitCostSuccess)
            throw new XmlSchemaException($"\"{CurrencyXElementName}\" element in {RatesFileName} file contains wrong data: \"{UnitCostXElementName}\" element has invalid value");



        if (Math.Abs(unitCost * amount - amountCost) > _EPS)
            throw new ArgumentException($"\"{CurrencyXElementName}\" element in {RatesFileName} file contains wrong data: UnitCost ({unitCost:F4}) * Amount ({amount}) != AmountCost ({amountCost:F4})");



        var currency = new Model.Currency(
            charCode,
            name,
            amount,
            amountCost,
            unitCost
        ); // ArgumentOutOfRangeException?
        return currency;
    }
}
