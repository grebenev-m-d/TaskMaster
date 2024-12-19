import { Card } from './Card';

// Список карточек
export class CardList {
  // Идентификатор списка
  id: string;
  // Название списка
  title: string;
  // Карточки в списке
  cards: Card[] = [];

  // Конструктор списка карточек
  constructor(cardList?: CardList) {
    // Если передан исходный список карточек
    if (cardList != undefined) {
      // Присвоение значений из исходного списка
      this.id = cardList?.id;
      this.title = cardList?.title;
      this.cards = cardList?.cards || this.cards;
    }
  }

  // Метод для присвоения значений из частичного списка карточек
  assign(cardList: Partial<CardList>) {
    this.id = cardList?.id ?? this.id;
    this.title = cardList?.title ?? this.title;
    this.cards = cardList?.cards ?? this.cards;
  }
}
