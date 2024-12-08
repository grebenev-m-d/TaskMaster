import { Component } from '@angular/core';

import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

import {
  AuthApiService,
  FormRestorePassword,
  FormRegistration,
} from '../../../services/api.services/authApi.service';
import { LoaderComponent } from '../../../components/loader/loader.component';
import { showError } from '../../../helpers/exceptionMessageHelper';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule, LoaderComponent],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
})
export class ForgotPasswordComponent {
  // Флаг отображения загрузчика
  hasShowLoader: boolean = false;
  // Флаг отображения сообщения о восстановлении пароля
  restorePasswordMessageShow: boolean = false;
  // Сообщение о восстановлении пароля
  restorePasswordMessage: string = '';

  // Конструктор компонента
  constructor(
    private router: Router,
    public dialog: MatDialog,
    private authService: AuthApiService
  ) {}

  // Форма восстановления пароля
  form: FormRestorePassword = new FormRestorePassword();

  // Минимальная длина пароля
  minPasswordLength: number = 8;

  // Сообщения об ошибках
  errorMessageName: string = 'Имя обязательно.';
  errorMessageEmail: string =
    'Введите действительный адрес электронной почты (например, example@domain.com).';
  errorMessagePasswordLength: string = `Пароль должен состоять не менее чем из ${this.minPasswordLength} символов. Пример: P@ssw0rd123!`;
  errorMessagePasswordMatch: string = 'Пароли не совпадают.';
  emailRegex = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;

  // Асинхронный метод восстановления пароля
  async restorePassword() {
    try {
      this.notify(
        'Для подтверждения вашего адреса электронной почты, мы направили письмо на указанный адрес.'
      );

      this.hasShowLoader = true;
      this.authService
        .restorePassword(this.form)
        .then((accessToken) => {
          this.hasShowLoader = false;
          localStorage.setItem('accessToken', accessToken);
          this.router.navigate(['login']);
        })
        .catch((e) => {
          this.hasShowLoader = false;
          showError(this.dialog, e.error);
        });
    } catch (error) {
      console.log(error);
    }
  }

  // Метод для отображения уведомления
  notify(message: string) {
    this.restorePasswordMessageShow = true;
    this.restorePasswordMessage = message;
    setTimeout(() => {
      this.restorePasswordMessage = '';
      this.restorePasswordMessageShow = false;
    }, 15000);
  }
}
