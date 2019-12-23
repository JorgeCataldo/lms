import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { CustomEmailPreview, UserInfoPreview } from 'src/app/models/previews/custom-email';
import * as Editor from 'tui-editor';

@Component({
  selector: 'app-custom-email-sent-info-dialog',
  template: `
    <div class="content" >
      <div class="inner-content">
        <div class="user-content">
          <h4>USUÁRIOS</h4>
          <div class="chips"
            *ngIf="process.users && process.users.length > 0" >
            <p *ngFor="let user of process.users" >
              <img class="logo" [src]="getImageUrl(user)" />
              {{ user.name }}
            </p>
          </div>
        </div>
        <div class="text-content">
          <h4>Conteúdo do E-mail</h4>
          <!--div id="htmlEditor" ></div-->
          <!--p style="text-align: left;">{{ process.text }}</p-->
          <div [innerHTML]="process.text"></div>
        </div>
      </div>
    </div>
    <p class="dismiss" (click)="dismiss()" >
      FECHAR
    </p>`,
  styleUrls: ['./custom-email-sent-info.dialog.scss']
})
export class CustomEmailSentInfoDialogComponent implements OnInit {

  public editor: Editor;

  constructor (
    public dialogRef: MatDialogRef<CustomEmailSentInfoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public process: CustomEmailPreview
  ) { }

  ngOnInit() {
    // this._configureEditor();
  }

  public getImageUrl(user: UserInfoPreview): string {
    return user.imageUrl ? user.imageUrl : './assets/img/user-image-placeholder.png';
  }

  public dismiss(): void {
    this.dialogRef.close();
  }

  private _configureEditor(): void {
    this.editor = new Editor({
      el: document.querySelector('#htmlEditor'),
      initialEditType: 'wysiwyg',
      hideModeSwitch: true,
      previewStyle: 'vertical',
      height: '200px'
    });

    this.editor.setMarkdown(
      this.process.text
    );
  }
}
