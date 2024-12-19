import { Component } from '@angular/core';
import {
  AuthApiService,
  FormRegistration,
} from '../../../services/api.services/authApi.service';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LoaderComponent } from '../../../components/loader/loader.component';
import { MatDialog } from '@angular/material/dialog';
import { showError } from '../../../helpers/exceptionMessageHelper';

@Component({
  selector: 'app-registration',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule, LoaderComponent],
  templateUrl: './registration.component.html',
  styleUrl: './registration.component.css',
})
export class RegistrationComponent {
  // Флаг отображения загрузчика
  hasShowLoader: boolean = false;
  // Сообщение о регистрации
  registrationMessage: string = '';
  // Флаг отображения сообщения о регистрации
  registrationMessageShow: boolean = false;

  // Конструктор компонента
  constructor(
    public dialog: MatDialog,
    private router: Router,
    private authService: AuthApiService
  ) {}

  // Форма регистрации
  form: FormRegistration = {
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
  };

  // Минимальная длина пароля
  minPasswordLength: number = 4;

  // Сообщения об ошибках
  errorMessageName: string = 'Имя обязательно.';
  errorMessageEmail: string =
    'Введите действительный адрес электронной почты (например, example@domain.com).';
  errorMessagePasswordLength: string = `Пароль должен состоять не менее чем из ${this.minPasswordLength} символов. Пример: P@ssw0rd123!`;
  errorMessagePasswordMatch: string = 'Пароли не совпадают.';

  // Регулярное выражение для проверки электронной почты
  emailRegex = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;

  // Асинхронный метод регистрации
  async registration() {
    try {
      // Отображение уведомления
      this.notify(
        'Для подтверждения вашего адреса электронной почты, мы направили письмо на указанный адрес.'
      );

      this.hasShowLoader = true;
      await this.authService
        .register(this.form)
        .then((accessToken) => {
          // Сохранение токенов в локальное хранилище
          localStorage.setItem('accessToken', accessToken);
          // Переход на главную страницу
          this.router.navigate(['']);
          this.hasShowLoader = false;
          console.log('Регистрация успешна!');
        })
        .catch((e) => {
          // Отображение ошибки
          showError(this.dialog, e.error);
        });
    } catch (error) {
      console.log(error);
    }
    this.hasShowLoader = false;
    this.registrationMessageShow = false;
  }

  // Метод для отображения уведомления
  notify(message: string) {
    this.registrationMessageShow = true;
    this.registrationMessage = message;
    setTimeout(() => {
      this.registrationMessage = '';
      this.registrationMessageShow = false;
    }, 15000);
  }
}
