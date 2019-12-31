import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ProgressBarModule } from '../layout/progress-bar/progress-bar.module';
import { ModuleSidebarComponent } from './module-sidebar.component';
import { SupportMaterialComponent } from './support-material/support-material.component';
import { DirectivesModule } from '../../directives/directives.module';
import { ForumQuestionCardModule } from 'src/app/pages/forum/question-card/question-card.module';
import { ForumQuestionDialogModule } from 'src/app/shared/dialogs/forum-question/forum-question.module';
import { EventForumQuestionDialogModule } from '../../dialogs/event-forum-question/event-forum-question.module';
import { SidebarEventApplicationNoteDialogComponent } from './sidebar-event-application-note/sidebar-event-application-note.dialog';

@NgModule({
  declarations: [
    ModuleSidebarComponent,
    SupportMaterialComponent,
    SidebarEventApplicationNoteDialogComponent
  ],
  imports: [
    BrowserModule,
    ProgressBarModule,
    DirectivesModule,
    ForumQuestionCardModule,
    ForumQuestionDialogModule,
    EventForumQuestionDialogModule
  ],
  exports: [
    ModuleSidebarComponent,
    SupportMaterialComponent
  ],
  entryComponents: [
    SidebarEventApplicationNoteDialogComponent
  ]
})
export class ModuleSidebarModule { }
