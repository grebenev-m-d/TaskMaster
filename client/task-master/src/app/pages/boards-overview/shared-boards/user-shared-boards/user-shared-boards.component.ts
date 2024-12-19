import { Component, Input } from '@angular/core';
import { Board } from '../../../../models/Board';
import { BoardHubService } from '../../../../services/hub.services/boardHub.service';
import { DesignType } from '../../../../models/enum/DesignType';
import { BoardApiService } from '../../../../services/api.services/boardApi.service';
import { MatDialog } from '@angular/material/dialog';
import { User } from '../../../../models/User';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BoardComponent } from '../../../../components/board/board.component';
import { BoardAccessLevelHubService } from '../../../../services/hub.services/boardAccessLevelHub.service';

@Component({
  selector: 'app-user-shared-boards',
  standalone: true,
  imports: [FormsModule, CommonModule, BoardComponent],
  templateUrl: './user-shared-boards.component.html',
  styleUrl: './user-shared-boards.component.css',
})
export class UserSharedBoardsComponent {
  @Input() user: User;

  // Номер текущей страницы с личными досками.
  currentPageSharedBoard: number = 1;

  // Количество досок на странице с личными досками.
  numberSharedBoardsPerPage: number = 5;

  // Общее количество.
  totalNumberSharedBoards: number = 0;

  @Input() boardHubService: BoardHubService;
  @Input() boardAccessLevelHubService: BoardAccessLevelHubService;

  // Инициализация компонента
  async ngOnInit() {
    // Регистрация обработчиков событий для досок
    await this.registerBoardEventHandlers();
    // Инициализация состояния досок
    await this.initializeBoardState();
    // Регистрация обработчиков событий для уровней доступа к доскам
    await this.registerBoardAccessLevelEventHandlers();

    // Инициализация сервиса для уровней доступа к доскам
    this.boardAccessLevelHubService = new BoardAccessLevelHubService();
  }

  constructor(
    private boardApiService: BoardApiService,
    public dialog: MatDialog
  ) {}

  // Показать больше общих досок
  async showMoreSharedBoards() {
    // Увеличение номера текущей страницы
    ++this.currentPageSharedBoard;

    let boards = await this.boardHubService.getSharedBoardsWithUser(
      this.user.id,
      this.currentPageSharedBoard,
      this.numberSharedBoardsPerPage
    );

    boards = await this.processImagesInBoards(boards);

    // Получение и объединение общих досок с пользователем
    this.user.ownBoards = this.user.ownBoards.concat(boards);
  }

  // Регистрация обработчиков событий для досок
  async registerBoardEventHandlers() {
    // Обновление заголовка доски
    await this.boardHubService.onTitleUpdated((boardId, title) => {
      this.user.ownBoards.forEach((item) => {
        if (item.id === boardId) {
          item.title = title;
        }
      });
    });

    // Обновление цвета доски
    await this.boardHubService.onColorUpdated((boardId, colorCode) => {
      this.user.ownBoards.forEach((item) => {
        if (item.id === boardId) {
          item.colorCode = colorCode;
        }
      });
    });

    // Обновление типа дизайна доски
    await this.boardHubService.onDesignTypeUpdated((boardId, designType) => {
      this.user.ownBoards.forEach((item) => {
        if (item.id === boardId) {
          item.designType = designType;
        }
      });
    });

    // Обновление владельца доски
    await this.boardHubService.onOwnerUpdated((boardId, owner) => {
      this.user.ownBoards.forEach((item) => {
        if (item.id === boardId) {
          item.owner = owner;
        }
      });
    });

    // Удаление доски
    await this.boardHubService.onBoardDeleted((boardId) => {
      this.user.ownBoards = this.user.ownBoards.filter(
        (item) => item.id !== boardId
      );
      --this.totalNumberSharedBoards;
    });

    // Обновление изображения доски
    await this.boardHubService.onImageUpdated((boardId) => {
      this.user.ownBoards.forEach((item) => {
        if (item.id === boardId) {
          if ((item.designType = DesignType.image)) {
            this.boardApiService.getImage(item.id).then((file) => {
              if (file) {
                item.imageUrl = window.URL.createObjectURL(file.data);
              }
            });
          }
        }
      });
    });
  }

  // Инициализация состояния досок
  async initializeBoardState() {
    // Получение общего количества досок с пользователем
    this.totalNumberSharedBoards =
      await this.boardHubService.getTotalNumberSharedBoardsWithUser(
        this.user.id
      );

    // Получение и установка общих досок с пользователем
    let boards = await this.boardHubService.getSharedBoardsWithUser(
      this.user.id,
      this.currentPageSharedBoard,
      this.numberSharedBoardsPerPage
    );

    this.user.ownBoards = await this.processImagesInBoards(boards);
  }

  // Регистрация обработчиков событий для уровней доступа к доскам
  async registerBoardAccessLevelEventHandlers() {
    // Добавление уровня доступа для пользователя
    this.boardAccessLevelHubService.onUserAccessLevelAdded(
      (boardId, user, permission) => {
        if (this.user.id !== user.id) {
          return;
        }
        this.boardHubService
          .getBoard(boardId)
          .then((b) => this.user.ownBoards.push(b));

        ++this.totalNumberSharedBoards;
      }
    );

    // Удаление уровня доступа для пользователя
    this.boardAccessLevelHubService.onUserAccessLevelDeleted(
      (boardId, user) => {
        if (this.user.id !== user.id) {
          return;
        }
        // Удаление доски пользователя
        this.user.ownBoards = this.user.ownBoards.filter(
          (i) => i.id !== boardId
        );
        --this.totalNumberSharedBoards;

        // Перезагрузка последней страницы
        --this.currentPageSharedBoard;
        this.user.ownBoards = this.user.ownBoards.slice(
          0,
          -this.numberSharedBoardsPerPage
        );

        ++this.currentPageSharedBoard;

        // Получение и добавление дополнительных досок
        this.boardHubService
          .getSharedBoardsWithUser(
            this.user.id,
            this.currentPageSharedBoard,
            this.numberSharedBoardsPerPage
          )
          .then((i) => i.forEach((j) => this.user.ownBoards.push(j)));
      }
    );
  }
  async processImagesInBoards(boards: Board[]) {
    for (let i = 0; i < boards.length; i++) {
      if (boards[i].designType === DesignType.image) {
        // Получение изображения для доски
        let file = await this.boardApiService.getImage(boards[i].id);
        if (file) {
          boards[i].imageUrl = window.URL.createObjectURL(file.data);
        }
      }
    }
    return boards;
  }
}
