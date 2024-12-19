import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../../environments/environment.development';
import { CardComment } from '../../models/CardComment';

export class CardCommentHubService {
  // Приватные свойства для хранения соединения с хабом и URL хаба.
  private hubConnection: HubConnection;
  private hubUrl: string = environment.hubUrl + '/card-comment';

  // Конструктор сервиса. Принимает идентификатор карточки и/или идентификатор комментария (необязательно).
  constructor(cardId: string) {
    // Формирование URL хаба с учетом идентификатора карточки или комментария, если они предоставлены.
    let url = this.hubUrl;
    if (cardId != null) {
      url += `/?card_id=${cardId}`;
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

  // Подписка на событие создания комментария к карточке.
  onCardCommentCreated(
    callback: (cardId: string, comment: CardComment) => void
  ) {
    this.hubConnection.on('ReceiveCardCommentCreated', callback);
  }

  // Подписка на событие обновления текста комментария.
  onTextUpdated(callback: (cardCommentId: string, text: string) => void) {
    this.hubConnection.on('ReceiveTextUpdated', callback);
  }

  // Подписка на событие удаления комментария к карточке.
  onCardCommentDeleted(callback: (cardCommentId: string) => void) {
    this.hubConnection.on('ReceiveCardCommentDeleted', callback);
  }

  // Метод для создания комментария к карточке.
  async createCardComment(cardId: string, text: string) {
    await this.hubConnection.invoke('CreateCardComment', cardId, text);
  }

  // Метод для получения комментариев к карточке.
  getCardComments(
    cardId: string,
    pageNumber: number,
    pageSize: number
  ): Promise<CardComment[]> {
    return this.hubConnection
      .invoke('GetCardComments', cardId, pageNumber, pageSize)
      .then((cardComments: CardComment[]) => {
        return cardComments;
      })
      .catch((err) => {
        console.error('Error while fetching boards: ' + err);
        return [];
      });
  }

  // Метод для получения количества комментариев к карточке.
  async getCardCommentsCount(cardId: string): Promise<number> {
    return this.hubConnection
      .invoke('GetCardCommentsCount', cardId)
      .then((count: number) => {
        return count;
      })
      .catch((err) => {
        console.error('Error while fetching boards: ' + err);
        return 0;
      });
  }

  // Метод для обновления текста комментария к карточке.
  async updateText(cardCommentId: string, text: string): Promise<void> {
    await this.hubConnection.invoke('UpdateText', cardCommentId, text);
  }

  // Метод для удаления комментария к карточке.
  async deleteCardComment(cardCommentId: string): Promise<void> {
    await this.hubConnection.invoke('DeleteCardComment', cardCommentId);
  }
}
