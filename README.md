**en [ru](README.md.ru)**

###### author: Danila "akshin_" Axyonov

# CurrencyConverter



### Contents:

1. [Description](#description);
2. [Usage](#usage);
3. [Available commands](#available-commands).



<hr/>



### Description:

**CurrencyConverter** is a **console utility** written in C# <small>_(.NET 10, but none of the latest features are being used, so an older version will do)_</small>, which can **convert currencies** and takes their exchange rates from the **API of the CBR** (Central Bank of Russia).

**Solution consists of 5 projects**:
1. **`ConsoleLogger`** - provides _convenient output to the console_ in certain cases;
2. **`Model`** - implements the _semantic logic of the program_, i.e. classes (entities) `Currency` and `CurrencyConverter`;
3. **`CBRSercive`** - responsible for _accessing the API of the CBR_ и _processing the response_ (_request result_);
4. **`Builder`** - builds an instance of `CurrencyConverter` based on the data from an XML-file received from **`CBRService`**;
5. **`View`** - _contacts directly with the user_, providing [certain functionality for interacting with the program](#available-commands).



<hr/>



### Usage:

1. Build the **`CurrencyConverter.View`** project <small>_(due to existing dependencies, the remaining 4 projects will also be built)_</small>;
2. Run the built project **without additional CLI-arguments**;

_Remark_: at startup, the program will try to update the XML-file with the currency rates with the most actual data and, regardless of the degree of success of the previous step, upload all the currencies available in the XML-file to the converter.

3. Interact with the program using **[available commands](#available-commands)**.



<hr/>



### Available commands:

_Remark_: commands can be typed **in any case**.

**<u>1</u>.** **`exit`**, **`q`**, **`quit`** - terminates the program.

**<u>2</u>.** **`h`**, **`help`** - displays help text (a list of available commands).

**<u>3</u>.** **`l`**, **`list`** - displays a list of available currencies.

**<u>4</u>.** **`update [_actual_date_]`** - updates an XML-file with currency rates actual for the latest day, not exceeding `_actual_date_`;

_**Remark 1**_: the required format for `_actual_date_` is `dd.MM.yyyy`;

_**Remark 2**_: `_actual_date_` is an **optional parameter**; its default value is "today" (means the day when the command was called);

**Example**: `UpDaTe 01.02.2003`.

**<u>5</u>.** **`load`** - loads all currencies from an XML-file to a converter and, if succeed, deletes the old ones.

**<u>6</u>.** **`reload [_actual_date_]`** - equals to `update [_actual_date_]` and `load` commands sequential calling;
_**Remarks 1 и 2**_: see remarks 1 & 2 for `update [_actual_date_]` command.

**<u>7</u>.** **`_amount_ _char_code_from_ {to / -> / =>} _char_code_to_`** - converts `_amount_` units of currency with char code equals to `_char_code_from_` to currency with char code equals to `_char_code_to_`;

_**Remark 1**_: `_amount_` must be a valid real number; the _decimal separator can be either a dot or a comma_;

_**Remark 2**_: `_char_code_from_` and `_char_code_to_` must consist of 3 Latin letters;

**Example 1**: `19999.99 rub -> kzt`;

**Example 2**: `0,25 USd To JPy`.

**<u>8</u>.** **`_char_code_from_ {to / -> / =>} _char_code_to_`** - calculates the F / T rate where F is the currency with char code equals to `_char_code_from_` and T is the currency with char code equals to `_char_code_to_`;

_**Remark 1**_: this command is equal to calling `1 _char_code_from_ {to / -> / =>} _char_code_to_`;

_**Remark 2**_: see remark 2 for the previous command;

**Example 1**: `BYB TO RUB`;

**Example 2**: `rSd => tRy`.
