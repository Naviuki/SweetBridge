namespace SweetBridge.Web.Services.Account;

/// <summary>
/// Параметры поля пароля для общего partial: один компонент на все формы входа и смены пароля.
/// </summary>
/// <param name="Name">Имя поля формы.</param>
/// <param name="Id">Идентификатор поля.</param>
/// <param name="Label">Подпись поля.</param>
/// <param name="Autocomplete">Значение атрибута autocomplete.</param>
/// <param name="Group">Список идентификаторов полей, которыми управляет общая кнопка «Показать».</param>
/// <param name="Hint">Подсказка под полем.</param>
public sealed record PasswordFieldViewModel(
    string Name,
    string Id,
    string Label,
    string Autocomplete,
    string Group,
    string? Hint = null);