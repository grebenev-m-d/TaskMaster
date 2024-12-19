import {
  Component,
  Input,
  Output,
  EventEmitter,
  Inject,
  OnInit,
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { BoardHubService } from '../../services/hub.services/boardHub.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Board } from '../../models/Board';
import { BoardApiService } from '../../services/api.services/boardApi.service';
import { DesignType } from '../../models/enum/DesignType';
import { extractExceptionMessage } from '../../helpers/exceptionMessageHelper';

@Component({
  selector: 'app-edit-board-modal',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './edit-board-modal.component.html',
  styleUrl: './edit-board-modal.component.css',
})
export class EditBoardModalComponent implements OnInit {
  board: Board; // Объект доски
  boardHubService: BoardHubService; // Сервис хаба доски
  urlImage: string; // URL изображения

  // Инициализация компонента
  async ngOnInit() {}

  // Конструктор
  constructor(
    private boardApiService: BoardApiService, // Сервис API доски
    private dialogRef: MatDialogRef<EditBoardModalComponent>, // Диалоговое окно для редактирования доски
    @Inject(MAT_DIALOG_DATA) private data: any // Переданные данные
  ) {
    this.board = data.board; // Получение объекта доски из переданных данных
    this.urlImage = data.urlImage; // Получение URL изображения из переданных данных
    this.boardHubService = data.boardHubService; // Получение сервиса хаба доски из переданных данных
  }

  selectedFile: File | null; // Выбранный файл для загрузки изображения

  // Обновление заголовка доски
  updateTitle() {
    this.boardHubService
      .updateTitle(this.board.id, this.board.title) // Вызов метода обновления заголовка доски
      .catch((e) => {}); // Обработка ошибок

    this.dialogRef.close(); // Закрытие диалогового окна
  }

  // Асинхронное обновление цвета доски
  async updateColorCode() {
    await this.boardHubService.updateColor(this.board.id, this.board.colorCode); // Обновление цвета доски
    await this.boardHubService.updateDesignType(
      this.board.id,
      DesignType.color // Обновление типа дизайна доски на основе цвета
    );
  }

  // Асинхронная загрузка изображения для доски
  async upload(event: Event) {
    let selectedFile = (event.target as HTMLInputElement).files[0]; // Получение выбранного файла
    if (selectedFile) {
      this.boardApiService.addImage(this.board.id, selectedFile).then((i) => {
        this.boardHubService.updateDesignType(this.board.id, DesignType.image); // Обновление типа дизайна доски на основе изображения
      });

      (event.target as HTMLInputElement).files = null; // Сброс выбранного файла
    }
  }

  // Закрытие диалогового окна без сохранения изменений
  cancel() {
    this.dialogRef.close();
  }
}
