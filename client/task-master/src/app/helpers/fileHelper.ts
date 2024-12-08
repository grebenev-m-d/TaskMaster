import { FileData } from '../models/FileData';

// Функция для загрузки файла
export function downloadFile(file: FileData) {
  // Создание URL для файла
  let url = window.URL.createObjectURL(file.data);
  // Создание элемента <a> для загрузки файла
  let a = document.createElement('a');
  // Добавление элемента <a> в тело документа
  document.body.appendChild(a);
  // Установка стиля элемента <a> для скрытия его из виду
  a.setAttribute('style', 'display: none');
  // Установка URL для загрузки файла
  a.href = url;
  // Установка имени файла для загрузки
  a.download = file.fileName;
  // Имитация клика по элементу <a> для начала загрузки файла
  a.click();
  // Освобождение URL для предотвращения утечек памяти
  window.URL.revokeObjectURL(url);
  // Удаление элемента <a> из документа
  a.remove();
}
