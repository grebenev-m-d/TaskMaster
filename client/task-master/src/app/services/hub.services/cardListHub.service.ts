import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../../environments/environment.development';
import { CardList } from '../../models/CardList';

export class CardListHubService {
  // Объект соединения с SignalR хабом.
  private hubConnection: HubConnection;

  // Базовый URL для соединения с хабом кардинальных списков.
  private hubUrl: string = environment.hubUrl + '/card-list';

  // Конструктор сервиса, принимает необязательные идентификаторы доски и списка карточек.
  constructor(boardId: string = null, cardListId: string = null) {
    let url = this.hubUrl;

    if (boardId != null) {
      url += `/?board_id=${boardId}`;
    } else if (cardListId != null) {
      url += `/?card_list_id=${cardListId}`;
    } else {
      return;
    }
    // Получаем токен доступа из локального хранилища.
    const token = localStorage.getItem('accessToken');

    // Создаем соединение с хабом, используя URL и токен доступа.
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => token, // Фабрика для получения токена доступа.
      })
      .build();
  }

  // Запускаем соединение с хабом.
  startConnection() {
    return this.hubConnection.start();
  }

  // Закрываем соединение с хабом.
  closeConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  // Обработка события создания нового списка карточек.
  // Вызывается, когда создается новый список карточек на доске.
  onCardListCreated(callback: (boardId: string, cardList: CardList) => void) {
    this.hubConnection.on('ReceiveCardListCreated', callback);
  }

  // Обработка события обновления заголовка списка карточек.
  // Вызывается, когда заголовок списка карточек изменяется.
  onCardListTitleUpdated(
    callback: (cardListId: string, title: string) => void
  ) {
    this.hubConnection.on('ReceiveCardListTitleUpdated', callback);
  }

  // Обработка события перемещения списка карточек.
  // Вызывается, когда список карточек перемещается на доске.
  onCardListMoved(
    callback: (movedCardListId: string, prevCardListId: string | null) => void
  ) {
    this.hubConnection.on('ReceiveCardListMoved', callback);
  }

  // Обработка события удаления списка карточек.
  // Вызывается, когда список карточек удаляется с доски.
  onCardListDeleted(callback: (cardListId: string) => void) {
    this.hubConnection.on('ReceiveCardListDeleted', callback);
  }

  // Создание нового списка карточек на доске с заданным заголовком.
  async createCardList(boardId: string, title: string) {
    await this.hubConnection.invoke('CreateCardList', boardId, title);
  }

  // Получение всех списков карточек на доске по идентификатору доски.
  async getCardLists(boardId: string): Promise<CardList[]> {
    return await this.hubConnection.invoke('GetCardLists', boardId);
  }

  // Обновление заголовка списка карточек по его идентификатору.
  async updateTitle(cardListId: string, title: string): Promise<void> {
    await this.hubConnection.invoke('UpdateTitle', cardListId, title);
  }

  // Перемещение списка карточек на новое место на доске.
  async moveCardList(
    movedCardListId: string,
    prevCardListId: string | null
  ): Promise<void> {
    await this.hubConnection.invoke(
      'MoveCardList',
      movedCardListId,
      prevCardListId
    );
  }

  // Удаление списка карточек с доски по его идентификатору.
  async deleteCardList(cardListId: string): Promise<void> {
    await this.hubConnection.invoke('DeleteCardList', cardListId);
  }
}
