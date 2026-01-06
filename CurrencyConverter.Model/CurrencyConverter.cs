// author: Danila "akshin_" Axyonov

using System.Collections;

namespace CurrencyConverter.Model;

/// <summary>
/// Класс конвертера валют.
/// </summary>
public class CurrencyConverter : IEnumerable<Currency>
{
    /// <summary>
    /// Словарь валют.
    /// </summary>
    /// <remarks>
    /// Хранит пары {CharCode, Currency}.
    /// </remarks>
    private Dictionary<string, Currency> _currencies = new Dictionary<string, Currency>();

    /// <summary>
    /// Readonly-свойство длины словаря валют (т.е. количества валют, имеющихся в нём).
    /// </summary>
    public int Length
    {
        get => _currencies.Count;
    }

    /// <summary>
    /// Дата (и её свойство), относительно которой является актуальным имеющийся словарь валют.
    /// </summary>
    public DateTime ActualDate;

    /// <summary>
    /// Основной формат, в котором задана актуальная дата курсов (котировок) валют.
    /// </summary>
    public static string ActualDateFormat = "dd.MM.yyyy";

    /// <summary>
    /// Инициализирует словарь валют российским рублём и выставляет актуальную дату.
    /// </summary>
    /// <param name="initActualDate">Начальная актуальная дата.</param>
    public CurrencyConverter(DateTime? initActualDate = null)
    {
        var russianRuble = new Currency(
            "RUB",
            "Российский рубль",
            1,
            1,
            1
        );
        _currencies["RUB"] = russianRuble;

        ActualDate = initActualDate ?? DateTime.Today;
    }

    /// <summary>
    /// Итерируется по всем объектам валюты, содержащимся в словаре валют.
    /// </summary>
    /// <remarks>Возвращает копии объектов валют =&gt; Любые изменения в них не повлияют на оригиналы =&gt; Эффект readonly.</remarks>
    /// <returns>Объект IEnumerator&lt;Currency&gt; для явно типизированного перечисления.</returns>
    public IEnumerator<Currency> GetEnumerator()
    {
        foreach (var currency in _currencies.Values)
            yield return (Currency)currency.Clone();
    }

    /// <summary>
    /// Итерируется по всем объектам валюты, содержащимся в словаре валют.
    /// </summary>
    /// <remarks>Возвращает копии объектов валют =&gt; Любые изменения в них не повлияют на оригиналы =&gt; Эффект readonly.
    /// <br/><br/>Идентичен методу для явно типизированного перечисления.</remarks>
    /// <returns>Объект IEnumerator для неявно типизированного перечисления.</returns> (наверное)
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Добавляет объект валюты в словарь валют, позволяя в дальнейшем осуществлять конвертации в/из неё.
    /// </summary>
    /// <param name="currency">Добавляемый объект валюты.</param>
    public void Add(Currency currency) => _currencies[currency.CharCode] = currency;

    /// <summary>
    /// Проверяет наличие валюты с заданным кодом в словаре валют.
    /// <br/>В случае отсутствия таковой - выбрасывает KeyNotFoundException.
    /// </summary>
    /// <param name="charCode">Код проверяемой валюты.</param>
    /// <exception cref="KeyNotFoundException">Валюта с заданным кодом отсутствует в словаре валют.</exception>
    private void CheckCharCode(string charCode)
    {
        if (!_currencies.ContainsKey(charCode))
            throw new KeyNotFoundException($"Currency with CharCode=\"{charCode}\" doesn't exist in the currencies dictionary");
    }

    /// <summary>
    /// Вычисляет курс валюты {from} к валюте {to} (расчёт производится через рубль).
    /// </summary>
    /// <param name="charCodeFrom">Код валюты {from} (базовой валюты).</param>
    /// <param name="charCodeTo">Код валюты {to} (валюты котировки).</param>
    /// <returns>Количество единиц валюты {to}, равное одной единице валюты {from}.</returns>
    /// <exception cref="KeyNotFoundException">Один (или оба) из кодов конвертируемых валют отсутствует(-ют) в словаре валют.</exception>
    public decimal GetRate(string charCodeFrom, string charCodeTo)
    {
        CheckCharCode(charCodeFrom);
        CheckCharCode(charCodeTo);

        var currencyFrom = _currencies[charCodeFrom];
        var currencyTo = _currencies[charCodeTo];
        return currencyFrom.GetRate(currencyTo);
    }

    /// <summary>
    /// Переводит (осуществляет конвертацию) {amountFrom} единиц валюты {from} в валюту {to} (расчёт производится через рубль).
    /// </summary>
    /// <param name="amountFrom">Количество единиц валюты {from} (базовой валюты).</param>
    /// <param name="charCodeFrom">Код валюты {from} (базовой валюты).</param>
    /// <param name="charCodeTo">Код валюты {to} (валюты котировки).</param>
    /// <returns>Количество единиц валюты {to}, равное {amountFrom} единицам валюты {from}.</returns>
    /// <exception cref="KeyNotFoundException">Один (или оба) из кодов конвертируемых валют отсутствует(-ют) в словаре валют.</exception>
    public decimal Convert(decimal amountFrom, string charCodeFrom, string charCodeTo) =>
        amountFrom * GetRate(charCodeFrom, charCodeTo);
}
