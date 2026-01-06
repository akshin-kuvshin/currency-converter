// author: Danila "akshin_" Axyonov

using System.Net;
using System.Text;
using System.Xml.Linq;

namespace CurrencyConverter.CBRService;

/// <summary>
/// Статический класс-клиент, реализующий получение данных о курсах (котировках) иностранных валют к российскому рублю из API ЦБ РФ.
/// </summary>
public static class CBRClient
{
    /// <summary>
    /// URL-адрес основного источника данных (XML-документ).
    /// </summary>
    private const string _RATES_URL = "https://www.cbr.ru/scripts/XML_daily.asp";

    /// <summary>
    /// Максимальное количество попыток выполнения HTTP-запроса, пока response.IsSuccessStatusCode == false.
    /// </summary>
    private const int _N_ATTEMPTS = 5;

    /// <summary>
    /// Единый (т.к. статический) клиент для отправки HTTP-запросов и получения ответов.
    /// </summary>
    private static HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Имя параметра, в котором в запросе по адресу {_RATES_URL} передаётся актуальная дата.
    /// </summary>
    private const string _REQUEST_ACTUAL_DATE_PARAMETER_NAME = "date_req";

    /// <summary>
    /// Формат, в котором передаётся актуальная дата в качестве параметра запроса по адресу {_RATES_URL}.
    /// </summary>
    private const string _REQUEST_ACTUAL_DATE_FORMAT = "dd/MM/yyyy";

    /// <summary>
    /// Получает курсы (котировки) иностранных валют к российскому рублю, актуальные на заданный момент, в формате XML.
    /// </summary>
    /// <param name="actualDate">Актуальная дата запрашиваемых курсов валют.<br/>По умолчанию - "сегодня", т.е. день, в который был вызван метод.</param>
    /// <returns>Результат в виде объекта XDocument.</returns>
    /// <exception cref="HttpRequestException">Не удалось успешно получить данные в результате 5-и запросов.</exception>
    public static XDocument GetRatesXML(DateTime? actualDate = null)
    {
        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
            "\nStarting requesting rates from ",
            _RATES_URL,
            ConsoleColor.Blue
        );

        var requestURL = _RATES_URL + "?" + _REQUEST_ACTUAL_DATE_PARAMETER_NAME + "=" + (actualDate ?? DateTime.Today).ToString(_REQUEST_ACTUAL_DATE_FORMAT);

        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        for (int attempt = 1; attempt <= _N_ATTEMPTS && !response.IsSuccessStatusCode; ++attempt)
        {
            Console.Write($"Attempt {attempt}... | ");

            try
            {
                response = _httpClient.GetAsync(requestURL).Result;
            }
            catch (Exception e)
            {
                ConsoleLogger.ConsoleLogger.ExceptionWrite(e, " | ");
            }

            if (response.IsSuccessStatusCode)
                ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                    "",
                    "Success",
                    ConsoleColor.Green,
                    "!"
                );
            else
                ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
                    "",
                    "Failed",
                    ConsoleColor.Red,
                    "."
                );
        }
        response.EnsureSuccessStatusCode(); // HttpRequestException?

        var xmlString = response.Content.ReadAsStringAsync().Result;
        var document = XDocument.Parse(xmlString);

        ConsoleLogger.ConsoleLogger.MiddleColoredWriteLine(
            "Rates have been retrieved ",
            "successfully",
            ConsoleColor.Green,
            "!"
        );
        return document;
    }

    /// <summary>
    /// Делает доступными кодировки, имеющиеся только в десктопной версии .NET Framework, в т.ч. Windows-1251.
    /// </summary>
    public static void AddDesktopNETFrameworkEncodings() =>
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}
