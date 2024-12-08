import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from '../../../environments/environment.development';
import { AccessLevelType } from '../../models/enum/AccessLevelType';
import { BoardAccessLevel } from '../../models/BoardAccessLevel';
import { User } from '../../models/User';

// Сервис для управления доступом к доскам через хаб сигналов.
export class BoardAccessLevelHubService {
  // Приватные свойства для хранения соединения с хабом и URL хаба.
  private hubConnection: HubConnection;
  private hubUrl: string = environment.hubUrl + '/board-permission';

  // Конструктор сервиса. Принимает необязательный параметр - идентификатор доски.
  constructor(boardId?: string) {
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
  }

  // Метод для запуска соединения с хабом.
  startConnection() {
    return this.hubConnection.start().catch((error) => {});
  }

  // Метод для закрытия соединения с хабом.
  closeConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  // Подписка на событие обновления публичного статуса доски.
  onBoardPublicStatusUpdated(
    callback: (boardId: string, isPublic: boolean) => void
  ) {
    this.hubConnection.on('ReceiveBoardPublicStatusUpdated', callback);
  }

  // Подписка на событие обновления общего уровня доступа к доске.
  onBoardDefaultAccessLevelUpdated(
    callback: (boardId: string, permission: AccessLevelType) => void
  ) {
    this.hubConnection.on('ReceiveBoardDefaultAccessLevelUpdated', callback);
  }

  // Подписка на событие добавления персонального уровня доступа.
  onPersonalAccessLevelAdded(
    callback: (boardId: string, user: User, permission: AccessLevelType) => void
  ) {
    this.hubConnection.on('ReceivePersonalAccessLevelAdded', callback);
  }

  // Подписка на событие обновления персонального уровня доступа.
  onPersonalAccessLevelUpdated(
    callback: (
      boardId: string,
      userId: string,
      permission: AccessLevelType
    ) => void
  ) {
    this.hubConnection.on('ReceivePersonalAccessLevelUpdated', callback);
  }

  // Подписка на событие удаления персонального уровня доступа.
  onPersonalAccessLevelDeleted(
    callback: (boardId: string, userId: string) => void
  ) {
    this.hubConnection.on('ReceivePersonalAccessLevelDeleted', callback);
  }

  // Подписка на событие добавления уровня доступа для пользователя.
  onUserAccessLevelAdded(
    callback: (boardId: string, user: User, permission: AccessLevelType) => void
  ) {
    this.hubConnection.on('ReceiveUserAccessLevelAdded', callback);
  }

  // Подписка на событие обновления уровня доступа для пользователя.
  onUserAccessLevelUpdated(
    callback: (boardId: string, permission: AccessLevelType) => void
  ) {
    this.hubConnection.on('ReceiveUserAccessLevelUpdated', callback);
  }

  // Подписка на событие удаления уровня доступа для пользователя.
  onUserAccessLevelDeleted(callback: (boardId: string, user: User) => void) {
    this.hubConnection.on('ReceiveUserAccessLevelDeleted', callback);
  }

  // Метод для получения уровня доступа к доске.
  getBoardAccessLevel(boardId: string): Promise<BoardAccessLevel> {
    return this.hubConnection
      .invoke('GetBoardAccessLevel', boardId)
      .then((boardAccessLevel: BoardAccessLevel) => {
        return boardAccessLevel;
      })
      .catch((err) => {
        console.error('Error while fetching boards: ' + err);
        return null;
      });
  }

  // Метод для получения уровня доступа пользователя к доске.
  getUserAccessLevel(boardId: string): Promise<AccessLevelType> {
    return this.hubConnection
      .invoke('GetUserAccessLevel', boardId)
      .then((permissionType: AccessLevelType) => {
        return permissionType;
      })
      .catch((err) => {
        console.error('Error while fetching boards: ' + err);
        return null;
      });
  }

  // Метод для обновления публичного статуса доски.
  async updateBoardPublicStatus(
    boardId: string,
    isPublic: boolean
  ): Promise<void> {
    await this.hubConnection.invoke(
      'UpdateBoardPublicStatus',
      boardId,
      isPublic
    );
  }

  // Метод для обновления общего уровня доступа к доске.
  async updateBoardGeneralAccessLevel(
    boardId: string,
    permissionType: AccessLevelType
  ): Promise<void> {
    await this.hubConnection.invoke(
      'UpdateBoardGeneralAccessLevel',
      boardId,
      permissionType
    );
  }

  // Метод для добавления персонального уровня доступа.
  async addPersonalAccessLevel(
    boardId: string,
    userEmail: string,
    permissionType: AccessLevelType
  ): Promise<void> {
    await this.hubConnection.invoke(
      'AddPersonalAccessLevel',
      boardId,
      userEmail,
      permissionType
    );
  }

  // Метод для обновления персонального уровня доступа.
  async updatePersonalAccessLevel(
    boardId: string,
    userId: string,
    permissionType: AccessLevelType
  ): Promise<void> {
    await this.hubConnection.invoke(
      'UpdatePersonalAccessLevel',
      boardId,
      userId,
      permissionType
    );
  }

  // Метод для удаления персонального уровня доступа.
  async deletePersonalAccessLevel(
    boardId: string,
    userId: string
  ): Promise<void> {
    await this.hubConnection.invoke(
      'DeletePersonalAccessLevel',
      boardId,
      userId
    );
  }
}
