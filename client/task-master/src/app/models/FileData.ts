// Данные файла
export class FileData {
  // Имя файла
  fileName: string;
  // Данные файла
  data: Blob;

  // Конструктор данных файла
  constructor(fileName: string, data: Blob) {
    this.fileName = fileName;
    this.data = data;
  }
}
