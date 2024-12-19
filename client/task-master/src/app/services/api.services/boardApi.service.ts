import { Injectable } from '@angular/core';
import { HttpClient, HttpEventType, HttpHeaders } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { FileData } from '../../models/FileData';

@Injectable({
  providedIn: 'root',
})
export class BoardApiService {
  // Базовый URL для API-запросов к доске
  private baseUrl: string = environment.apiUrl + '/board';

  constructor(private http: HttpClient) {}

  // Добавление изображения к доске
  async addImage(boardId: string, file: File): Promise<any> {
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

      try {
        // Отправка POST-запроса для загрузки изображения
        const response = await this.http
          .post(`${this.baseUrl}/add-image?boardId=${boardId}`, formData, {
            headers,
            reportProgress: true,
            observe: 'events',
          })
          .toPromise();

        return response; // Возвращение ответа от сервера
      } catch (error) {
        console.error('Error uploading file:', error);
        throw error; // Бросает ошибку для обработки во внешнем коде
      }
    }
  }

  // Получение изображения доски по идентификатору
  async getImage(boardId: string): Promise<FileData> {
    try {
      // Получение токена доступа из локального хранилища
      const token = localStorage.getItem('accessToken');
      // Формирование URL для запроса изображения
      const url = `${this.baseUrl}/get-image?boardId=${boardId}`;
      // Установка заголовков с токеном доступа
      const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

      // Выполнение GET-запроса для получения изображения
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
    } catch (error) {
      return null; // Возврат значения null в случае возникновения ошибки
    }
  }
}
