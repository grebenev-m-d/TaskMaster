import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment.development';
import { HttpClient, HttpEventType, HttpHeaders } from '@angular/common/http';
import { FileData } from '../../models/FileData';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CardApiService {
  // Базовый URL для API-запросов к карточке
  private baseUrl: string = environment.apiUrl + '/card';

  constructor(private http: HttpClient) {}

  // Добавление файла вложения к карточке
  async addAttachmentFile(cardId: string, file: File): Promise<void> {
    // Проверка наличия файла
    if (file) {
      // Создание объекта FormData для отправки файла
      const formData = new FormData();
      // Получение токена доступа из локального хранилища
      const token = localStorage.getItem('accessToken');
      // Установка заголовков с токеном доступа
      const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

      // Добавление файла к FormData
      formData.append('file', file);

      // Отправка POST-запроса для загрузки файла вложения
      this.http
        .post(
          `${this.baseUrl}/add-attachment-file?cardId=${cardId}`,
          formData,
          {
            headers,
            reportProgress: true,
            observe: 'events',
          }
        )
        .subscribe((event) => {
          // Обработка событий загрузки
          if (event.type === HttpEventType.UploadProgress) {
            // Обработка прогресса загрузки
          } else if (event.type === HttpEventType.Response) {
            // Обработка завершения загрузки
          }
        });
    }
  }

  // Получение файла вложения к карточке по идентификаторам
  async getAttachmentFile(
    cardId: string,
    attachmentId: string
  ): Promise<FileData> {
    // Получение токена доступа из локального хранилища
    const token = localStorage.getItem('accessToken');
    // Формирование URL для запроса файла вложения
    const url = `${this.baseUrl}/get-attachment-file?cardId=${cardId}&attachmentId=${attachmentId}`;
    // Установка заголовков с токеном доступа
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    // Выполнение GET-запроса для получения файла вложения
    let response = await firstValueFrom(
      this.http.get(url, {
        headers,
        responseType: 'blob', // Установка типа ответа как blob
        observe: 'response', // Включение наблюдения за ответом
      })
    );

    // Получение значения заголовка Content-Disposition
    let contentDisposition = response.headers.get('Content-Disposition');
    // Проверка наличия заголовка Content-Disposition
    if (!contentDisposition) {
      return null; // Возврат значения null, если заголовок отсутствует
    }
    // Декодирование имени файла из заголовка Content-Disposition
    let encodedName = contentDisposition.match(/''(.+)/)[1];
    let fileName = decodeURIComponent(encodedName.replace(/\s/g, '+'));

    // Возвращение объекта FileData с именем и данными файла
    return new FileData(fileName, response.body);
  }

  // Добавление обложки карточки
  async addCoverImage(cardId: string, file: File): Promise<void> {
    // Проверка наличия файла
    if (file) {
      // Создание объекта FormData для отправки файла
      const formData = new FormData();
      // Получение токена доступа из локального хранилища
      const token = localStorage.getItem('accessToken');
      // Установка заголовков с токеном доступа
      const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

      // Добавление файла к FormData
      formData.append('file', file);

      // Отправка POST-запроса для загрузки обложки карточки
      this.http
        .post(`${this.baseUrl}/add-cover-image?cardId=${cardId}`, formData, {
          headers,
          reportProgress: true,
          observe: 'events',
        })
        .subscribe((event) => {});
    }
  }

  // Асинхронная функция для получения обложки карточки по идентификатору карточки
  async getCoverImage(cardId: string): Promise<FileData> {
    // Получение токена доступа из локального хранилища
    const token = localStorage.getItem('accessToken');
    // Формирование URL для запроса обложки карточки с использованием переданного идентификатора карточки
    const url = `${this.baseUrl}/get-cover-image?cardId=${cardId}`;
    // Создание объекта заголовков с токеном доступа для аутентификации при запросе
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    // Выполнение GET-запроса для получения данных обложки карточки
    let response = await firstValueFrom(
      this.http.get(url, {
        headers, // Передача заголовков для аутентификации
        responseType: 'blob', // Установка типа ответа как blob для получения файла
        observe: 'response', // Установка режима наблюдения за ответом
      })
    );

    // Получение значения заголовка Content-Disposition из ответа
    let contentDisposition = response.headers.get('Content-Disposition');
    // Проверка наличия заголовка Content-Disposition
    if (!contentDisposition) {
      return null; // Возврат значения null, если заголовок отсутствует
    }
    // Извлечение закодированного имени файла из заголовка Content-Disposition
    let encodedName = contentDisposition.match(/''(.+)/)[1];
    // Декодирование имени файла из его закодированного формата и замена пробелов
    let fileName = decodeURIComponent(encodedName.replace(/\s/g, '+'));

    // Возврат нового экземпляра объекта FileData с именем файла и данными файла
    return new FileData(fileName, response.body);
  }
}
