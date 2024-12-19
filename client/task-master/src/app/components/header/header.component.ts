import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  getEmailFromJwt,
  getRoleFromJwt,
  getUserNameFromJwt,
} from '../../helpers/jwtServiceHelper';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent {
  // Свойство для отображения/скрытия меню
  showMenu: boolean = false;

  // Инициализация компонента с передачей зависимости для маршрутизатора
  constructor(private router: Router) {}

  // Метод для выхода пользователя из системы
  exit() {
    // Удаление токенов из локального хранилища
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');

    // Перенаправление на страницу входа
    this.router.navigate(['/login']);
  }

  // Переменные для отображения информации о пользователе
  name: string;
  role: string;
  email: string;

  // Асинхронная инициализация компонента
  async ngOnInit() {
    // Получение токена доступа из локального хранилища
    let token = localStorage.getItem('accessToken');
    // Получение имени пользователя из JWT-токена
    this.name = getUserNameFromJwt(token);
    // Получение роли пользователя из JWT-токена
    this.role = getRoleFromJwt(token);
    // Получение адреса электронной почты пользователя из JWT-токена
    this.email = getEmailFromJwt(token);
  }
}
