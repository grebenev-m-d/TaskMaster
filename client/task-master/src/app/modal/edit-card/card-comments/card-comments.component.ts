import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { CardComment } from '../../../models/CardComment';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardCommentHubService } from '../../../services/hub.services/cardCommentHub.service';
import { EntityState } from '../../../models/EntityState';

import { MatDialog } from '@angular/material/dialog';
import { getUserIdFromJwt } from '../../../helpers/jwtServiceHelper';
import { showError } from '../../../helpers/exceptionMessageHelper';

@Component({
  selector: 'app-card-comments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './card-comments.component.html',
  styleUrl: './card-comments.component.css',
})
export class CardCommentsComponent implements OnInit, OnDestroy {
  // Идентификатор пользователя
  userId: string;

  // Состояния комментариев к карточке
  cardCommentStates: EntityState<CardComment>[] = [];
  // Размер страницы комментариев
  pageSize: number = 5;
  // Номер текущей страницы комментариев
  pageNumber: number = 1;
  // Общее количество страниц комментариев
  totalNumberPages: number;

  // Флаг для редактирования комментария
  isEditing: boolean = false;

  // Конструктор с инъекцией диалогового окна
  constructor(public dialog: MatDialog) {}

  // Сервис хаба комментариев карточки
  cardCommentHubService: CardCommentHubService;
  // Текст комментария
  commentText: string = '';
  // Флаг для отслеживания показа полной информации
  hasShownFull: boolean = true;

  // Входной параметр: идентификатор карточки
  @Input() cardId: string;

  // Метод для форматирования даты
  formatDate(date: Date): string {
    return `${new Date(date).getDate()}.${new Date(date).getMonth()}.${new Date(
      date
    ).getFullYear()} 
    / ${new Date(date).getHours()}:${new Date(date).getMinutes()}`;
  }

  // Инициализация компонента
  async ngOnInit() {
    let accessToken = localStorage.getItem('accessToken');
    this.userId = getUserIdFromJwt(accessToken);

    this.cardCommentHubService = new CardCommentHubService(this.cardId);
    await this.cardCommentHubService
      .startConnection()
      .then(async () => {
        this.registerCardCommentEventHandlers();
        this.initializeCardCommentState();
      })
      .catch((e) => {
        showError(this.dialog, e);
      });
  }

  // Регистрация обработчиков событий комментариев карточки
  async registerCardCommentEventHandlers() {
    await this.cardCommentHubService
      .getCardCommentsCount(this.cardId)
      .then((totalNumberPages) => {
        this.totalNumberPages = totalNumberPages;
      })
      .catch((e) => {
        showError(this.dialog, e);
      });

    if (this.totalNumberPages !== 0) {
      this.cardCommentStates = EntityState.wrapList(
        await this.cardCommentHubService.getCardComments(
          this.cardId,
          this.pageNumber,
          this.pageSize
        )
      );

      this.pageNumber++;
    }
  }

  // Инициализация состояния комментариев карточки
  async initializeCardCommentState() {
    await this.cardCommentHubService.onCardCommentCreated((cardId, comment) => {
      this.cardCommentStates.push(new EntityState<CardComment>(comment));
    });
    await this.cardCommentHubService.onTextUpdated((commentId, text) => {
      this.cardCommentStates.find((i) => i.entity.id == commentId).entity.text =
        text;
    });
    await this.cardCommentHubService.onTextUpdated((commentId, text) => {
      let cardComment = this.cardCommentStates.find(
        (i) => i.entity.id == commentId
      ).entity;
      cardComment.updatedAt = new Date();
    });
    await this.cardCommentHubService.onCardCommentDeleted((commentId) => {
      this.cardCommentStates = this.cardCommentStates.filter(
        (item) => item.entity.id !== commentId
      );
    });
  }

  // Уничтожение компонента
  ngOnDestroy() {
    this.cardCommentHubService.closeConnection();
  }

  // Включение редактора комментария
  enableCommentEditor() {
    this.isEditing = true;
  }

  // Создание комментария
  async createComment() {
    await this.cardCommentHubService
      .createCardComment(this.cardId, this.commentText)
      .catch((e) => {
        showError(this.dialog, e);
      });

    this.commentText = '';
  }

  // Изменение текста комментария
  async changeText(commentState: EntityState<CardComment>) {
    await this.cardCommentHubService
      .updateText(commentState.entity.id, commentState.entity.text)
      .catch((e) => {
        showError(this.dialog, e);
      });
    commentState.isEditing = false;
  }

  // Удаление комментария
  async deleteComment(commentState: EntityState<CardComment>) {
    await this.cardCommentHubService
      .deleteCardComment(commentState.entity.id)
      .catch((e) => {
        showError(this.dialog, e);
      });
    commentState.isEditing = false;
  }
}
