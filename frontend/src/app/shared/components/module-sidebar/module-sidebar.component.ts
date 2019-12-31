import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Module } from '../../../models/module.model';
import { ForumQuestion } from 'src/app/models/forum.model';
import { ContentForumService } from 'src/app/pages/_services/forum.service';
import { NotificationClass } from '../../classes/notification';
import { MatSnackBar, MatDialog } from '@angular/material';
import { AuthService } from '../../services/auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { ForumQuestionDialogComponent } from '../../dialogs/forum-question/forum-question.dialog';
import * as pdfform from 'pdfform.js/pdfform';
import { UtilService } from 'src/app/shared/services/util.service';
import { HttpClient } from '@angular/common/http';
import { EventForumQuestionDialogComponent } from '../../dialogs/event-forum-question/event-forum-question.dialog';
import { FormGroup, FormControl, Validators, FormArray, AbstractControl } from '@angular/forms';
import { UploadResource } from '../../../models/shared/upload-resource.interface';
import { SharedService } from '../../../shared/services/shared.service';
import { SettingsUsersService } from '../../../settings/_services/users.service';
import { UserFile } from 'src/app/models/user-file.interface';
import { UserProfile } from 'src/app/settings/users/user-models/user';
import { SidebarEventApplicationNoteDialogComponent } from './sidebar-event-application-note/sidebar-event-application-note.dialog';

@Component({
  selector: 'app-module-sidebar',
  templateUrl: './module-sidebar.component.html',
  styleUrls: ['./module-sidebar.component.scss']
})
export class ModuleSidebarComponent extends NotificationClass {

  @Input() readonly module: any;
  @Input() readonly isEvent: boolean = false;
  @Input() readonly hasForum: boolean = true;
  @Input() readonly disabledQuestionBtn: boolean = false;
  @Input() readonly levelList: any;
  @Input() readonly forumQuestionsPreview: ForumQuestion[] = [];
  @Input() readonly moduleProgress: any;
  @Input() readonly eventApplication: any;
  @Output() reloadForumQuestionsPreview: EventEmitter<string> = new EventEmitter();

  private currentFileName: string = null;
  public formGroup: FormGroup;
  private selectedFile: File;
  public arquivos: Array<UserFile> = [];

  constructor(
    protected _snackBar: MatSnackBar,
    private _activatedRoute: ActivatedRoute,
    private _forumService: ContentForumService,
    private _authService: AuthService,
    private _router: Router,
    private _dialog: MatDialog,
    private _utilService: UtilService,
    private _httpClient: HttpClient,
    private _sharedService: SharedService,
    private _settingsUsersService: SettingsUsersService
  ) {
    super(_snackBar);
  }

  public manageLike(question: ForumQuestion): void {
    this._forumService.manageQuestionLike(
      question.id, question.liked
    ).subscribe(
      () => {
        const user = this._authService.getLoggedUser();
        question.liked ? question.likedBy.push(user.user_id) : question.likedBy.pop();
      },
      () => { this.notify('Ocorreu um erro, por favor tente novamente mais tarde'); }
    );
  }

  public goToForum() {
    if (this.module) {
      if (this.isEvent) {
        const eventScheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
        this._router.navigate([ '/forum-evento/' + this.module.id + '/' + eventScheduleId ]);
      } else {
        this._router.navigate([ '/forum/' + this.module.id ]);
      }
    }
  }

  public openQuestionModal() {
    if (this.isEvent) {
      const dialogRef = this._dialog.open(EventForumQuestionDialogComponent, {
        width: '1000px'
      });
      const eventScheduleId = this._activatedRoute.snapshot.paramMap.get('scheduleId');
      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result)
          this.reloadForumQuestionsPreview.emit(eventScheduleId);
      });
    } else {
      const dialogRef = this._dialog.open(ForumQuestionDialogComponent, {
        width: '1000px'
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result)
          this.reloadForumQuestionsPreview.emit(this.module.id);
      });
    }
  }

  public generateCertificatePDF(): void {
    this._httpClient.get(
      this.module.certificateUrl, { responseType: 'arraybuffer' }
    ).subscribe(
      (response) => {
        const fields = { };
        fields['nome_modulo'] = [ this.module.title ];
        fields['nome_aluno'] = [ this._authService.getLoggedUser().name ];
        fields['data_conclusao'] = [ this._utilService.formatDateToDDMMYYYY(new Date())];
        fields['data_conclusao_extenso'] = [ this._utilService.formatDateToName(new Date())];
        fields['nivel'] = [ this.levelList[this.moduleProgress.level - 1].description ];
        const out_buf = pdfform().transform(response, fields);

        const blob = new Blob([out_buf], { type: 'application/pdf' });
        const fileURL = URL.createObjectURL(blob);
        window.open(fileURL);
    }, () => {
      this.notify('Ocorreu um erro ao carregar o certificado');
    });
  }

  public openFileUpload(): void {
    (document.getElementById('inputFile') as HTMLElement).click();
  }

  public setDocumentFile(event, childFormGroup: FormGroup) {
    if (event.target && event.target.files && event.target.files.length > 0) {
      this.selectedFile = event.target.files[0];

      this._uploadFile(
        event.target.files[0],
        this.selectedFile.name
      );
    }
  }

  private _uploadFile(file, valueControl: string) {
    const callback = this._sendFileToServer.bind(this);
    const reader = new FileReader();
    reader.onloadend = function(e) {
      callback(
        this.result as string,
        file.name
      );
    };
    reader.readAsDataURL(file);
  }

  private _sendFileToServer(result: string, fileName: string) {
    const resource: UploadResource = {
      data: result,
      filename: fileName,
      resource: 'module_assesment'
    };

    this._sharedService.uploadFile(resource).subscribe((response) => {
      const arquivo: UserFile = {
        downloadLink: response.value.data.url,
        description: response.value.data.description || null,
        title: response.value.data.filename,
        id: null,
        resourceId: this.module.id
      };

      this.arquivos.push(arquivo);
      const user = this._authService.getLoggedUser();
      this._settingsUsersService.addAssesmentUserFiles(
        user.user_id,
        this.arquivos
      ).subscribe(() => {
        this.arquivos = [];
        this.notify('Documentos enviados com sucesso.');
      });
        }, () => {
          this.arquivos = [];
      this.notify('Ocorreu um erro ao enviar o arquivo, por favor tente novamente mais tarde');
    });
  }


  public openParticipation() {
    this._dialog.open(SidebarEventApplicationNoteDialogComponent, {
      width: '1000px',
      data: this.eventApplication.transcribedParticipation
    });
  }
}
