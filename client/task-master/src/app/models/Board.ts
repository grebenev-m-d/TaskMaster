import { CardList } from './CardList';
import { User } from './User';
import { DesignType } from './enum/DesignType';

export class Board {
  // Идентификатор доски
  id: string;
  // Название доски
  title?: string;
  // Дата создания доски
  createdAt?: Date;
  // Дата последнего обновления доски
  lastUpdated?: Date;
  // Код цвета доски
  colorCode?: string;
  // URL изображения доски
  imageUrl?: string;
  // Тип дизайна доски
  designType: DesignType;
  // Владелец доски
  owner?: User;
  // Списки карточек на доске
  cardLists: CardList[] = [];

  // Конструктор объекта доски
  constructor(board?: Board) {
    // Если передана исходная доска
    if (board != undefined) {
      // Присвоение значений из исходной доски
      this.id = board?.id;
      this.title = board?.title;
      this.createdAt = board?.createdAt;
      this.lastUpdated = board?.lastUpdated;
      this.colorCode = board?.colorCode;
      this.designType = board?.designType;
      this.owner = board?.owner;
      this.cardLists = board?.cardLists ?? this.cardLists;
    }
  }

  // Метод для присвоения значений из частичной доски
  assign(board: Partial<Board>) {
    this.id = board?.id ?? this.id;
    this.title = board?.title ?? this.title;
    this.createdAt = board?.createdAt ?? this.createdAt;
    this.lastUpdated = board?.lastUpdated ?? this.lastUpdated;
    this.colorCode = board?.colorCode ?? this.colorCode;
    this.imageUrl = board?.imageUrl ?? this.imageUrl;
    this.designType = board?.designType ?? this.designType;
    this.owner = board?.owner ?? this.owner;
    this.cardLists = board?.cardLists ?? this.cardLists;
  }
}
