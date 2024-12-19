import { User } from './User';

// Комментарий к карточке
export class CardComment {
  // Идентификатор комментария
  id: string;
  // Текст комментария
  text: string;
  // Пользователь, оставивший комментарий
  user: User;
  // Дата создания комментария
  createdAt: Date;
  // Дата последнего обновления комментария
  updatedAt: Date;
}
