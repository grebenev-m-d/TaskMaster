import { UserAccessLevel } from './UserAccessLevel';
import { AccessLevelType } from './enum/AccessLevelType';

// Объект уровня доступа к доске
export class BoardAccessLevel {
  // Флаг публичного доступа к доске
  isPublic: boolean;
  // Уровни доступа пользователей к доске
  personalAccessLevels: UserAccessLevel[] = [];
  // Тип уровня доступа по умолчанию
  defaultAccessLevelType: AccessLevelType;

  // Метод для присвоения значений из частичного уровня доступа к доске
  assign(boardAccessLevel: Partial<BoardAccessLevel>) {
    this.isPublic = boardAccessLevel?.isPublic ?? this.isPublic;
    this.personalAccessLevels =
      boardAccessLevel?.personalAccessLevels ?? this.personalAccessLevels;
    this.defaultAccessLevelType =
      boardAccessLevel?.defaultAccessLevelType ?? this.defaultAccessLevelType;
  }
}
