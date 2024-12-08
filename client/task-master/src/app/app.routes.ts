import { Routes } from '@angular/router';
import { EditBoardComponent } from './pages/edit-board/edit-board.component';
import { BoardsOverviewComponent } from './pages/boards-overview/boards-overview.component';
import { LoginComponent } from './pages/auth/login/login.component';
import { RegistrationComponent } from './pages/auth/register/registration.component';
import { ForgotPasswordComponent } from './pages/auth/forgot-password/forgot-password.component';
import { PageNotFoundComponent } from './pages/page-not-found/page-not-found.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'boards-overview',
    pathMatch: 'full',
  },
  {
    path: 'boards-overview',
    component: BoardsOverviewComponent,
  },
  {
    path: 'edit-board/:id',
    component: EditBoardComponent,
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'register',
    component: RegistrationComponent,
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordComponent,
  },
  {
    path: '**',
    component: PageNotFoundComponent,
  },
];
