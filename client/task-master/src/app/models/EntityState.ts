// Сущность
export class EntityState<T> {
  // Объект сущности
  entity: T;
  // Флаг выбранности
  selected: boolean = false;
  // Флаг редактирования
  isEditing: boolean = false;

  // Конструктор сущности
  constructor(entity?: T) {
    this.entity = entity;
  }

  // Метод для оборачивания списка сущностей
  static wrapList<T>(entityList: T[]) {
    return entityList.map((i) => new EntityState<T>(i));
  }
}
