import { CardAttachment } from './CardAttachment';
import { CardComment } from './CardComment';

// Объект карточки
export class Card {
  // Идентификатор карточки
  id: string;
  // Флаг наличия обложки карточки
  hasCoverImage: boolean;
  // URL изображения обложки карточки
  imageUrl: string;
  // Заголовок карточки
  title: string = '';
  // Описание карточки
  description: string = '';
  // Вложения карточки
  attachments: CardAttachment[];
}
