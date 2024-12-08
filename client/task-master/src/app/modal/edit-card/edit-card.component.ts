import { CommonModule } from '@angular/common';
import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  Inject,
  OnDestroy,
} from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import {
  MatSidenavContainer,
  MatSidenavModule,
} from '@angular/material/sidenav';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { CardDescriptionComponent } from './card-description/card-description.component';
import { CardAttachmentsComponent } from './card-attachment/card-attachment.component';
import { CardCommentsComponent } from './card-comments/card-comments.component';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogRef,
} from '@angular/material/dialog';
import { CardHubService } from '../../services/hub.services/cardHub.service';
import { Card } from '../../models/Card';
import { CardApiService } from '../../services/api.services/cardApi.service';
import { showError } from '../../helpers/exceptionMessageHelper';

@Component({
  selector: 'app-edit-card',
  standalone: true,
  imports: [
    CommonModule,

    CardDescriptionComponent,
    CardAttachmentsComponent,
    CardCommentsComponent,

    FormsModule,
    ReactiveFormsModule,

    MatSidenavModule,
    MatButtonModule,
    MatRadioModule,
    MatSidenavContainer,
    MatToolbarModule,
    MatSidenavModule,
  ],
  templateUrl: './edit-card.component.html',
  styleUrl: './edit-card.component.css',
})
export class EditCardComponent implements OnInit, OnDestroy {
  // Флаги для отображения различных частей карточки
  showDescription: boolean = true;
  showTags: boolean = false;
  showTime: boolean = false;
  showAttachments: boolean = true;
  showChecklist: boolean = false;
  showComments: boolean = true;

  // Объект карточки и сервис хаба карточек
  card: Card = new Card();
  cardHubService: CardHubService;
  cardId: string;

  // Конструктор с инъекцией сервисов и данных диалогового окна
  constructor(
    private cardApiService: CardApiService,
    public dialog: MatDialog,
    private dialogRef: MatDialogRef<EditCardComponent>,
    @Inject(MAT_DIALOG_DATA) private data: any
  ) {
    this.cardId = data.card.id;
  }

  // Инициализация компонента
  async ngOnInit() {
    this.cardHubService = new CardHubService(null, this.cardId);
    this.cardHubService
      .startConnection()
      .then(async () => {
        this.initializeCardState();
        this.registerCardEventHandlers();
      })
      .catch((e) => {
        showError(this.dialog, e);
      });
  }

  // Уничтожение компонента
  ngOnDestroy() {
    this.cardHubService.closeConnection();
  }

  // Инициализация состояния карточки
  async initializeCardState() {
    let card;
    await this.cardHubService
      .getCard(this.cardId)
      .then((c) => {
        card = c;
      })
      .catch((e) => {
        showError(this.dialog, e);
      });

    if (card) {
      if (card.hasCoverImage) {
        let file = await this.cardApiService.getCoverImage(card.id);
        if (file) {
          card.imageUrl = window.URL.createObjectURL(file.data);
        }
      }
    }

    this.card = card;
  }

  // Регистрация обработчиков событий карточки
  async registerCardEventHandlers() {
    await this.cardHubService.onCardTitleUpdated((cardId, title) => {
      this.card.title = title;
    });
    await this.cardHubService.onCardDeleted((cardId) => {
      this.dialogRef.close();
    });
    await this.cardHubService.onCardCoverImageDeleted((cardId) => {
      if (this.card.id === cardId) {
        this.card.hasCoverImage = false;
        this.card.imageUrl = null;
      }
    });
    await this.cardHubService.onCardCoverImageAdded((cardId) => {
      if (this.card.id === cardId) {
        this.cardApiService.getCoverImage(cardId).then((file) => {
          if (file) {
            this.card.hasCoverImage = true;
            this.card.imageUrl = window.URL.createObjectURL(file.data);
          }
        });
      }
    });
    await this.cardHubService.onCardCoverImageDeleted((cardId) => {
      this.card.imageUrl = null;
      this.card.hasCoverImage = false;
    });
  }

  // Добавление обложки карточки
  async addCoverImage(event: Event) {
    let selectedFile = (event.target as HTMLInputElement).files[0];
    if (selectedFile) {
      await this.cardApiService
        .addCoverImage(this.card.id, selectedFile)
        .catch((e) => {
          showError(this.dialog, e);
        });

      (event.target as HTMLInputElement).files = null;
    }
  }

  // Удаление обложки карточки
  async deleteCoverImage() {
    if (this.card.imageUrl) {
      await this.cardHubService.deleteCoverImage(this.card.id).catch((e) => {
        showError(this.dialog, e);
      });
    }
  }

  // Обновление заголовка карточки
  async updateTitle() {
    await this.cardHubService.updateTitle(this.card.id, this.card.title);
  }
}
