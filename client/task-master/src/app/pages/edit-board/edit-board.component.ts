import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  Input,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { CardList } from '../../models/CardList';
import { CardComponent } from '../../components/card/card.component';
import {
  CdkDrag,
  CdkDragDrop,
  CdkDropList,
  CdkDropListGroup,
  moveItemInArray,
  transferArrayItem,
} from '@angular/cdk/drag-drop';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ChangeDetectionStrategy, ViewEncapsulation } from '@angular/core';

import { FormsModule } from '@angular/forms';
import { HostListener } from '@angular/core';
import { BoardApiService } from '../../services/api.services/boardApi.service';
import { BoardHubService } from '../../services/hub.services/boardHub.service';
import { CardHubService } from '../../services/hub.services/cardHub.service';
import { Board } from '../../models/Board';
import { ActivatedRoute } from '@angular/router';
import { CardListHubService } from '../../services/hub.services/cardListHub.service';
import { BoardAccessLevel } from '../../models/BoardAccessLevel';
import { BoardAccessLevelHubService } from '../../services/hub.services/boardAccessLevelHub.service';
import { AccessLevelType } from '../../models/enum/AccessLevelType';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { DesignType } from '../../models/enum/DesignType';
import { EditBoardModalComponent } from '../../modal/edit-board-modal/edit-board-modal.component';
import { HeaderComponent } from '../../components/header/header.component';
import { CardListComponent } from '../../components/card-list/card-list.component';
import { getUserNameFromJwt } from '../../helpers/jwtServiceHelper';
import { showError } from '../../helpers/exceptionMessageHelper';
import { BoardAccessLevelsComponent } from '../../modal/board-access-levels-modal/board-access-levels-modal.component';

@Component({
  selector: 'app-owned-boards',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
    CardListComponent,
    CardComponent,
    CdkDropListGroup,
    CdkDropList,
    CdkDrag,
    ScrollingModule,
    HeaderComponent,
  ],
  encapsulation: ViewEncapsulation.None,

  templateUrl: './edit-board.component.html',
  styleUrl: './edit-board.component.css',
})
export class EditBoardComponent implements OnInit, OnDestroy {
  // Идентификатор пользователя
  userId: string;

  // Доска для редактирования
  board: Board = new Board();

  // Уровень доступа к доске
  boardAccessLevel: BoardAccessLevel = new BoardAccessLevel();

  // Идентификатор доски
  boardId: string;

  // Перечисление типов дизайна
  public DesignTypeEnum = DesignType;

  // Текущий уровень доступа
  currentAccessLevel: AccessLevelType;

  // Сервис для работы с доской
  boardHubService: BoardHubService;

  // Сервис для работы с списками карточек
  cardListHubService: CardListHubService;

  // Сервис для работы с карточками
  cardHubService: CardHubService;

  // Сервис для уровней доступа к доске
  boardAccessLevelHubService: BoardAccessLevelHubService;

  // Конструктор компонента
  constructor(
    private boardApiService: BoardApiService,
    public dialog: MatDialog,
    private route: ActivatedRoute
  ) {}

  // Инициализация компонента
  async ngOnInit() {
    // Получение токена доступа и идентификатора пользователя
    let accessToken = localStorage.getItem('accessToken');
    this.userId = getUserNameFromJwt(accessToken);

    // Получение идентификатора доски из маршрута
    this.boardId = this.route.snapshot.paramMap.get('id');

    // Инициализация сервиса для работы с доской
    this.boardHubService = new BoardHubService(this.boardId);
    this.boardHubService
      .startConnection()
      .then(async () => {
        // Регистрация обработчиков событий для доски
        this.registerBoardEventHandlers();
        // Инициализация состояния доски
        this.initializeBoardState();
      })
      .catch((e) => {
        showError(this.dialog, e);
      });

    // Инициализация сервиса для работы со списками карточек
    this.cardListHubService = new CardListHubService(this.boardId);
    this.cardListHubService.startConnection().then(async () => {
      // Регистрация обработчиков событий для списков карточек
      this.registerCardListEventHandlers();
      // Инициализация состояния списков карточек
      this.initializeCardListState();
    });

    // Инициализация сервиса для работы с карточками
    this.cardHubService = new CardHubService(this.boardId);
    this.cardHubService.startConnection().then(async () => {
      // Регистрация обработчиков событий для карточек
      this.registerCardEventHandlers();
    });

    // Инициализация сервиса для уровней доступа к доске
    this.boardAccessLevelHubService = new BoardAccessLevelHubService(
      this.boardId
    );
    this.boardAccessLevelHubService.startConnection().then(async () => {
      // Инициализация состояния уровней доступа к доске
      this.initializeBoardAccessLevelState();
      // Регистрация обработчиков событий для уровней доступа к доске
      this.registerBoardAccessLevelEventHandlers();
    });
  }

  // Действия при уничтожении компонента
  ngOnDestroy() {
    document.removeEventListener('click', this.handleDocumentClick);
  }

  // Обработчик события перетаскивания
  drop(event: CdkDragDrop<any>) {
    // Получение перемещаемого списка карточек
    let movedList = this.board.cardLists[event.previousIndex];

    // Перемещение списка в новую позицию
    moveItemInArray(
      this.board.cardLists,
      event.previousIndex,
      event.currentIndex
    );

    // Получение предыдущего списка
    let previousList =
      event.currentIndex === 0
        ? null
        : this.board.cardLists[event.currentIndex - 1];

    // Отправка запроса на перемещение списка карточек
    this.cardListHubService.moveCardList(
      movedList.id,
      previousList == null ? null : previousList.id
    );
  }

  // Изменение уровня доступа к доске
  changeAccessLevel() {
    // Конфигурация диалогового окна
    const config = new MatDialogConfig();
    config.data = {
      board: this.board,
      boardHubService: this.boardHubService,
      boardAccessLevelHubService: this.boardAccessLevelHubService,
    };
    // Открытие диалогового окна для выбора уровня доступа
    const dialogRef = this.dialog.open(BoardAccessLevelsComponent, config);

    // Ожидание закрытия диалогового окна и вывод результата в консоль
    dialogRef.afterClosed().subscribe((result) => {
      console.log(result);
    });
  }

  // Ссылка на блок добавления списка карточек
  @ViewChild('addListBlock') addListBlockRef: ElementRef;

  // Обработка щелчка на документе
  handleDocumentClick = (event: MouseEvent) => {
    const target = event.target as HTMLElement; // Получение целевого элемента
    if (document.body.contains(target)) {
      const addListBlock = this.addListBlockRef.nativeElement;
      // Проверка, не является ли целевой элемент блоком добавления списка
      if (!addListBlock.contains(target)) {
        this.hideForm();
      }
    }
  };

  // Текст нового списка карточек
  text: string;

  // Видимость формы добавления списка
  formVisibility: boolean = false;

  // Добавление списка карточек
  async addCardList() {
    await this.cardListHubService
      .createCardList(this.board.id, this.text)
      .catch((e) => {
        showError(this.dialog, e);
      });
    this.hideForm();
  }

  // Отображение формы добавления списка
  showForm() {
    console.log(this.board.cardLists);
    document.addEventListener('click', this.handleDocumentClick);
    this.formVisibility = true;
  }

  // Скрытие формы добавления списка
  hideForm() {
    document.removeEventListener('click', this.handleDocumentClick);
    this.formVisibility = false;
    this.text = '';
  }

  // Открытие модального окна для редактирования доски
  openModalEditBoard(event: MouseEvent) {
    event.preventDefault();

    const config = new MatDialogConfig();
    config.panelClass = 'custom-dialog-cont';

    config.data = {
      board: this.board,
      boardHubService: this.boardHubService,
    };

    const dialogRef = this.dialog.open(EditBoardModalComponent, config);

    dialogRef.afterClosed().subscribe((result) => {});
  }

  // Регистрация обработчиков событий для доски
  async registerBoardEventHandlers() {
    // Обновление заголовка доски
    await this.boardHubService.onTitleUpdated(
      (boardId: string, title: string) => {
        if (this.board.id == boardId) {
          this.board.title = title;
        }
      }
    );
    // Обновление цвета доски
    await this.boardHubService.onColorUpdated(
      (boardId: string, colorCode: string) => {
        if (this.board.id == boardId) {
          this.board.colorCode = colorCode;
        }
      }
    );
    // Обновление владельца доски
    await this.boardHubService.onOwnerUpdated((boardId, owner) => {
      if (this.board.id !== boardId) {
        return;
      }
      if (this.board.owner.id !== owner.id) {
        window.location.reload();
      }
    });
    // Обновление типа дизайна доски
    await this.boardHubService.onDesignTypeUpdated((boardId, designType) => {
      if (this.board.id !== boardId) {
        return;
      }
      this.board.designType = designType;
    });
    // Обновление изображения доски
    await this.boardHubService.onImageUpdated((boardId) => {
      if (this.board.id === boardId) {
        this.boardApiService.getImage(this.board.id).then((file) => {
          if (file) {
            this.board.imageUrl = window.URL.createObjectURL(file.data);
          }
        });
      }
    });
  }

  // Инициализация состояния доски
  async initializeBoardState() {
    try {
      // Получение информации о доске
      const board = await this.boardHubService.getBoard(this.boardId);
      // Проверка типа дизайна доски
      if (board.designType === DesignType.image) {
        // Получение изображения доски
        const file = await this.boardApiService.getImage(board.id);
        if (file) {
          board.imageUrl = window.URL.createObjectURL(file.data);
          console.log(this.board);
          console.log(window.URL.createObjectURL(file.data));
        }
      }
      // Присвоение информации о доске
      this.board.assign(board);
    } catch (e) {
      showError(this.dialog, e);
    }
  }

  // Регистрация обработчиков событий для списков карточек
  async registerCardListEventHandlers() {
    // Обработчик создания списка карточек
    await this.cardListHubService.onCardListCreated((boardId, cardList) => {
      if (this.board.id == boardId) {
        this.board.cardLists.push(cardList);
      }
    });
    // Обработчик удаления списка карточек
    await this.cardListHubService.onCardListDeleted((cardListId) => {
      this.board.cardLists = this.board.cardLists.filter(
        (item) => item.id !== cardListId
      );
    });
    // Обработчик перемещения списка карточек
    await this.cardListHubService.onCardListMoved(
      (movedCardListId, prevCardListId) => {
        // Получение индексов перемещаемого и предыдущего списка
        let movedCardListIndex = this.board.cardLists.indexOf(
          this.board.cardLists.find(
            (cardList) => cardList.id === movedCardListId
          )
        );
        let prevCardListIndex = this.board.cardLists.indexOf(
          this.board.cardLists.find(
            (cardList) => cardList.id === prevCardListId
          )
        );

        // Проверка на возможность перемещения списка
        if (prevCardListId === null && movedCardListIndex === 0) {
          return;
        } else if (prevCardListIndex + 1 === movedCardListIndex) {
          return;
        }

        // Определение индексов для перемещения списка
        let previousIndex = movedCardListIndex;
        let currentIndex =
          prevCardListId === null
            ? 0
            : prevCardListIndex + 1 > previousIndex
            ? prevCardListIndex
            : prevCardListIndex + 1;

        // Перемещение списка
        moveItemInArray(this.board.cardLists, previousIndex, currentIndex);
      }
    );
  }

  // Инициализация состояния списков карточек
  async initializeCardListState() {
    // Получение списков карточек для доски
    this.cardListHubService.getCardLists(this.boardId).then((cardLists) => {
      // Присвоение списков карточек доске
      this.board.cardLists = cardLists.map(
        (x, i) => new CardList(cardLists[i])
      );
    });
  }

  // Регистрация обработчиков событий для карточек
  async registerCardEventHandlers() {
    // Обработчик перемещения карточки
    await this.cardHubService.onCardMoved(
      (boardId, currentCardListId, movedCardId, prevCardId) => {
        // Проверка идентификатора доски
        if (this.board.id !== boardId) {
          return;
        }

        // Получение текущего и предыдущего списка карточек
        let currentCardList = this.board.cardLists.find(
          (list) => list.id == currentCardListId
        );
        let previousCardList = this.board.cardLists.find((list) =>
          list.cards.find((card) => card.id === movedCardId)
        );

        // Получение перемещаемой карточки
        let movedCard = previousCardList.cards.find((i) => i.id == movedCardId);

        // Удаление карточки из предыдущего списка
        previousCardList.cards = previousCardList.cards.filter(
          (i) => i.id != movedCardId
        );

        // Определение индекса для вставки перемещаемой карточки
        let insertIndex = prevCardId
          ? currentCardList.cards.findIndex((card) => card.id === prevCardId) +
            1
          : currentCardList.cards.length;

        // Вставка перемещаемой карточки в новый список
        prevCardId === null
          ? currentCardList.cards.unshift(movedCard)
          : currentCardList.cards.splice(insertIndex, 0, movedCard);
      }
    );
  }

  // Инициализация состояния уровня доступа к доске
  async initializeBoardAccessLevelState() {
    // Получение текущего уровня доступа к доске
    this.currentAccessLevel =
      await this.boardAccessLevelHubService.getUserAccessLevel(this.boardId);
  }

  // Регистрация обработчиков событий для уровней доступа к доске
  async registerBoardAccessLevelEventHandlers() {
    // Обработчик обновления уровня доступа к доске
    await this.boardAccessLevelHubService.onUserAccessLevelUpdated(
      (boardId, permission) => {
        if (this.board.id !== boardId) {
          return;
        }
        // Перезагрузка страницы при изменении уровня доступа
        if (this.currentAccessLevel !== permission) {
          window.location.reload();
        }
      }
    );
  }
}
