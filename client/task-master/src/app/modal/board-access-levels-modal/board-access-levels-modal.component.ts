import { Component, Inject, Input, OnInit, Output } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogRef,
} from '@angular/material/dialog';
import { AccessLevelType } from '../../models/enum/AccessLevelType';
import { UserAccessLevel } from '../../models/UserAccessLevel';
import { FormsModule, NgModel } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BoardAccessLevel } from '../../models/BoardAccessLevel';
import { BoardAccessLevelHubService } from '../../services/hub.services/boardAccessLevelHub.service';
import { Board } from '../../models/Board';
import { User } from '../../models/User';
import { BoardHubService } from '../../services/hub.services/boardHub.service';
import { showError } from '../../helpers/exceptionMessageHelper';

@Component({
  selector: 'app-board-access-levels-modal',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './board-access-levels-modal.component.html',
  styleUrl: './board-access-levels-modal.component.css',
})
export class BoardAccessLevelsComponent implements OnInit {
  board: Board; // Доска, к которой применяются настройки доступа
  boardHubService: BoardHubService; // Сервис для взаимодействия с хабом доски
  owner: User; // Владелец доски
  boardAccessLevelHubService: BoardAccessLevelHubService; // Сервис для взаимодействия с хабом уровней доступа к доске
  PUBLICLY: boolean = true; // Флаг общедоступности доски
  PRIVATE: boolean = false; // Флаг приватности доски

  constructor(
    public dialog: MatDialog, // Диалоговое окно для отображения сообщений об ошибках
    private dialogRef: MatDialogRef<BoardAccessLevelsComponent>, // Ссылка на диалоговое окно
    @Inject(MAT_DIALOG_DATA) private data: any // Данные, переданные в диалоговое окно
  ) {
    this.board = data.board; // Инициализация доски
    this.owner = this.board.owner; // Инициализация владельца доски
    this.ownerEmail = this.board.owner.email; // Инициализация электронной почты владельца доски

    this.boardHubService = data.boardHubService; // Инициализация сервиса хаба доски
    this.boardAccessLevelHubService = data.boardAccessLevelHubService; // Инициализация сервиса хаба уровней доступа к доске
  }

  public AccessLevelTypeEnum = AccessLevelType; // Перечисление типов уровней доступа

  boardAccessLevel: BoardAccessLevel = new BoardAccessLevel(); // Уровень доступа к доске
  accessLevel: AccessLevelType; // Уровень доступа

  ownerEmail: string; // Электронная почта владельца доски
  ownerSuggestions: User[] = []; // Список пользователей для предложения в качестве нового владельца

  // Изменение владельца доски
  async changeOwner() {
    await this.boardHubService
      .updateOwner(this.board.id, this.ownerEmail) // Обновление владельца доски
      .catch((e) => {
        showError(this.dialog, e); // Отображение ошибки при возникновении ошибки
      });
  }

  userEmail: string; // Электронная почта пользователя для добавления нового уровня доступа

  // Добавление нового уровня доступа к доске
  async addAccessLevel() {
    if (this.accessLevel != null) {
      await this.boardAccessLevelHubService
        .addPersonalAccessLevel(this.board.id, this.userEmail, this.accessLevel) // Добавление персонального уровня доступа к доске для пользователя
        .catch((e) => {
          showError(this.dialog, e); // Отображение ошибки при возникновении ошибки
        });
    }
  }

  // Изменение уровня доступа пользователя к доске
  async changeAccessLevel(accessLevel: UserAccessLevel) {
    await this.boardAccessLevelHubService
      .updatePersonalAccessLevel(
        this.board.id,
        accessLevel.user.id,
        accessLevel.accessLevel // Обновление персонального уровня доступа к доске для пользователя
      )
      .catch((e) => {
        showError(this.dialog, e); // Отображение ошибки при возникновении ошибки
      });
  }

  // Удаление уровня доступа пользователя к доске
  async deleteAccessLevel(accessLevel: UserAccessLevel) {
    await this.boardAccessLevelHubService
      .deletePersonalAccessLevel(this.board.id, accessLevel.user.id) // Удаление персонального уровня доступа к доске для пользователя
      .catch((e) => {
        showError(this.dialog, e); // Отображение ошибки при возникновении ошибки
      });
  }

  // Изменение общедоступности доски
  async changeIsPublic() {
    this.boardAccessLevel.isPublic = !this.boardAccessLevel.isPublic; // Изменение состояния общедоступности доски
    await this.boardAccessLevelHubService
      .updateBoardPublicStatus(this.board.id, this.boardAccessLevel.isPublic) // Обновление статуса общедоступности доски
      .catch((e) => {
        showError(this.dialog, e); // Отображение ошибки при возникновении ошибки
      });
  }

  // Изменение общего уровня доступа к доске
  async changeGeneralAccessLevel(accessLevelType: AccessLevelType) {
    await this.boardAccessLevelHubService
      .updateBoardGeneralAccessLevel(this.board.id, accessLevelType) // Обновление общего уровня доступа к доске
      .catch((e) => {
        showError(this.dialog, e); // Отображение ошибки при возникновении ошибки
      });
  }

  // Инициализация компонента при его создании
  async ngOnInit() {
    await this.initializeBoardAccessLevelState(); // Инициализация состояния уровней доступа к доске
    await this.registerBoardAccessLevelEventHandlers(); // Регистрация обработчиков событий уровней доступа к доске
  }

  // Регистрация обработчиков событий доски и уровней доступа к доске
  async registerBoardEventHandlers() {
    await this.boardHubService.onOwnerUpdated((boardId, owner) => {
      if (this.board.id !== boardId) {
        return;
      }
      this.owner = owner;
    });
    await this.boardAccessLevelHubService.onBoardPublicStatusUpdated(
      (boardId, isPublic) => {
        if (this.board.id !== boardId) {
          return;
        }

        this.boardAccessLevel.isPublic = isPublic;
      }
    );

    await this.boardAccessLevelHubService.onBoardDefaultAccessLevelUpdated(
      (boardId, accessLevel) => {
        if (this.board.id !== boardId) {
          return;
        }
        this.boardAccessLevel.defaultAccessLevelType = accessLevel;
      }
    );
    await this.boardAccessLevelHubService.onPersonalAccessLevelAdded(
      (boardId, user, accessLevel) => {
        if (this.board.id !== boardId) {
          return;
        }
        let userAccessLevel = new UserAccessLevel();
        userAccessLevel.user = user;
        userAccessLevel.accessLevel = accessLevel;

        this.boardAccessLevel.personalAccessLevels.push(userAccessLevel);
      }
    );
  }

  // Инициализация начального состояния уровней доступа к доске
  async initializeBoardAccessLevelState() {
    let accessLevel = await this.boardAccessLevelHubService.getBoardAccessLevel(
      this.board.id
    );

    this.boardAccessLevel.assign(accessLevel); // Инициализация уровней доступа к доске
  }

  // Регистрация обработчиков событий уровней доступа к доске
  async registerBoardAccessLevelEventHandlers() {
    await this.boardAccessLevelHubService.onPersonalAccessLevelUpdated(
      (boardId, userId, accessLevel) => {
        if (this.board.id !== boardId) {
          return;
        }

        this.boardAccessLevel.personalAccessLevels.find(
          (i) => i.user.id === userId
        ).accessLevel = accessLevel; // Обновление персонального уровня доступа к доске для пользователя
      }
    );
    await this.boardAccessLevelHubService.onBoardPublicStatusUpdated(
      (boardId, isPublic) => {
        if (this.board.id !== boardId) {
          return;
        }

        this.boardAccessLevel.isPublic = isPublic; // Обновление статуса общедоступности доски
      }
    );
    await this.boardAccessLevelHubService.onBoardDefaultAccessLevelUpdated(
      (boardId, accessLevel) => {
        if (this.board.id !== boardId) {
          return;
        }
        this.boardAccessLevel.defaultAccessLevelType = accessLevel; // Обновление общего уровня доступа к доске
      }
    );
    await this.boardAccessLevelHubService.onPersonalAccessLevelAdded(
      (boardId, user, accessLevel) => {
        if (this.board.id !== boardId) {
          return;
        }
        let userAccessLevel = new UserAccessLevel();
        userAccessLevel.user = user;
        userAccessLevel.accessLevel = accessLevel;
        // Добавление персонального уровня доступа к доске для пользователя
        this.boardAccessLevel.personalAccessLevels.push(userAccessLevel);
      }
    );
    await this.boardAccessLevelHubService.onPersonalAccessLevelDeleted(
      (boardId, userId) => {
        if (this.board.id !== boardId) {
          return;
        }

        this.boardAccessLevel.personalAccessLevels =
          this.boardAccessLevel.personalAccessLevels.filter(
            (i) => i.user.id !== userId
          ); // Удаление персонального уровня доступа к доске для пользователя
      }
    );
  }

  // Закрытие диалогового окна с подтверждением
  onConfirm(): void {
    this.dialogRef.close(true);
  }

  // Закрытие диалогового окна без подтверждения
  onCancel(): void {
    this.dialogRef.close(false);
  }
}
