import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-error-modal',
  standalone: true,
  imports: [],
  templateUrl: './error-modal.component.html',
  styleUrl: './error-modal.component.css',
})
export class ErrorModalComponent {
  // Сообщение об ошибке
  message: string = '';

  // Конструктор с инъекцией диалогового окна и переданных данных
  constructor(
    private dialogRef: MatDialogRef<ErrorModalComponent>, // Доступ к экземпляру модального окна
    @Inject(MAT_DIALOG_DATA) private data: any // Доступ к переданным данным
  ) {
    this.message = data.message;
  }

  // Закрытие модального окна
  close() {
    this.dialogRef.close();
  }
}
