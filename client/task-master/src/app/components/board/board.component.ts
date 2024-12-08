import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { Route, RouterLink } from '@angular/router';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { BoardHubService } from '../../services/hub.services/boardHub.service';
import { Board } from '../../models/Board';
import { EntityState } from '../../models/EntityState';
import { EditBoardModalComponent } from '../../modal/edit-board-modal/edit-board-modal.component';
import { AccessLevelType } from '../../models/enum/AccessLevelType';
import { DesignType } from '../../models/enum/DesignType';
import { BoardApiService } from '../../services/api.services/boardApi.service';
import { BoardAccessLevelHubService } from '../../services/hub.services/boardAccessLevelHub.service';
@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCheckboxModule],
  templateUrl: './board.component.html',
  styleUrl: './board.component.css',
})
export class BoardComponent implements OnInit {
  // Позволяет выбирать элементы
  @Input() allowSelection: boolean = false;

  // Текущий уровень доступа
  currentAccessLevel: AccessLevelType = null;

  // Сервис хаба доски
  @Input() boardHubService: BoardHubService;

  // Идентификатор доски
  @Input() boardId: string;

  // Флаг выбранной доски
  @Input() hasBoardSelected: boolean;

  // Доска
  @Input() board: Board;

  // Сервис хаба уровня доступа к доске
  @Input() boardAccessLevelHubService: BoardAccessLevelHubService;

  // Типы дизайна
  public DesignTypeEnum = DesignType;

  constructor(
    private boardApiService: BoardApiService,
    public dialog: MatDialog
  ) {}

  // Инициализация компонента
  async ngOnInit() {
    // Если сервис хаба доски не указан, создаем новый
    if (!this.boardHubService) {
      // Проверка наличия идентификатора доски
      if (!this.boardId) {
        throw new Error('Для создания соединения требуется id доски.');
      }

      this.boardHubService = new BoardHubService();

      // Установка соединения с хабом доски
      await this.boardHubService
        .startConnection()
        .then(async () => {
          this.registerBoardEventHandlers();
          this.initializeBoardState();
        })
        .catch((err) => console.error('Ошибка подключения к SignalR:', err));
    } else {
      this.registerBoardEventHandlers();
    }

    // Получение уровня доступа пользователя к доске
    this.currentAccessLevel =
      this.boardAccessLevelHubService != null
        ? await this.boardAccessLevelHubService.getUserAccessLevel(
            this.board.id
          )
        : null;
  }

  // Регистрация обработчиков событий доски
  async registerBoardEventHandlers() {
    // Обновление заголовка доски
    await this.boardHubService.onTitleUpdated((boardId, title) => {
      if (this.board.id === boardId) {
        this.board.title = title;
      }
    });

    // Обновление цвета доски
    await this.boardHubService.onColorUpdated((boardId, colorCode) => {
      if (this.board.id === boardId) {
        this.board.colorCode = colorCode;
      }
    });

    // Обновление типа дизайна доски
    await this.boardHubService.onDesignTypeUpdated((boardId, designType) => {
      if (this.board.id === boardId) {
        this.board.designType = designType;
      }
    });

    // Обновление изображения доски
    await this.boardHubService.onImageUpdated((boardId) => {
      if (this.board.id === boardId) {
        if ((this.board.designType = DesignType.image)) {
          this.boardApiService.getImage(this.board.id).then((file) => {
            if (file) {
              this.board.imageUrl = window.URL.createObjectURL(file.data);
            }
          });
        }
      }
    });
  }

  // Инициализация состояния доски
  async initializeBoardState() {
    this.board = await this.boardHubService.getBoard(this.boardId);
  }

  // Открытие модального окна редактирования доски
  openModalEditBoard(event: MouseEvent) {
    // Проверка уровня доступа для редактирования
    if (!(this.currentAccessLevel >= 2)) {
      return;
    }
    event.preventDefault();

    // Конфигурация модального окна
    const config = new MatDialogConfig();
    config.panelClass = 'custom-dialog-cont';

    config.data = {
      board: this.board,
      boardHubService: this.boardHubService,
    };

    // Открытие модального окна редактирования доски
    this.dialog.open(EditBoardModalComponent, config);
  }
}
