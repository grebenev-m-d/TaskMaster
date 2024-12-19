import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  Input,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { BoardComponent } from '../../components/board/board.component';
import { ActivatedRoute } from '@angular/router';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { EntityState } from '../../models/EntityState';
import { Board } from '../../models/Board';
import { BoardApiService } from '../../services/api.services/boardApi.service';
import { BoardHubService } from '../../services/hub.services/boardHub.service';
import { EditBoardModalComponent } from '../../modal/edit-board-modal/edit-board-modal.component';
import { DesignType } from '../../models/enum/DesignType';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { OwnBoardsComponent } from './own-boards/own-boards.component';
import { SharedBoardsComponent } from './shared-boards/shared-boards.component';

enum BoardType {
  ownedBoard,
  sharedBoard,
}

@Component({
  selector: 'app-boards-overview',
  standalone: true,
  imports: [
    OwnBoardsComponent,
    SharedBoardsComponent,
    FormsModule,
    CommonModule,
    HeaderComponent,
    BoardComponent,
  ],
  templateUrl: './boards-overview.component.html',
  styleUrl: './boards-overview.component.css',
})
export class BoardsOverviewComponent {
  // Текущий тип досок
  currentBoardsType: BoardType = BoardType.ownedBoard;

  // Перечисление типов досок
  BoardTypeEnum = BoardType;
}
