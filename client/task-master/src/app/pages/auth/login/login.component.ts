import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  AuthApiService,
  FormLogin,
} from '../../../services/api.services/authApi.service';
import { CommonModule } from '@angular/common';
import { LoaderComponent } from '../../../components/loader/loader.component';
import { showError } from '../../../helpers/exceptionMessageHelper';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    RouterLink,
    FormsModule,
    CommonModule,
    LoginComponent,
    LoaderComponent,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  // Конструктор компонента
  constructor(
    private router: Router,
    public dialog: MatDialog,
    private authService: AuthApiService
  ) {}

  // Форма входа
  form: FormLogin = {
    email: '',
    password: '',
  };

  // Флаг отображения загрузчика
  hasShowLoader: boolean = false;
  // Сообщение об ошибке регистрации
  registrationErrorMessage: string;
  // Флаг отображения ошибки регистрации
  registrationErrorShow: boolean = false;

  // Регулярное выражение для проверки электронной почты
  emailRegex = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;

  // Сообщение об ошибке для электронной почты
  errorMessageEmail: string =
    'Введите действительный адрес электронной почты (например, example@domain.com).';

  // Асинхронный метод входа в систему
  async login() {
    // Выполнение проверок валидации
    if (!this.emailRegex.test(this.form.email)) {
      showError(this.dialog, this.errorMessageEmail);
      return; // Прерывание выполнения в случае неудачной валидации
    }

    try {
      this.hasShowLoader = true;
      await this.authService
        .login(this.form)
        .then((token) => {
          this.hasShowLoader = false;
          localStorage.setItem('accessToken', token);
          this.router.navigate(['']);
        })
        .catch((e) => {
          this.hasShowLoader = false;
          showError(this.dialog, e.error);
        });
    } catch (error) {
      console.log(error);
      this.registrationErrorMessage = error;
      this.registrationErrorShow = true;

      setTimeout(() => {
        this.registrationErrorMessage = '';
        this.registrationErrorShow = false;
      }, 1000);
    }
  }
}
