import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../../environments/environment.development';
import { Card } from '../../models/Card';
import { CardAttachment } from '../../models/CardAttachment';

export class CardHubService {
  // Приватные свойства для хранения соединения с хабом и URL хаба.
  private hubConnection: HubConnection;
  private hubUrl: string = environment.hubUrl + '/card';

  // Конструктор сервиса. Принимает идентификатор доски и/или идентификатор карточки (необязательно).
  constructor(boardId: string = null, cardId: string = null) {
    // Формирование URL хаба с учетом идентификатора доски или карточки, если они предоставлены.
    let url = this.hubUrl;
    if (boardId != null) {
      url += `/?board_id=${boardId}`;
    } else if (cardId != null) {
      url += `/?card_id=${cardId}`;
    } else {
      // Возвращаем, если не предоставлены ни идентификатор доски, ни идентификатор карточки.
      return;
    }

    // Получение токена доступа из локального хранилища.
    const token = localStorage.getItem('accessToken');

    // Создание соединения с хабом.
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => token,
      })
      .build();
  }

  // Метод для запуска соединения с хабом.
  startConnection() {
    return this.hubConnection.start();
  }

  // Метод для закрытия соединения с хабом.
  closeConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  // Подписка на событие создания карточки.
  onCardCreated(callback: (cardListId: string, card: Card) => void) {
    this.hubConnection.on('ReceiveCardCreated', callback);
  }

  // Подписка на событие добавления вложения к карточке.
  onCardAttachmentAdded(
    callback: (cardId: string, cardAttachment: CardAttachment) => void
  ) {
    this.hubConnection.on('ReceiveCardAttachmentAdded', callback);
  }

  // Подписка на событие удаления вложения к карточке.
  onCardAttachmentDeleted(
    callback: (cardId: string, cardAttachmentId: string) => void
  ) {
    this.hubConnection.on('ReceiveCardAttachmentDeleted', callback);
  }

  // Подписка на событие обновления названия карточки.
  onCardTitleUpdated(callback: (cardId: string, title: string) => void) {
    this.hubConnection.on('ReceiveCardTitleUpdated', callback);
  }

  // Подписка на событие обновления описания карточки.
  onCardDescriptionUpdated(
    callback: (cardId: string, description: string) => void
  ) {
    this.hubConnection.on('ReceiveCardDescriptionUpdated', callback);
  }

  // Подписка на событие перемещения карточки.
  onCardMoved(
    callback: (
      boardId: string,
      currentCardListId: string,
      movedCardId: string,
      prevCardId: string | null
    ) => void
  ) {
    this.hubConnection.on('ReceiveCardMoved', callback);
  }

  // Подписка на событие удаления карточки.
  onCardDeleted(callback: (cardId: string) => void) {
    this.hubConnection.on('ReceiveCardDeleted', callback);
  }

  // Метод для создания новой карточки.
  async createCard(cardListId: string, title: string) {
    await this.hubConnection.invoke('CreateCard', cardListId, title);
  }

  // Подписка на событие добавления обложки к карточке.
  onCardCoverImageAdded(callback: (cardId: string) => void) {
    this.hubConnection.on('ReceiveCardCoverImageAdded', callback);
  }

  // Подписка на событие удаления обложки к карточке.
  onCardCoverImageDeleted(callback: (cardId: string) => void) {
    this.hubConnection.on('ReceiveCardCoverImageDeleted', callback);
  }

  // Метод для получения карточки по ее идентификатору.
  getCard(cardId: string): Promise<Card> {
    return this.hubConnection
      .invoke('GetCard', cardId)
      .then((card: Card) => {
        return card;
      })
      .catch((err) => {
        console.error('Error while fetching boards: ' + err);
        return null;
      });
  }

  // Метод для получения списка карточек по идентификатору списка.
  getCards(cardListId: string): Promise<Card[]> {
    return this.hubConnection
      .invoke('GetCards', cardListId)
      .then((cards: Card[]) => {
        return cards;
      })
      .catch((err) => {
        console.error('Error while fetching boards: ' + err);
        return [];
      });
  }

  // Метод для обновления названия карточки.
  async updateTitle(cardId: string, title: string): Promise<void> {
    await this.hubConnection.invoke('UpdateCardTitle', cardId, title);
  }

  // Метод для обновления описания карточки.
  async updateDescription(cardId: string, description: string): Promise<void> {
    await this.hubConnection.invoke(
      'UpdateCardDescription',
      cardId,
      description
    );
  }

  // Метод для перемещения карточки на доске.
  async moveCard(
    boardId: string,
    currentCardListId: string,
    movedCardId: string,
    prevCardId: string | null
  ): Promise<void> {
    await this.hubConnection.invoke(
      'MoveCard',
      boardId,
      currentCardListId,
      movedCardId,
      prevCardId
    );
  }

  // Метод для удаления обложки карточки.
  async deleteCoverImage(cardId: string): Promise<void> {
    await this.hubConnection.invoke('DeleteCardCoverImage', cardId);
  }

  // Метод для удаления вложения из карточки.
  async deleteAttachment(cardId: string, fileId: string): Promise<void> {
    await this.hubConnection.invoke('DeleteCardAttachment', cardId, fileId);
  }

  // Метод для удаления карточки.
  async deleteCard(cardId: string): Promise<void> {
    await this.hubConnection.invoke('DeleteCard', cardId);
  }
}
