import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  Input,
  OnDestroy,
  OnInit,
  Output,
  QueryList,
  ViewChild,
} from '@angular/core';
import {
  CdkDragStart,
  CdkDragEnd,
  CdkDrag,
  CdkDropListGroup,
  CdkDropList,
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem,
  CdkDragPlaceholder,
  CdkDragSortEvent,
} from '@angular/cdk/drag-drop';

import { CardComponent } from '../card/card.component';
import { EditBoardComponent } from '../../pages/edit-board/edit-board.component';
import { FormsModule } from '@angular/forms';
import { CardList } from '../../models/CardList';
import { Card } from '../../models/Card';
import { CardListHubService } from '../../services/hub.services/cardListHub.service';
import { CardHubService } from '../../services/hub.services/cardHub.service';
import { Board } from '../../models/Board';
import { AccessLevelType } from '../../models/enum/AccessLevelType';
import { CardApiService } from '../../services/api.services/cardApi.service';
import { MatDialog } from '@angular/material/dialog';
import { showError } from '../../helpers/exceptionMessageHelper';

@Component({
  selector: 'app-card-list',
  standalone: true,
  imports: [
    FormsModule,
    CdkDropList,
    CdkDropListGroup,
    CdkDrag,
    CommonModule,
    CardComponent,
    CdkDragPlaceholder,
  ],
  templateUrl: './card-list.component.html',
  styleUrl: './card-list.component.css',
})
// Компонент списка карточек
export class CardListComponent implements OnInit, OnDestroy {
  // Индекс списка карточек
  @Input() indexCardList: number;

  // Доска, к которой относится список карточек
  @Input() board: Board;

  // Список карточек
  @Input() cardList: CardList;

  // Сервис хаба списка карточек
  @Input() cardListHubService: CardListHubService;

  // Сервис хаба карточек
  @Input() cardHubService: CardHubService;

  // Текущий уровень доступа
  @Input() currentAccessLevel: AccessLevelType;

  // Флаг перетаскивания и изменения списка
  dragAndDropList: boolean = true;

  // Флаг редактирования
  isEditing: boolean = false;

  // Высота заполнителя
  placeholderHeight: number = 0;

  constructor(
    private cardApiService: CardApiService,
    public dialog: MatDialog
  ) {}

  // Обработчик начала перетаскивания
  handleDragStarted(event: any) {
    this.placeholderHeight = event.source.element.nativeElement.clientHeight;
  }

  // Обновление заголовка списка карточек
  updateTitle() {
    this.cardListHubService.updateTitle(this.cardList.id, this.cardList.title);
  }

  // Обработчик изменения перетаскивания списка
  onDragAndDropListChange(dragAndDropList: boolean) {
    this.dragAndDropList = dragAndDropList;
  }

  // Инициализация компонента
  async ngOnInit() {
    await this.registerCardListEventHandlers();
    await this.registerCardEventHandlers();
    await this.initializeCardState();
  }

  // Уничтожение компонента
  ngOnDestroy() {
    document.removeEventListener('click', this.handleDocumentClick);
  }

  // Обработчик события сброса в списке
  drop(event: any) {
    if (event.previousContainer === event.container) {
      moveItemInArray(
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }

    let currentCard = this.cardList.cards.find(
      (i) => i.id == event.container.data[event.currentIndex].id
    );

    let index = this.cardList.cards.indexOf(currentCard);

    let prevCardId = index == 0 ? null : this.cardList.cards[index - 1].id;

    this.cardHubService.moveCard(
      this.board.id,
      this.cardList.id,
      currentCard.id,
      prevCardId
    );
  }

  // Обработчик клика вне элемента списка карточек
  @ViewChild('addListBlock') addListBlockRef: ElementRef;
  handleDocumentClick = (event: MouseEvent) => {
    const target = event.target as HTMLElement; // Получаем целевой элемент
    if (document.body.contains(target)) {
      const addListBlock = this.addListBlockRef.nativeElement;
      if (!addListBlock.contains(target)) {
        this.hideForm();
      }
    }
  };

  // Удаление списка карточек
  deleteCardList() {
    this.cardListHubService.deleteCardList(this.cardList.id);
  }

  // Текст новой карточки
  text: string;

  // Флаг видимости формы
  formVisibility: boolean = false;

  // Добавление новой карточки в список
  addCard() {
    this.cardHubService.createCard(this.cardList.id, this.text).catch((e) => {
      showError(this.dialog, e);
    });
    this.hideForm();
  }

  // Отображение формы для добавления карточки
  showForm() {
    document.addEventListener('click', this.handleDocumentClick);
    this.formVisibility = true;
  }

  // Скрытие формы для добавления карточки
  hideForm() {
    document.removeEventListener('click', this.handleDocumentClick);
    this.formVisibility = false;
    this.text = '';
  }

  // Регистрация обработчиков событий списка карточек
  async registerCardListEventHandlers() {
    await this.cardListHubService.onCardListTitleUpdated(
      (cardListId, title) => {
        if (this.cardList.id == cardListId) {
          this.cardList.title = title;
        }
      }
    );
  }

  // Регистрация обработчиков событий карточек
  async registerCardEventHandlers() {
    await this.cardHubService.onCardCreated((cardListId, card) => {
      if (this.cardList.id == cardListId) {
        this.cardList.cards.push(card);
      }
    });
    await this.cardHubService.onCardDeleted((cardId) => {
      this.cardList.cards = this.cardList.cards.filter(
        (item) => item.id !== cardId
      );
    });
  }

  // Инициализация состояния списка карточек
  async initializeCardState() {
    this.cardList.cards = await this.cardHubService.getCards(this.cardList.id);

    if (this.cardList.cards) {
      this.cardList.cards.forEach((i) => {
        if (i.hasCoverImage) {
          this.cardApiService.getCoverImage(i.id).then((file) => {
            if (file) {
              i.imageUrl = window.URL.createObjectURL(file.data);
            }
          });
        }
      });
    }
  }
}
