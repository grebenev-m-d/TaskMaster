import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Output,
  Renderer2,
  ViewChild,
} from '@angular/core';
import {
  CdkDrag,
  CdkDragEnd,
  CdkDragMove,
  CdkDragPlaceholder,
  CdkDragStart,
} from '@angular/cdk/drag-drop';
import { RouterLink } from '@angular/router';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { EditCardComponent } from '../../modal/edit-card/edit-card.component';
import { FormsModule } from '@angular/forms';
import { Card } from '../../models/Card';
import { CardHubService } from '../../services/hub.services/cardHub.service';
import { AccessLevelType } from '../../models/enum/AccessLevelType';
import { CardApiService } from '../../services/api.services/cardApi.service';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
    CdkDrag,
    CdkDragPlaceholder,
    RouterLink,
    EditCardComponent,
  ],
  templateUrl: './card.component.html',
  styleUrl: './card.component.css',
})
export class CardComponent implements OnInit, OnDestroy, AfterViewInit {
  // Карточка
  @Input() card: Card;

  // Сервис хаба карточки
  @Input() cardHubService: CardHubService;

  // Флаг редактирования карточки
  isEditing = false;

  // Индекс карточки
  @Input() index: number;

  // Флаг перетаскивания и изменения списка
  @Input() dragAndDropList: boolean;

  // Текущий уровень доступа
  @Input() currentAccessLevel: AccessLevelType;

  // Событие изменения перетаскивания и списка
  @Output() dragAndDropListChange = new EventEmitter<boolean>();

  // Высота заполнителя
  placeholderHeight: number;

  constructor(
    private cardApiService: CardApiService,
    public dialog: MatDialog
  ) {}

  // Инициализация компонента
  async ngOnInit() {
    await this.initializeCardState();
    await this.registerCardEventHandlers();
  }

  // Уничтожение компонента
  ngOnDestroy() {}

  // Удаление карточки
  deleteCard() {
    this.cardHubService.deleteCard(this.card.id);
  }

  // Включение редактора карточки
  enableCardEditor(event: MouseEvent) {
    event.preventDefault();
    this.isEditing = true;
    this.dragAndDropListChange.emit(false);
    document.addEventListener('click', this.handleClickOutside);
  }

  // Отключение редактора карточки
  disableCardEditor() {
    this.isEditing = false;
    this.dragAndDropListChange.emit(true);
    document.removeEventListener('click', this.handleClickOutside);
  }

  // Обработчик клика вне элемента карточки
  @ViewChild('cardContainer') cardElement: ElementRef;
  handleClickOutside = (event: MouseEvent) => {
    const clickedElement = event.target as HTMLElement;

    if (!this.cardElement.nativeElement.contains(clickedElement)) {
      this.disableCardEditor();
    }
  };

  // Открытие модального окна редактирования карточки
  openModalPopUpEditCard() {
    const config = new MatDialogConfig();
    config.data = {
      card: this.card,
      cardHubService: this.cardHubService,
      panelClass: 'custom-dialog-cont',
    };
    const dialogRef = this.dialog.open(EditCardComponent, config);

    dialogRef.afterClosed().subscribe((result) => {
      console.log(`Dialog result: ${result}`);
    });
  }

  // Начало перетаскивания карточки
  dragStart(card: any) {
    this.placeholderHeight = this.cardElement.nativeElement.offsetHeight;
  }

  // Окончание перетаскивания карточки
  dragEnd(card: any) {}

  // После инициализации представления
  ngAfterViewInit() {
    this.placeholderHeight = this.cardElement.nativeElement.offsetHeight;
  }

  // Обновление заголовка карточки
  updateTitle() {
    this.cardHubService.updateTitle(this.card.id, this.card.title);
  }

  // Инициализация состояния карточки
  async initializeCardState() {
    if (this.card) {
      if (this.card.hasCoverImage) {
        this.cardApiService.getCoverImage(this.card.id).then((file) => {
          if (file) {
            this.card.imageUrl = window.URL.createObjectURL(file.data);
          }
        });
      }
    }
  }

  // Регистрация обработчиков событий карточки
  async registerCardEventHandlers() {
    // Обновление заголовка карточки
    await this.cardHubService.onCardTitleUpdated((cardId, title) => {
      if (this.card.id == cardId) {
        this.card.title = title;
      }
    });

    // Добавление обложки карточки
    await this.cardHubService.onCardCoverImageAdded((cardId) => {
      if (this.card.id === cardId) {
        this.cardApiService.getCoverImage(cardId).then((file) => {
          if (file) {
            this.card.hasCoverImage = true;
            this.card.imageUrl = window.URL.createObjectURL(file.data);
          }
        });
      }
    });

    // Удаление обложки карточки
    await this.cardHubService.onCardCoverImageDeleted((cardId) => {
      if (this.card.id === cardId) {
        this.card.hasCoverImage = false;
        this.card.imageUrl = null;
      }
    });

    // Обновление описания карточки
    await this.cardHubService.onCardDescriptionUpdated(
      (cardId, description) => {
        if (this.card.id == cardId) {
          this.card.description = description;
        }
      }
    );
  }
}
