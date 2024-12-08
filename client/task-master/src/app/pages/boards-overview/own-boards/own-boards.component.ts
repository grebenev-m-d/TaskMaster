import { Component, ElementRef, ViewChild } from '@angular/core';
import { EntityState } from '../../../models/EntityState';
import { Board } from '../../../models/Board';
import { DesignType } from '../../../models/enum/DesignType';
import { MatDialog } from '@angular/material/dialog';
import { BoardHubService } from '../../../services/hub.services/boardHub.service';
import { BoardApiService } from '../../../services/api.services/boardApi.service';
import { ActivatedRoute } from '@angular/router';
import { BoardComponent } from '../../../components/board/board.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoaderComponent } from '../../../components/loader/loader.component';
import { showError } from '../../../helpers/exceptionMessageHelper';
import { BoardAccessLevelHubService } from '../../../services/hub.services/boardAccessLevelHub.service';

@Component({
  selector: 'app-own-boards',
  standalone: true,
  imports: [BoardComponent, FormsModule, CommonModule, LoaderComponent],
  templateUrl: './own-boards.component.html',
  styleUrl: './own-boards.component.css',
})
export class OwnBoardsComponent {
  // История последних просмотренных досок
  historyLastViewedBoards: Board[];
  // Разрешить выбор досок
  allowSelection: boolean = false;

  // Состояния собственных досок
  ownedBoardStates: EntityState<Board>[] = [];

  // Номер текущей страницы с собственными досками
  currentPageOwnedBoard: number = 1;
  // Количество досок на странице с собственными досками
  numberBoardsPerPageOwned: number = 5;
  // Общее количество собственных досок
  totalNumberOwnedBoards: number = 0;

  // Сервис для работы с досками
  boardHubService: BoardHubService;
  // Сервис для управления уровнями доступа к доскам
  boardAccessLevelHubService: BoardAccessLevelHubService;

  // Флаг создания новой доски
  isCreatedBoard: boolean = false;
  // Заголовок создаваемой доски
  boardCreationTitle: string;

  // Ссылка на элемент формы создания доски
  @ViewChild('boardCreation') boardCreation: ElementRef;
  // Ссылка на кнопку открытия формы создания доски
  @ViewChild('openButtonBoardCreation') openButtonBoardCreation: ElementRef;

  // Конструктор компонента
  constructor(
    private boardApiService: BoardApiService,
    public dialog: MatDialog
  ) {}

  // Инициализация компонента
  async ngOnInit() {
    // Инициализация сервиса для работы с досками
    this.boardHubService = new BoardHubService();

    // Установка соединения с хабом
    await this.boardHubService
      .startConnection()
      .then(async () => {
        this.registerEventHandlers();
        this.initializeBoardState();
      })
      .catch((e) => {
        showError(this.dialog, e);
      });

    // Инициализация сервиса для управления уровнями доступа к доскам
    this.boardAccessLevelHubService = new BoardAccessLevelHubService();
    this.boardAccessLevelHubService.startConnection().catch((e) => {
      showError(this.dialog, e);
    });
  }

  // Выбор доски
  selectBoard(ownedBoardState: EntityState<Board>) {
    if (this.allowSelection) {
      ownedBoardState.selected = !ownedBoardState.selected;
    }
  }

  // Удаление выбранных досок
  async deleteSelectedBoards() {
    for (const item of this.ownedBoardStates) {
      if (item.selected) {
        await this.boardHubService.deleteBoard(item.entity.id).catch((e) => {
          showError(this.dialog, e);
        });
      }
    }
  }

  // Создание новой доски
  async createBoard() {
    await this.boardHubService
      .createBoard(this.boardCreationTitle)
      .catch((e) => {
        showError(this.dialog, e);
      });

    this.hideFormCreatedBoard();
  }

  // Обработчик клика на документе
  handleDocumentClick = (event: MouseEvent) => {
    const target = event.target as HTMLElement;
    if (document.body.contains(target)) {
      if (
        !this.openButtonBoardCreation?.nativeElement.contains(target) &&
        !this.boardCreation.nativeElement.contains(target)
      ) {
        this.hideFormCreatedBoard();
      }
    }
  };

  // Отображение формы создания доски
  showFormCreatedBoard() {
    this.isCreatedBoard = true;
    document.addEventListener('click', this.handleDocumentClick);
  }

  // Скрытие формы создания доски
  hideFormCreatedBoard() {
    document.removeEventListener('click', this.handleDocumentClick);
    this.isCreatedBoard = false;
    this.boardCreationTitle = '';
  }

  // Загружает дополнительные собственные доски
  async showMoreOwnedBoards() {
    ++this.currentPageOwnedBoard;
    // Получение дополнительных собственных досок с сервера
    await this.boardHubService
      .getOwnBoards(this.currentPageOwnedBoard, this.numberBoardsPerPageOwned)
      .then((ownedBoardData) => {
        // Обработка изображений в досках и обновление состояния собственных досок
        this.processImagesInBoards(ownedBoardData)
          .then((boards) => {
            this.ownedBoardStates = this.ownedBoardStates.concat(
              this.processBoards(boards)
            );
          })
          .catch((e) => {
            showError(this.dialog, e);
          });
      })
      .catch((e) => {
        showError(this.dialog, e);
      });
  }

  // Регистрирует обработчики событий хаба для досок
  async registerEventHandlers() {
    // Обработчик обновления истории последних просмотренных досок
    await this.boardHubService.onUpdatedHistoryLastViewedBoards((boards) => {
      this.historyLastViewedBoards = boards;
    });

    // Обработчик создания новой доски
    await this.boardHubService.onBoardCreated((board) => {
      this.ownedBoardStates.push(new EntityState(board));
      ++this.totalNumberOwnedBoards;
    });

    // Обработчик обновления владельца доски
    await this.boardHubService.onOwnerUpdated((boardId, owner) => {
      this.ownedBoardStates.forEach((item) => {
        if (item.entity.id === boardId) {
          item.entity.owner = owner;
        }
      });
    });

    // Обработчик удаления доски
    await this.boardHubService.onBoardDeleted((boardId) => {
      this.ownedBoardStates = this.ownedBoardStates.filter(
        (item) => item.entity.id !== boardId
      );
      --this.totalNumberOwnedBoards;
    });
  }

  // Инициализирует состояние досок
  async initializeBoardState() {
    // Получение общего количества собственных досок
    await this.boardHubService
      .getTotalNumberOwnBoards()
      .then((b) => {
        this.totalNumberOwnedBoards = b;
      })
      .catch((e) => {
        showError(this.dialog, e);
      });

    // Получение собственных досок для текущей страницы
    await this.boardHubService
      .getOwnBoards(this.currentPageOwnedBoard, this.numberBoardsPerPageOwned)
      .then((ownedBoardData) => {
        // Обработка изображений в досках и обновление состояния собственных досок
        this.processImagesInBoards(ownedBoardData)
          .then((boards) => {
            this.ownedBoardStates = this.processBoards(boards);
          })
          .catch((e) => {
            showError(this.dialog, e);
          });
      })
      .catch((e) => {
        showError(this.dialog, e);
      });

    // Получение истории последних просмотренных досок
    await this.boardHubService
      .getHistoryLastViewedBoards()
      .then((historyLastViewedBoards) => {
        // Обработка изображений в досках и обновление состояния истории последних просмотренных досок
        this.processImagesInBoards(historyLastViewedBoards).then(
          (i) => (this.historyLastViewedBoards = i)
        );
      })
      .catch((e) => {
        showError(this.dialog, e);
      });
  }

  // Обрабатывает доски для формирования EntityState
  processBoards(boards: Board[]) {
    let boardStates = [];
    for (let i = 0; i < boards.length; i++) {
      let boardState = new EntityState(boards[i]);
      boardStates.push(boardState);
    }
    return boardStates;
  }

  // Обрабатывает изображения в досках
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
