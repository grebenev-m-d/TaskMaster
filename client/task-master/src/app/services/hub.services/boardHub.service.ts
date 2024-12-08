import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Board } from '../../models/Board';
import { environment } from '../../../environments/environment.development';
import { DesignType } from '../../models/enum/DesignType';
import { User } from '../../models/User';

export class BoardHubService {
  // Приватные свойства для хранения соединения с хабом и URL хаба.
  private hubConnection: HubConnection;
  private hubUrl: string = environment.hubUrl + '/board';

  // Конструктор сервиса. Принимает идентификатор доски (необязательно).
  constructor(boardId: string = null) {
    // Формирование URL хаба с учетом идентификатора доски, если он предоставлен.
    let url = this.hubUrl;
    if (boardId != null) {
      url += `/?board_id=${boardId}`;
    }

    // Получение токена доступа из локального хранилища.
    const token = localStorage.getItem('accessToken');

    // Создание соединения с хабом.
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => token,
      })
      .build();

    // Установка интервала отправки пинговых запросов для поддержания соединения.
    this.hubConnection.keepAliveIntervalInMilliseconds = 102400000;
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

  // Подписка на событие обновления истории последних просмотренных досок.
  onUpdatedHistoryLastViewedBoards(callback: (boards: Board[]) => void) {
    this.hubConnection.on('ReceiveUpdatedHistoryLastViewedBoards', callback);
  }

  // Подписка на событие создания новой доски.
  onBoardCreated(callback: (board: Board) => void) {
    this.hubConnection.on('ReceiveBoardCreated', callback);
  }

  // Подписка на событие обновления названия доски.
  onTitleUpdated(callback: (boardId: string, title: string) => void) {
    this.hubConnection.on('ReceiveTitleUpdated', callback);
  }

  // Подписка на событие обновления цвета доски.
  onColorUpdated(callback: (boardId: string, colorCode: string) => void) {
    this.hubConnection.on('ReceiveColorUpdated', callback);
  }

  // Подписка на событие обновления изображения доски.
  onImageUpdated(callback: (boardId: string) => void) {
    this.hubConnection.on('ReceiveImageUpdated', callback);
  }

  // Подписка на событие обновления типа дизайна доски.
  onDesignTypeUpdated(
    callback: (boardId: string, designType: DesignType) => void
  ) {
    this.hubConnection.on('ReceiveDesignTypeUpdated', callback);
  }

  // Подписка на событие обновления владельца доски.
  onOwnerUpdated(callback: (boardId: string, owner: User) => void) {
    this.hubConnection.on('ReceiveOwnerUpdated', callback);
  }

  // Подписка на событие удаления доски.
  onBoardDeleted(callback: (boardId: string) => void) {
    this.hubConnection.on('ReceiveBoardDeleted', callback);
  }

  // Метод для создания новой доски.
  createBoard(title: string) {
    return this.hubConnection.invoke('CreateBoard', title);
  }

  // Метод для получения пользователей, просмотревших доску.
  getUsersWhoViewBoard(boardId: string): Promise<User[]> {
    return this.hubConnection.invoke('GetUsersWhoViewBoard', boardId);
  }

  // Метод для получения истории последних просмотренных досок.
  getHistoryLastViewedBoards(): Promise<Board[]> {
    return this.hubConnection.invoke('GetHistoryLastViewedBoards');
  }

  // Метод для получения информации о доске.
  getBoard(boardId: string): Promise<Board> {
    return this.hubConnection.invoke('GetBoard', boardId);
  }

  // Метод для получения собственных досок пользователя.
  getOwnBoards(pageNumber: number, pageSize: number): Promise<Board[]> {
    return this.hubConnection.invoke('GetOwnBoards', pageNumber, pageSize);
  }

  // Метод для получения общего количества собственных досок пользователя.
  getTotalNumberOwnBoards(): Promise<number> {
    return this.hubConnection.invoke('GetTotalNumberOwnBoards');
  }

  // Метод для получения пользователей с общими досками.
  getUsersWithSharedBoards(): Promise<User[]> {
    return this.hubConnection.invoke('GetUsersWithSharedBoards');
  }

  // Метод для получения общих досок с пользователем.
  getSharedBoardsWithUser(
    inviterUserId: string,
    pageNumber: number,
    pageSize: number
  ): Promise<Board[]> {
    return this.hubConnection.invoke(
      'GetSharedBoardsWithUser',
      inviterUserId,
      pageNumber,
      pageSize
    );
  }

  // Метод для получения общего количества общих досок с пользователем.
  getTotalNumberSharedBoardsWithUser(inviterUserId: string): Promise<number> {
    return this.hubConnection.invoke(
      'GetTotalNumberSharedBoardsWithUser',
      inviterUserId
    );
  }

  // Метод для обновления названия доски.
  updateTitle(boardId: string, title: string): Promise<void> {
    return this.hubConnection.invoke('UpdateTitle', boardId, title);
  }

  // Метод для обновления цвета доски.
  updateColor(boardId: string, colorCode: string): Promise<void> {
    return this.hubConnection.invoke('UpdateColor', boardId, colorCode);
  }

  // Метод для обновления типа дизайна доски.
  updateDesignType(boardId: string, designType: DesignType): Promise<void> {
    return this.hubConnection.invoke('UpdateDesignType', boardId, designType);
  }

  // Метод для обновления владельца доски.
  updateOwner(boardId: string, ownerEmail: string): Promise<void> {
    return this.hubConnection.invoke('UpdateOwner', boardId, ownerEmail);
  }

  // Метод для удаления доски.
  deleteBoard(boardId: string): Promise<void> {
    return this.hubConnection.invoke('DeleteBoard', boardId);
  }
}
