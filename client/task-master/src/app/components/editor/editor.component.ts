import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { FormControl, FormGroup, FormsModule, NgModel } from '@angular/forms';
import { Editor, Toolbar, Validators } from 'ngx-editor';
import { NgxEditorModule, schema } from 'ngx-editor';

@Component({
  selector: 'app-editor',
  standalone: true,
  imports: [FormsModule, NgxEditorModule],
  templateUrl: './editor.component.html',
  styleUrl: './editor.component.css',
})
export class EditorComponent implements OnInit, OnDestroy {
  // Входное свойство для передачи HTML-кода в редактор
  @Input() html: string = '';

  // Экземпляр редактора
  editor: Editor;

  // Настройка панели инструментов редактора
  toolbar: Toolbar = [
    ['bold', 'italic'],
    ['underline', 'strike'],
    ['code', 'blockquote'],
    ['ordered_list', 'bullet_list'],
    [{ heading: ['h1', 'h2', 'h3', 'h4', 'h5', 'h6'] }],
    ['link', 'image'],
    ['text_color', 'background_color'],
    ['align_left', 'align_center', 'align_right', 'align_justify'],
  ];

  // Форма для управления содержимым редактора
  form = new FormGroup({
    editorContent: new FormControl('', Validators.required()),
  });

  ngOnInit(): void {
    // Создание экземпляра редактора при инициализации компонента
    this.editor = new Editor();
  }

  ngOnDestroy(): void {
    // Уничтожение экземпляра редактора при уничтожении компонента
    this.editor.destroy();

    // Удаление обработчиков событий при уничтожении компонента
    document.removeEventListener('click', this.handleClickOutsideEditor);
    document.removeEventListener('click', this.handleClickInsideEditor);
  }

  // Событие при потере фокуса редактором
  @Output() blur = new EventEmitter();

  constructor() {
    // Добавление обработчика события клика для редактора при создании компонента
    document.addEventListener('click', this.handleClickInsideEditor);
  }

  // Получение ссылки на DOM-элемент редактора
  @ViewChild('editorComponent') editorComponent: ElementRef;

  // Обработчик события клика вне области редактора
  handleClickOutsideEditor = (event: MouseEvent) => {
    const target = event.target as HTMLElement; // Получение целевого элемента
    if (document.body.contains(target)) {
      const editorElement = this.editorComponent.nativeElement;
      if (!editorElement.contains(target)) {
        // Удаление обработчика события клика вне области редактора
        document.removeEventListener('click', this.handleClickOutsideEditor);
        // Добавление обработчика события клика в области редактора
        document.addEventListener('click', this.handleClickInsideEditor);
        // Генерация события потери фокуса редактором
        this.blur.emit();
      }
    }
  };

  // Обработчик события клика внутри области редактора
  handleClickInsideEditor = (event: MouseEvent) => {
    const target = event.target as HTMLElement; // Получение целевого элемента
    if (document.body.contains(target)) {
      const editorElement = this.editorComponent.nativeElement;
      if (editorElement.contains(target)) {
        // Удаление обработчика события клика в области редактора
        document.removeEventListener('click', this.handleClickInsideEditor);
        // Добавление обработчика события клика вне области редактора
        document.addEventListener('click', this.handleClickOutsideEditor);
      }
    }
  };
}
