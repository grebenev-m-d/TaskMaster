import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ErrorModalComponent } from '../modal/error-modal/error-modal.component';

// Функция для извлечения сообщения об ошибке из строки ошибки
export function extractExceptionMessage(
  errorString: string, // Строка ошибки, из которой извлекается сообщение
  exceptionType: string = 'HubException' // Тип исключения, по умолчанию "HubException"
): string | null {
  // Создание регулярного выражения для поиска сообщения об ошибке с заданным типом исключения
  const hubExceptionRegex = new RegExp(`${exceptionType}: (.*)`);
  // Поиск соответствия регулярному выражению в строке ошибки
  const match = hubExceptionRegex.exec(errorString);

  // Если найдено соответствие
  if (match) {
    return match[1]; // Извлечение сообщения об ошибке
  } else {
    // Если не найдено соответствие или строка ошибки не является строкой
    // Возвращается либо исходная строка, либо сообщение об ошибке по умолчанию
    return typeof errorString == 'string' ? errorString : 'Неизвестная ошибка.';
  }
}

function is401Error(errorMessage: string): boolean {
  const error401Regex = /Status code '(\d+)'/;
  const match = errorMessage.match(error401Regex);

  if (match && match[1] === '401') {
    return true;
  } else {
    return false;
  }
}

// Функция для отображения модального окна с сообщением об ошибке
export function showError(dialog: MatDialog, errorString: string) {
  // Создание конфигурации для модального окна
  const config = new MatDialogConfig();
  config.panelClass = 'custom-dialog-cont'; // Установка пользовательского класса для стилизации модального окна

  // Передача сообщения об ошибке в данные модального окна
  config.data = {
    message: extractExceptionMessage(errorString), // Извлеченное сообщение об ошибке
  };

  // Открытие модального окна с компонентом для отображения сообщения об ошибке
  dialog.open(ErrorModalComponent, config);
}
