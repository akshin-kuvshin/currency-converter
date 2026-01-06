// author: Danila "akshin_" Axyonov

namespace CurrencyConverter.Model;

/// <summary>
/// Класс валюты.
/// </summary>
public class Currency : ICloneable
{
    /// <summary>
    /// Код (сокращённое наименование) валюты.
    /// </summary>
    /// <remarks>Состоит ровно из 3-ёх заглавных латинских букв.</remarks>
    private string _charCode = string.Empty;

    /// <summary>
    /// Полное наименование валюты.
    /// </summary>
    private string _name = string.Empty;

    /// <summary>
    /// Количество единиц валюты, стоящее {_amountCost} российских рублей.
    /// </summary>
    /// <remarks>
    /// Используется для удобного представления курса между валютами.
    /// <br/><br/>Должно являться натуральным числом.
    /// </remarks>
    private int _amount;

    /// <summary>
    /// Стоимость за {_amount} единиц валюты в российских рублях.
    /// </summary>
    /// <remarks>
    /// Используется для удобного представления курса между валютами.
    /// <br/><br/>Должно являться положительным числом.
    /// </remarks>
    private decimal _amountCost;

    /// <summary>
    /// Стоимость за единицу валюты в российских рублях.
    /// </summary>
    /// <remarks>
    /// Должно являться положительным числом.
    /// </remarks>
    private decimal _unitCost;

    /// <summary>
    /// Проверяет строку на то, является ли она валидным кодом валюты.
    /// </summary>
    /// <remarks>Строка является валидным кодом валюты т. и т. т., когда она состоит ровно из 3-ёх заглавных латинских букв.</remarks>
    /// <param name="charCode">Проверяемая строка</param>
    /// <returns>true, если может; false, если не может.</returns>
    public static bool IsAvailableCharCode(string charCode) =>
        charCode.Length == 3 && charCode.All(c => char.IsAsciiLetterUpper(c));

    /// <summary>
    /// Свойство кода (сокращённого наименования) валюты.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Неверный формат кода валюты: должен состоять ровно из 3-ёх заглавных латинских букв.</exception>
    public string CharCode
    {
        get => _charCode;
        set
        {
            if (!IsAvailableCharCode(value))
                throw new ArgumentOutOfRangeException($"CharCode must consist of 3 uppercase Latin letters. Given CharCode: {value}");
            
            _charCode = value;
        }
    }

    /// <summary>
    /// Свойство полного наименования валюты.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Свойство количества единиц валюты, стоящего {AmountCost} российских рублей.
    /// </summary>
    /// <remarks>
    /// Используется для удобного представления курса между валютами.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Недопустимое значение поля {Amount}: должно являться натуральным числом.</exception>
    public int Amount
    {
        get => _amount;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException($"Amount must be a natural (positive integer) number. Given Amount: {value}");
            
            _amount = value;
        }
    }

    /// <summary>
    /// Свойство стоимости за {_amount} единиц валюты в российских рублях.
    /// </summary>
    /// <remarks>
    /// Используется для удобного представления курса между валютами.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Недопустимое значение поля {AmountCost}: должно являться положительным числом.</exception>
    public decimal AmountCost
    {
        get => _amountCost;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException($"AmountCost must be a positive number. Given AmountCost: {value}");
            
            _amountCost = value;
        }
    }

    /// <summary>
    /// Свойство стоимости за единицу валюты в российских рублях.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Недопустимое значение поля {UnitCost}: должно являться положительным числом.</exception>
    public decimal UnitCost
    {
        get => _unitCost;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException($"UnitCost must be a positive number. Given UnitCost: {value}");
            
            _unitCost = value;
        }
    }

    /// <summary>
    /// Инициализирует все поля объекта валюты соотвествующими значениями из аргументов.
    /// </summary>
    /// <param name="initCharCode">Начальный код валюты.</param>
    /// <param name="initName">Начальное полное наименование валюты.</param>
    /// <param name="initAmount">Начальное количество единиц валюты, стоящее {initAmountCost} российских рублей.</param>
    /// <param name="initAmountCost">Начальная стоимость за {initAmount} единиц валюты в российских рублях.</param>
    /// <param name="initUnitCost">Начальная стоимость за единицу валюты в российских рублях.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public Currency(string initCharCode, string initName, int initAmount, decimal initAmountCost, decimal initUnitCost)
    {
        CharCode = initCharCode;
        Name = initName;
        Amount = initAmount;
        AmountCost = initAmountCost;
        UnitCost = initUnitCost;
    }

    /// <summary>
    /// Создаёт поверхностную копию текущего объекта валюты.
    /// </summary>
    /// <returns>Поверхностная копия текущего объекта валюты в виде объекта типа object.</returns>
    public object Clone() => MemberwiseClone();

    /// <summary>
    /// Формирует и возвращает строку-представление курса текущей валюты к российскому рублю.
    /// </summary>
    /// <returns>Сформированная строка-представление в формате "100 Тенге = 15.4978 Российских рублей".</returns>
    public string GetRateString() => $"{_amount} {_charCode} ({_name}) = {_amountCost:F4} RUB (Российских рублей)";

    /// <summary>
    /// Вычисляет курс текущей валюты к валюте {other} (расчёт производится через рубль).
    /// </summary>
    /// <param name="other">Валюта котировки.</param>
    /// <returns>Количество единиц валюты {other}, равное одной единице текущей валюты.</returns>
    public decimal GetRate(Currency other) => _unitCost / other._unitCost;
}
