import { CommonModule } from '@angular/common';
import {
  Component,
  Output,
  EventEmitter,
  Input,
  OnInit,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { NgxEditorModule } from 'ngx-editor';
import { EditorComponent } from '../../../components/editor/editor.component';
import { CardHubService } from '../../../services/hub.services/cardHub.service';
import { Card } from '../../../models/Card';
import { MatDialog } from '@angular/material/dialog';
import { showError } from '../../../helpers/exceptionMessageHelper';

@Component({
  selector: 'app-card-description',
  standalone: true,
  imports: [CommonModule, NgxEditorModule, EditorComponent],
  templateUrl: './card-description.component.html',
  styleUrl: './card-description.component.css',
})
// Компонент для описания карточки
export class CardDescriptionComponent {
  // Входные данные: объект карточки и сервис хаба карточек
  @Input() card: Card;
  @Input() cardHubService: CardHubService;
  // Промежуточное описание
  @Input() middleDescription: string = '';
  // Ссылка на дочерний компонент редактора
  @ViewChild('editorComponent') childComponent: EditorComponent;
  // Флаг для редактирования
  isEditing: boolean = false;

  // Конструктор с инъекцией диалогового окна
  constructor(public dialog: MatDialog) {}

  // Инициализация компонента
  async ngOnInit() {
    await this.registerCardEventHandlers();
  }

  // Регистрация обработчиков событий карточки
  async registerCardEventHandlers() {
    await this.cardHubService.onCardDescriptionUpdated(
      (cardId, description) => {
        this.card.description = description;
      }
    );
  }

  // Начало редактирования описания
  startEdit() {
    this.isEditing = true;
  }

  // Обновление описания
  updateDescription() {
    this.card.description = this.childComponent.html;
    this.cardHubService
      .updateDescription(this.card.id, this.card.description)
      .catch((e) => {
        showError(this.dialog, e);
      });
    this.isEditing = false;
  }

  // Отмена изменений
  cancelChanges() {
    this.childComponent.html = this.card.description;
    this.isEditing = false;
  }
}
