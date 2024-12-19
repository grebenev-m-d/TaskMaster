import { Board } from './Board';
import { RoleType } from './enum/RoleType';

// Пользователь
export class User {
  // Идентификатор пользователя
  id: string;
  // Имя пользователя
  name: string;
  // Электронная почта пользователя
  email: string;
  // Дата создания пользователя
  createdAt?: Date;
  // Дата последнего входа пользователя
  lastLogin?: Date;
  // Роль пользователя
  role: RoleType;
  // Список собственных досок пользователя
  ownBoards: Board[];
}
