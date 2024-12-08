import { User } from './User';
import { AccessLevelType } from './enum/AccessLevelType';

// Уровень доступа пользователя
export class UserAccessLevel {
  // Пользователь
  user: User;
  // Уровень доступа
  accessLevel: AccessLevelType;
}
