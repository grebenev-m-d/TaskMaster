import { HttpClient, HttpEventType } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { CardApiService } from '../../../services/api.services/cardApi.service';
import { CardAttachment } from '../../../models/CardAttachment';
import { Card } from '../../../models/Card';
import { CardHubService } from '../../../services/hub.services/cardHub.service';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { showError } from '../../../helpers/exceptionMessageHelper';
import { downloadFile } from '../../../helpers/fileHelper';

@Component({
  selector: 'app-card-attachment',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card-attachment.component.html',
  styleUrl: './card-attachment.component.css',
})
export class CardAttachmentsComponent implements OnInit {
  // Входные данные: объект карточки и сервис хаба карточек
  @Input() card: Card;
  @Input() cardHubService: CardHubService;

  // Конструктор с инъекцией сервисов: API карточек и диалогового окна
  constructor(
    private cardApiService: CardApiService,
    public dialog: MatDialog
  ) {}

  // Прогресс загрузки файлов (по умолчанию 0)
  progress: number = 0;
  // Флаг для отслеживания показа полной информации (по умолчанию false)
  hasShownFull: boolean = false;

  // Инициализация компонента
  async ngOnInit() {
    await this.registerCardEventHandlers();
  }

  // Регистрация обработчиков событий карточки
  async registerCardEventHandlers() {
    // Обработчик добавления вложения карточки
    await this.cardHubService.onCardAttachmentAdded(
      (cardId, cardAttachment) => {
        this.card.attachments.push(cardAttachment);
      }
    );
    // Обработчик удаления вложения карточки
    await this.cardHubService.onCardAttachmentDeleted(
      (cardId, cardAttachmentId) => {
        this.card.attachments = this.card.attachments.filter(
          (i) => i.id !== cardAttachmentId
        );
      }
    );
  }

  // Метод для загрузки файлов в карточку
  upload(event: Event) {
    let selectedFileList = (event.target as HTMLInputElement).files;
    if (selectedFileList) {
      for (let index = 0; index < selectedFileList.length; index++) {
        // Вызов метода API для добавления вложения карточки
        this.cardApiService
          .addAttachmentFile(this.card.id, selectedFileList[index])
          .catch((e) => {
            // Вывод ошибки при неудачной загрузке файла
            showError(this.dialog, e.error);
          });
      }
    }
  }

  // Метод для загрузки вложения карточки
  async downloadAttachment(attachmentId: string) {
    let file = await this.cardApiService
      // Вызов метода API для получения файла вложения карточки
      .getAttachmentFile(this.card.id, attachmentId)
      .then((f) => {
        // Скачивание файла
        downloadFile(f);
      })
      .catch((e) => {
        // Вывод ошибки при неудачной загрузке файла
        showError(this.dialog, e.error);
      });
  }

  // Метод для удаления вложения карточки
  deleteAttachment(attachmentId: string): void {
    // Вызов метода сервиса хаба карточек для удаления вложения
    this.cardHubService
      .deleteAttachment(this.card.id, attachmentId)
      .catch((e) => {
        // Вывод ошибки при неудачном удалении вложения
        showError(this.dialog, e.error);
      });
  }
}
