import { environment } from '../../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom, throwError } from 'rxjs';

// Модель формы регистрации пользователя
export class FormRegistration {
  // Имя пользователя
  name: string;
  // Email пользователя
  email: string;
  // Пароль пользователя
  password: string;
  // Подтверждение пароля
  confirmPassword: string;
}

// Модель формы входа в систему
export class FormLogin {
  // Email пользователя
  email: string;
  // Пароль пользователя
  password: string;
}

// Модель формы восстановления пароля
export class FormRestorePassword {
  // Email пользователя для отправки ссылки на восстановление
  email: string = '';
  // Новый пароль пользователя
  newPassword: string = '';
  // Подтверждение нового пароля
  confirmNewPassword: string = '';
}
// Сервис для аутентификации, предоставляющий методы для взаимодействия с API аутентификации
@Injectable({
  providedIn: 'root', // Указывает, что сервис должен быть создан на уровне корня приложения (singleton)
})
export class AuthApiService {
  // Внедрение зависимости HttpClient для выполнения HTTP-запросов
  constructor(private http: HttpClient) {}

  // Метод для регистрации нового пользователя
  async register(formRegistration: FormRegistration): Promise<string> {
    // Выполнение POST-запроса на регистрацию и возвращение полученных токенов
    return (await firstValueFrom(
      this.http.post(
        `${environment.apiAuthUrl}/register`, // URL для регистрации
        formRegistration, // Данные формы регистрации
        {
          responseType: 'text',
        }
      )
    )) as string;
  }

  // Метод для входа пользователя в систему
  async login(formLogin: FormLogin): Promise<string> {
    // Выполнение POST-запроса на вход и возвращение полученных токенов
    return (await firstValueFrom(
      this.http.post(`${environment.apiAuthUrl}/login`, formLogin, {
        responseType: 'text',
      })
    )) as string;
  }

  // Метод для восстановления пароля пользователя
  async restorePassword(
    formRestorePassword: FormRestorePassword
  ): Promise<string> {
    // Выполнение POST-запроса на восстановление пароля и возвращение полученных токенов
    return (await firstValueFrom(
      this.http.post(
        `${environment.apiAuthUrl}/restore-password`, // URL для восстановления пароля
        formRestorePassword, // Данные формы восстановления пароля
        {
          responseType: 'text',
        }
      )
    )) as string;
  }
}
