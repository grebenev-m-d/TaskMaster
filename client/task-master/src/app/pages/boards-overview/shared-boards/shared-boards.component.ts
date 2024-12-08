import { Component, ElementRef, ViewChild } from '@angular/core';
import { DesignType } from '../../../models/enum/DesignType';
import { EntityState } from '../../../models/EntityState';
import { Board } from '../../../models/Board';
import { BoardHubService } from '../../../services/hub.services/boardHub.service';
import { BoardApiService } from '../../../services/api.services/boardApi.service';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { BoardComponent } from '../../../components/board/board.component';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { User } from '../../../models/User';
import { BoardAccessLevelHubService } from '../../../services/hub.services/boardAccessLevelHub.service';
import { UserSharedBoardsComponent } from './user-shared-boards/user-shared-boards.component';
import { LoaderComponent } from '../../../components/loader/loader.component';

@Component({
  selector: 'app-shared-boards',
  standalone: true,
  imports: [
    BoardComponent,
    FormsModule,
    CommonModule,
    UserSharedBoardsComponent,
    LoaderComponent,
  ],
  templateUrl: './shared-boards.component.html',
  styleUrl: './shared-boards.component.css',
})
export class SharedBoardsComponent {
  // Список пользователей с общими досками
  usersWithSharedBoards: User[] = [];

  // Сервис для взаимодействия с досками
  boardHubService: BoardHubService;

  // Сервис для уровней доступа к доскам
  boardAccessLevelHubService: BoardAccessLevelHubService;

  // Конструктор компонента
  constructor(public dialog: MatDialog) {}

  // Инициализация компонента
  async ngOnInit() {
    // Инициализация сервиса для взаимодействия с досками
    this.boardHubService = new BoardHubService();

    // Начало соединения с сервером SignalR для досок
    await this.boardHubService
      .startConnection()
      .then(async () => {
        this.initializeBoardState();
      })
      .catch((err) => console.error('Error connecting to SignalR:', err));

    // Инициализация сервиса для уровней доступа к доскам
    this.boardAccessLevelHubService = new BoardAccessLevelHubService();

    // Начало соединения с сервером SignalR для уровней доступа к доскам
    await this.boardAccessLevelHubService
      .startConnection()
      .then(async () => {
        this.registerBoardAccessLevelEventHandlers();
      })
      .catch((err) => console.error('Error connecting to SignalR:', err));
  }

  // Инициализация состояния досок
  async initializeBoardState() {
    // Получение пользователей с общими досками
    this.usersWithSharedBoards =
      await this.boardHubService.getUsersWithSharedBoards();
    console.log(this.usersWithSharedBoards);
  }

  // Регистрация обработчиков событий для уровней доступа к доскам
  async registerBoardAccessLevelEventHandlers() {
    // Добавление уровня доступа для пользователя
    this.boardAccessLevelHubService.onUserAccessLevelAdded(
      (boardId, user, permission) => {
        // Поиск пользователя с общими досками
        let userWithSharedBoards = this.usersWithSharedBoards.find(
          (i) => i.id === user.id
        );

        // Если пользователь не найден, добавить его
        if (userWithSharedBoards == null) {
          this.usersWithSharedBoards.push(user);
        }
      }
    );

    // Удаление уровня доступа для пользователя
    this.boardAccessLevelHubService.onUserAccessLevelDeleted(
      (boardId, user) => {
        // Поиск пользователя с общими досками
        let userWithSharedBoards = this.usersWithSharedBoards.find(
          (i) => i.id == user.id
        );

        // Проверка, остались ли у пользователя доски после удаления уровня доступа
        if (
          (userWithSharedBoards.ownBoards.length === 1 &&
            userWithSharedBoards.ownBoards.filter((i) => i.id === boardId)) ||
          userWithSharedBoards.ownBoards.length === 0
        ) {
          // Удаление пользователя из списка, если у него не осталось досок
          this.usersWithSharedBoards = this.usersWithSharedBoards.filter(
            (i) => i.id !== user.id
          );
        }
      }
    );
  }
}
