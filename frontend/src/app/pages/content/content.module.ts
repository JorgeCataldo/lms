import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ContentMenuComponent } from './common/menu/menu.component';
import { ContentComponent } from './content.component';
import { ProgressBarModule } from '../../shared/components/layout/progress-bar/progress-bar.module';
import { VideoContentComponent } from './video/video.component';
import { ContentFooterComponent } from './common/footer/footer.component';
import { ContentDescriptionComponent } from './common/description/description.component';
import { TextContentComponent } from './text/text.component';
import { PDFContentComponent } from './pdf/pdf.component';
import { PipesModule } from '../../shared/pipes/pipes.module';
import { MarkdownToHtmlModule } from 'markdown-to-html-pipe';
import { ForumQuestionDialogModule } from 'src/app/shared/dialogs/forum-question/forum-question.module';
import { EventForumQuestionDialogModule } from 'src/app/shared/dialogs/event-forum-question/event-forum-question.module';
import { MaterialComponentsModule } from '../../shared/material.module';
import { FormsModule } from '@angular/forms';
import { HTMLContentComponent } from './html/html.component';

@NgModule({
  declarations: [
    ContentComponent,
    ContentMenuComponent,
    VideoContentComponent,
    TextContentComponent,
    PDFContentComponent,
    ContentDescriptionComponent,
    ContentFooterComponent,
    HTMLContentComponent
  ],
  imports: [
    BrowserModule,
    ProgressBarModule,
    PipesModule,
    MarkdownToHtmlModule,
    ForumQuestionDialogModule,
    EventForumQuestionDialogModule,
    FormsModule,
    MaterialComponentsModule
  ],
  exports: [
    VideoContentComponent,
    TextContentComponent,
    PDFContentComponent,
    ContentDescriptionComponent,
    ContentFooterComponent,
    HTMLContentComponent
  ]
})
export class ContentModule { }
